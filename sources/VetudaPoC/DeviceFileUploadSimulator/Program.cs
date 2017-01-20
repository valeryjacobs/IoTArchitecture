using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Microsoft.Azure.Devices.Client;

namespace DeviceFileUploadSimulator
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "vetuda.azure-devices.net";
        static string deviceKey = "dbXqDmDOU9903d4xseWUTH7pazl2Z41igUaFKC1Pl28=";
        static string deviceId = "Vehicle_0";


        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device file upload\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

            SendToBlobAsync();

            Console.ReadLine();
        }

        private static async void SendToBlobAsync()
        {
            string fileName = "UploadFile.json";
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(@"UploadFile.json", FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(fileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }
    }
}
