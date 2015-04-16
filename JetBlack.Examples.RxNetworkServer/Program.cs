﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Examples.RxNetwork;

namespace JetBlack.Examples.RxNetworkServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: NetworkServer <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > NetworkServer 127.0.0.1:9211");
                Environment.Exit(-1);
            }

            var address = IPAddress.Parse(splitArgs[0]);
            var port = int.Parse(splitArgs[1]);

            var cts = new CancellationTokenSource();

            Task.Run(() => new TcpListener(address, port).Listen(TaskPoolScheduler.Default, cts.Token), cts.Token);

            Console.WriteLine("Press <ENTER> to quit");
            Console.ReadLine();

            cts.Cancel();
        }
    }
}
