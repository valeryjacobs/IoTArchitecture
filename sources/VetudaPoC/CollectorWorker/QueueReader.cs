using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorWorker
{
    class QueueReader
    {
        private readonly CloudQueue _queue;
        private readonly JobPerformer _performer;
        private readonly ServiceLogger _log;
        private readonly AverageStopWatch _watch;

        public QueueReader(ServiceLogger log, JobPerformer performer)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            
            // Create the queue client
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            var queueName = ConfigurationManager.AppSettings["QueueName"];
            // Retrieve a reference to a queue
            _queue = queueClient.GetQueueReference(queueName);
            _performer = performer;
            _watch = new AverageStopWatch();
            _log = log;
            log.LogInfo("QueueName", queueName);
        }

        public async Task<bool> ReadItem(CancellationToken cancellationToken)
        {
            await _queue.CreateIfNotExistsAsync();

            // Get the next message
            CloudQueueMessage retrievedMessage = await _queue.GetMessageAsync(cancellationToken);
            if (retrievedMessage != null)
            {
                try
                {
                    _watch.Start();
                    var json = retrievedMessage.AsString;
                    var job = JsonConvert.DeserializeObject<CollectorJob>(json);
                    _log.LogMessage($"Starting Job {job.EndPoint}{job.Query}");
                    var success = await _performer.PerformJob(job, cancellationToken);

                    if (success)
                    {
                        await _queue.DeleteMessageAsync(retrievedMessage, cancellationToken);
                    }
                }
                finally
                {
                    _watch.Stop();
                    _log.LogInfo("ExecutionTime", _watch.AverageExecutionInSeconds.ToString());
                }
            }

            return retrievedMessage != null;
        }

        private class AverageStopWatch
        {
            private Stopwatch Watch { get; }

            private int Executions { get; set; }

            public TimeSpan AverageExecutionInSeconds
            {
                get
                {
                    return TimeSpan.FromSeconds(Watch.Elapsed.TotalSeconds / Executions);
                }
            }

            public AverageStopWatch()
            {
                Watch = new Stopwatch();
            }

            public void Start()
            {
                Executions++;
                Watch.Start();
            }

            public void Stop()
            {
                Watch.Stop();
            }
        }
    }
}
