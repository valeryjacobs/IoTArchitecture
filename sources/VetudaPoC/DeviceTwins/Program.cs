using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;

namespace DeviceTwins
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=vetuda.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=iRAW5aUFOgrGbqZ5JbnywfBA38SKIvI6ouRedcPeilY=";

        static string deviceId = "Vehicle_0";


        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddTagsAndQuery().Wait();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        public static async Task AddTagsAndQuery()
        {
            var twin = await registryManager.GetTwinAsync(deviceId);
            var patch =
                @"{
             tags: {
                 location: {
                     region: 'EU',
                     lon: '52.3054724',
                     lat: '4.7533562'
                 },
                 allocation:{fleet: 'AgriAutomotive_F12',
                     fleetId: 'F12C345'}
             }
         }";
            await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);

            var query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.allocation.fleetId = 'F12C345q'", 100);
            var twinsInFleet = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in F12C345: {0}", string.Join(", ", twinsInFleet.Select(t => t.DeviceId)));

            query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.allocation.fleetId = 'F12C345q' AND properties.reported.connectivity.type = 'cellular'", 100);
            var twinsInFleetUsingCellular = await query.GetNextAsTwinAsync();
            Console.WriteLine("Vehicles in EU using cellular network: {0}", string.Join(", ", twinsInFleetUsingCellular.Select(t => t.DeviceId)));

            query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.lon < 53 AND tags.location.lat < 5", 100);
            var twinsInNearFogZone = await query.GetNextAsTwinAsync();
            Console.WriteLine("Vehicles in fog zone: {0}", string.Join(", ", twinsInFleetUsingCellular.Select(t => t.DeviceId)));


        }
    }
}
