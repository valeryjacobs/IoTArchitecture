using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorWorker
{
    class ServiceLogger
    {
        private readonly StatelessService _service;
        private readonly FabricClient _client;

        public ServiceLogger(StatelessService service)
        {
            _service = service;
            _client = new FabricClient();
        }

        internal void LogMessage(string message)
        {
            ServiceEventSource.Current.ServiceMessage(_service, message);
        }

        internal void LogWarning(string identifier, string message)
        {
            LogReport(identifier, message, HealthState.Warning, TimeSpan.FromSeconds(30));
        }
        
        internal void LogInfo(string identifier, string message)
        {
            LogReport(identifier, message, HealthState.Ok, TimeSpan.Zero);
        }

        private void LogReport(string identifier, string message, HealthState state, TimeSpan lifeTime)
        {
            var health = _client.HealthManager;
            var details = new HealthInformation("Vetuda", identifier, state);
            details.Description = message;
            details.RemoveWhenExpired = true;

            if (lifeTime > TimeSpan.Zero)
            {
                details.TimeToLive = lifeTime;
            }

            health.ReportHealth(new StatelessServiceInstanceHealthReport(_service.Context.PartitionId, _service.Context.InstanceId, details));
        }
    }
}

