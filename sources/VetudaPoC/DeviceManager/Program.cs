using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace DeviceManager
{
    class Program
    {
        static RegistryManager registryManager;
        static string connString = "HostName=vetuda.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=iRAW5aUFOgrGbqZ5JbnywfBA38SKIvI6ouRedcPeilY=";
        static ServiceClient client;
        static JobClient jobClient;
        static string deviceId = "Vehicle_0";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connString);
            Console.WriteLine("Press ENTER to start.");
            Console.ReadLine();

            StartReboot().Wait();

            //StartFirmwareUpdate().Wait();


            QueryTwinRebootReported().Wait();
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task QueryTwinRebootReported()
        {
            Twin twin = await registryManager.GetTwinAsync(deviceId);
            Console.WriteLine(twin.Properties.Reported.ToJson());
        }

        public static async Task StartReboot()
        {
            client = ServiceClient.CreateFromConnectionString(connString);
            CloudToDeviceMethod method = new CloudToDeviceMethod("reboot");
            method.ResponseTimeout = TimeSpan.FromSeconds(30);

            CloudToDeviceMethodResult result = await client.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine("Invoked firmware update on device.");
        }

        public static async Task StartFirmwareUpdate()
        {
            client = ServiceClient.CreateFromConnectionString(connString);
            CloudToDeviceMethod method = new CloudToDeviceMethod("firmwareUpdate");
            method.ResponseTimeout = TimeSpan.FromSeconds(30);
            method.SetPayloadJson(
                @"{
             fwPackageUri : 'https://someurl'
         }");

            CloudToDeviceMethodResult result = await client.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine("Invoked firmware update on device.");
        }
    }
}
