#r "Newtonsoft.Json"
#r "Microsoft.ServiceBus"

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.ServiceBus.Messaging; 
using System.Text;


public static void Run(string myQueueItem, TraceWriter log, out string outputBlob)
{
    
    var json = myQueueItem;
    var job = JsonConvert.DeserializeObject<CollectorJob>(json); 
    log.Info($"Working on {job.EndPoint}/{job.Query}");

    var eventHubName = "data";
    var connectionString = "Endpoint=sb://ventudadata.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=JnJZzHgeSU57ABNT1u+QTHT43g1TdZAhkd6guDESCAg=";
    var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);

    var result = Task.Run(() => PerformJob(job)).Result; 
    outputBlob = result;
    var signalList  = JsonConvert.DeserializeObject<Measurements>(result); 

    var batch = new List<EventData>();

    var maxBatchSize = 200000;

    long totalCurrentSize = 0;

    foreach(SignalValue obj in signalList.SignalValues)
    {
        var serializedIOTObject = JsonConvert.SerializeObject(obj);
        var x = new EventData(Encoding.UTF8.GetBytes(serializedIOTObject));
        totalCurrentSize += x.SerializedSizeInBytes;
        if (totalCurrentSize > maxBatchSize)
        {
            SendBatchToEventHub(batch, eventHubClient);
            totalCurrentSize = 0;
            batch.Clear();
        }
       
        batch.Add(x);
    }

    SendBatchToEventHub(batch, eventHubClient);
}

public static void SendToEventHub(SignalValue value, EventHubClient eventHubClient)
{
   var serializedIOTObject = JsonConvert.SerializeObject(value);
   eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(serializedIOTObject)));
}

public static void SendBatchToEventHub(List<EventData> batch, EventHubClient eventHubClient)
{
    eventHubClient.SendBatch(batch);
}

public async static Task<string> PerformJob(CollectorJob job) 
{
    try
    {
        string uri = $"{job.EndPoint}{job.Query}";
        var client = new HttpClient();
        client.BaseAddress = job.EndPoint;
        var response = await client.GetAsync(job.Query);
        var json = await response.Content.ReadAsStringAsync();
        return json;
    }
    catch(Exception ex)
    {
        return null;
    }
}

public class CollectorJob
{
    public Uri EndPoint;
    public string Query;
}

public class SignalValue
{
    public string Fleet { get; set; }
    public string Vin { get; set; }
    public int SignalId { get; set; }
    public DateTime Time { get; set; }
    public string Location { get; set; }
    public string Value { get; set; }
}

public class Measurements
{
    public Measurements()
    {
        this.SignalValues = new List<SignalValue>();
    }
    public List<SignalValue> SignalValues { get; set; }
}