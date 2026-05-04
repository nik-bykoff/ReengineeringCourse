using System;
using System.Threading.Tasks;

namespace EchoTcpServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            IEchoServer server = new EchoServer(5000);

            _ = Task.Run(server.StartAsync);

            const string host = "127.0.0.1";
            const int port = 60000;
            const int intervalMilliseconds = 5000;

            using var sender = new UdpTimedSender(host, port);

            Console.WriteLine("Press any key to stop sending...");
            sender.StartSending(intervalMilliseconds);

            Console.WriteLine("Press 'q' to quit...");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
            {
            }

            sender.StopSending();
            server.Stop();
            Console.WriteLine("Sender stopped.");

            await Task.CompletedTask;
        }
    }
}
