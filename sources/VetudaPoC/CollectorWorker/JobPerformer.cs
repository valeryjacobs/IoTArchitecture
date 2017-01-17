using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VentudaModels;

namespace CollectorWorker
{
    class JobPerformer
    {
        private readonly MeteredEventHubClient eventHubClient;

        private ServiceLogger Log { get; }

        private long JobCount { get; set; }

        public JobPerformer(ServiceLogger log)
        {
            var eventHubName = ConfigurationManager.AppSettings["EventHubName"];
            var connectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            var eventHub = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            eventHubClient = new MeteredEventHubClient(eventHub, 838861 * 11 / 10, TimeSpan.FromSeconds(1));
            Log = log;
            log.LogInfo("Exception", String.Empty);
        }

        public async Task<bool> PerformJob(CollectorJob job, CancellationToken cancellationToken)
        {
            var parameters = job.Query.Split('?')[1].Split('&');
            var customer = parameters[0].Split('=')[1];
            var nrCars = int.Parse(parameters[1].Split('=')[1]);
            var nrSignals = int.Parse(parameters[2].Split('=')[1]);

            var result = true;

            try
            {
                var signals = GetObjectSignals(customer, nrCars, nrSignals, cancellationToken);

                var batch = signals.Objects
                    .Select(car => car.Signals)
                    .Select(signal => new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(signal))));

                await eventHubClient.SendBatchAsync(batch, 225000, cancellationToken);
                JobCount++;
                Log.LogInfo("AverageBytes", $"Event Count: {eventHubClient.EventCount}. Bytes: {eventHubClient.BytesSent}. Target: {eventHubClient.AverageBytesPerSecond}.");
                Log.LogInfo("TotalEvents", $"{eventHubClient.TotalEventCount}");
                Log.LogInfo("AverageEvents", $"{eventHubClient.TotalEventCount / JobCount}");
                Log.LogInfo("JobCount", $"{JobCount}");
            }
            catch (Exception ex)
            {
                Log.LogWarning("Exception", ex.ToString());
                result = false;
            }

            return result;
        }

        private ObjectSignals GetObjectSignals(string customer, int nrCars, int nrSignals, CancellationToken cancellationToken)
        {
            Random rand = new Random();

            var signals = new ObjectSignals();
            for (int c = 0; c < nrCars; c++)
            {
                var obj = new IoTObject();
                obj.id = $"{customer}-{c}";

                int previousTemp = 30;
                for (int s = 0; s < nrSignals; s++)
                {
                    int change = rand.Next(11) - 5;
                    IOTObjectMetric iotObject = new IOTObjectMetric()
                    {
                        IOTObjectId = s.ToString(),
                        Value = previousTemp + change
                    };
                    obj.Signals.Add(iotObject);
                    previousTemp = iotObject.Value;
                }
                signals.Objects.Add(obj);
            }

            return signals;
        }
    }
}
