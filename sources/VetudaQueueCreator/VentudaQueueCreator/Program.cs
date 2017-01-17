using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using Newtonsoft.Json;
using System.Threading;

namespace VentudaQueueCreator
{
    [Serializable]
    public class CollectorJob
    {
        public Uri EndPoint;
        public string Query;
    }

    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
         {
            var cString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            queue.CreateIfNotExists();

            var j = 1;
            var nrCustomers = 16;
            var nrOfSignalValues = 200000;
            var signalValuesPerCustomer = nrOfSignalValues / nrCustomers;
            var pullingTimeSpan = TimeSpan.FromSeconds(5);

            while (true)
            {
                var expired = DateTime.UtcNow.Add(pullingTimeSpan);
                for (int c = 1; c <= nrCustomers; c++)
                {
                    var job = new CollectorJob { EndPoint = new Uri("http://ventudavibexdemo.azurewebsites.net"), Query = $"api/IOTObject?customer={c}&nrCars={signalValuesPerCustomer}&nrSignalValues=1" };
                    var json = JsonConvert.SerializeObject(job);
                    var message = new CloudQueueMessage(json);
                    queue.AddMessage(message);
                    Console.WriteLine($"Created Job {j} to {job.EndPoint}/{job.Query}");
                    j++;
                }

                var timeLeft = expired - DateTime.UtcNow;

                if (timeLeft.TotalMilliseconds > 0)
                {
                    Console.WriteLine($"Sleeping for: {timeLeft}");
                }

                while (DateTime.UtcNow < expired)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
