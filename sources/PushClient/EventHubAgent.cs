using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Vetuda.DatabaseAccess.Interfaces;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;

namespace EventHubDataPush
{
    public class EventHubAgent : IEventHubAgent
    {
        public class SignalValue
        {
            public string Vin { get; set; }
            public int SignalId { get; set; }
            public DateTime Time { get; set; }
            public string Location { get; set; }
            public string Value { get; set; }
        }

        private readonly string mEventhubName;
        private readonly string mConnectionString;

        public EventHubAgent(IDatabaseAccessFacade databaseAccessFacade, string eventhubName, string connectionString )
        {
            databaseAccessFacade.DatabaseActionOcurred += DatabaseAccessFacade_DatabaseActionOcurred;
            mEventhubName = eventhubName;
            mConnectionString = connectionString;
        }

        private void DatabaseAccessFacade_DatabaseActionOcurred(object sender, DatabaseActionEventArgs e)
        {
            Console.WriteLine($"database action occurred. Sending {e.Values.Count} to eventHub");
            Stopwatch s = new Stopwatch();
            s.Start();
            var values = e.Values.Select(v => new SignalValue { Vin = v.Vin, Location = v.Location, SignalId = v.SignalId, Time = v.Time, Value = v.Value }).ToList();
            var eventHubClient = EventHubClient.CreateFromConnectionString(mConnectionString, mEventhubName);

            var batch = new List<EventData>();

            var maxBatchSize = 200000;

            long totalCurrentSize = 0;
            int i = 0;
            foreach (var value in values)
            {
                i++;
                var serializedSignalValue = JsonConvert.SerializeObject(value);
                var x = new EventData(Encoding.UTF8.GetBytes(serializedSignalValue));
                totalCurrentSize += x.SerializedSizeInBytes;
                if (totalCurrentSize > maxBatchSize)
                {
                    Console.WriteLine($"Sending batch to eventHub. batch size: {i}");
                    SendBatchToEventHub(batch, eventHubClient);
                    totalCurrentSize = 0;
                    batch.Clear();
                    i = 0;
                }

                batch.Add(x);
            }

            Console.WriteLine($"Sending batch to eventHub. batch size: {i}");
            SendBatchToEventHub(batch, eventHubClient);
            Console.WriteLine($"done sending batches, time spent sending: {s.ElapsedMilliseconds} milliseconds");
        }

        public static void SendBatchToEventHub(List<EventData> batch, EventHubClient eventHubClient)
        {
            eventHubClient.SendBatch(batch);
        }

    }
}

