﻿using System;
using System.Diagnostics;

namespace NSonic.ExampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostname = "localhost";
            var port = 1491;
            var secret = "SecretPassword";

            try
            {
                using (var control = NSonic.Control(hostname, port, secret))
                {
                    control.Connect();

                    var info = control.Info();
                    Console.WriteLine(info);
                }

                using (var search = NSonic.Search(hostname, port, secret))
                {
                    search.Connect();

                    var result = search.Query("messages", "user:1", "s");
                    Console.WriteLine($"Query: {string.Join(", ", result)}");

                    var result2 = search.Suggest("messages", "user:1", "s");
                    Console.WriteLine($"Suggest: {string.Join(", ", result2)}");
                }

                using (var ingest = NSonic.Ingest(hostname, port, secret))
                {
                    ingest.Connect();

                    var result = ingest.Count("messages", "user:1");
                    Console.WriteLine($"Count for `messages user:1`: {result}");
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
