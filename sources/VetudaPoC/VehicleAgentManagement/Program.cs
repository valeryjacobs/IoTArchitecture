using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace VehicleAgentManagement
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=vetuda.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=iRAW5aUFOgrGbqZ5JbnywfBA38SKIvI6ouRedcPeilY=";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            for(int i =0; i<1; i++)
            {
                AddDeviceAsync("Vehicle_"+i.ToString()).Wait();
            }
           
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
