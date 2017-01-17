using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;

namespace C2DClient
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=vetuda.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=iRAW5aUFOgrGbqZ5JbnywfBA38SKIvI6ouRedcPeilY=";

        static void Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            ReceiveFeedbackAsync();
        
            Console.WriteLine("Press any key to send a C2D message.");
            while (true)
            {
                SendCloudToDeviceMessageAsync(Console.ReadLine()).Wait();
            }
        }
    
        private async static Task SendCloudToDeviceMessageAsync(string content)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(content));

            commandMessage.Ack = DeliveryAcknowledgement.Full;

            await serviceClient.SendAsync("myFirstDevice", commandMessage);
        }

        private async static void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
    }
}
