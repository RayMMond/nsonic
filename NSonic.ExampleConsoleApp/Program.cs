using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NSonic.ExampleConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostname = "localhost";
            var port = 18111;
            var secret = "Secret";

            
            var task = StartListeningAsync(port, false); //change delay to false to trigger IOException

            try
            {
                using (var search = NSonicFactory.Search(hostname, port, secret))
                {
                    await search.ConnectAsync();

                    var queryResults = search.Query("messages", "user:1", "s");
                    Console.WriteLine($"QUERY: {string.Join(", ", queryResults)}");
                }

                await task;
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        static async Task StartListeningAsync(int port, bool delay)
        {
            await Task.Yield();

            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Start listening on {IPAddress.Any}:{port}.");

            using var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"Connected {client.Client.RemoteEndPoint}.");

            using var reader = new StreamReader(client.GetStream());
            using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

            await writer.WriteLineAsync("CONNECTED <sonic-server v1.2.3>");

            while (true)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                if (line.Contains("QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    await writer.WriteLineAsync("ENDED quit");
                    break;
                }

                Console.WriteLine($"Received:{line}.");

                if (line.StartsWith("START"))
                {
                    await writer.WriteLineAsync("STARTED search protocol(1) buffer(20000)");
                }
                else if (line.StartsWith("QUERY"))
                {
                    if (delay)
                    {
                        await writer.WriteLineAsync("PENDING Bt2m2gYa");
                        await Task.Delay(500);
                        await writer.WriteLineAsync("EVENT QUERY Bt2m2gYa conversation:71f3d63b");
                    }
                    else
                    {
                        await writer.WriteAsync("PENDING Bt2m2gYa\r\nEVENT QUERY Bt2m2gYa conversation:71f3d63b\r\n");
                    }
                }
            }

            client.Close();
            listener.Stop();
            Console.WriteLine("Stop listening.");
        }
    }
}
