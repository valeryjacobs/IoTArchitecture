using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CollectorWorker : StatelessService
    {
        private readonly Random rand;
        private readonly JobPerformer performer;
        private readonly ServiceLogger log;

        public CollectorWorker(StatelessServiceContext context)
            : base(context)
        {
            rand = new Random();
            log = new ServiceLogger(this);
            performer = new JobPerformer(log);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            QueueReader queue = new QueueReader(log, performer);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await queue.ReadItem(cancellationToken))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    log.LogWarning("Exception", ex.ToString());
                }
            }
        }
    }
}