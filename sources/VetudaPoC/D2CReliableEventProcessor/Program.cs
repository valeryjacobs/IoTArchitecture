using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ServiceBus.Messaging;

namespace D2CReliableEventProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotHubConnectionString = "HostName=vetuda.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=iRAW5aUFOgrGbqZ5JbnywfBA38SKIvI6ouRedcPeilY=";
            string iotHubD2cEndpoint = "messages/events";
            ReliableEventProcessor.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=vetudainteractiveevents;AccountKey=sIOz5weuiPvMcxI7SlodGWqrhw+EzH/yD6zCbcDPYtWsK6ByPaa/XfYVYK3u+6tiPd92wQTz4HShtlGnfxh7ig==;";
            ReliableEventProcessor.ServiceBusConnectionString = "Endpoint=sb://vetudainteractivemessaging.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=eTnNIxI495/fiqfxq1Jp8bkX2/nbW3kLhxdz95nfwxw=;EntityPath=d2cinteractive";

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, ReliableEventProcessor.StorageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<ReliableEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
