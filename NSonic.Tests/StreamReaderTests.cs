using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSonic.Tests
{
    [TestClass]
    public class StreamReaderTests
    {
        [TestMethod]
        public void ShouldNotTimeoutWhenNotDelay()
        {
            const int port = 11888;

            Task.Factory.StartNew(() =>
            {
                StartListening(port, false);
            }, TaskCreationOptions.LongRunning);

            using var search = NSonicFactory.Search("localhost", port, "SecretPassword");
            search.Connect();

            var timeout = Task.Run(() =>
            {
                var queryResults = search.Query("messages", "user:1", "s");
                Assert.IsTrue(queryResults.Length > 0);
            }).Wait(5000);

            Assert.IsTrue(timeout);
        }

        [TestMethod]
        public void ShouldNotTimeoutWhenDelay()
        {
            const int port = 11888;

            Task.Factory.StartNew(() =>
            {
                StartListening(port, true);
            }, TaskCreationOptions.LongRunning);

            using var search = NSonicFactory.Search("localhost", port, "SecretPassword");
            search.Connect();

            var timeout = Task.Run(() =>
            {
                var queryResults = search.Query("messages", "user:1", "s");
                Assert.IsTrue(queryResults.Length > 0);
            }).Wait(5000);

            Assert.IsTrue(timeout);
        }

        static void StartListening(int port, bool delay)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Start listening on {IPAddress.Any}:{port}.");

            using var client = listener.AcceptTcpClient();
            Console.WriteLine($"Connected {client.Client.RemoteEndPoint}.");

            using var reader = new StreamReader(client.GetStream());
            using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

            writer.WriteLine("CONNECTED <sonic-server v1.2.3>");

            while (true)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                if (line.Contains("QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    writer.WriteLine("ENDED quit");
                    break;
                }

                Console.WriteLine($"Received:{line}.");

                if (line.StartsWith("START"))
                {
                    writer.WriteLine("STARTED search protocol(1) buffer(20000)");
                }
                else if (line.StartsWith("QUERY"))
                {
                    writer.WriteLine("PENDING Bt2m2gYa");

                    if (delay)
                    {
                        Thread.Sleep(500); //this will lead to timeout
                    }
                    
                    writer.WriteLine("EVENT QUERY Bt2m2gYa conversation:71f3d63b");
                }
            }

            client.Close();
            listener.Stop();
            Console.WriteLine("Stop listening.");
        }
    }
}
