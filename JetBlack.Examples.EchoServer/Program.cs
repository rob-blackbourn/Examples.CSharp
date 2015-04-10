using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace JetBlack.Examples.EchoServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[]{':'}, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoServer <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoServer 127.0.0.1:9211");
                Environment.Exit(-1);
            }

            var address = IPAddress.Parse(splitArgs[0]);
            var port = int.Parse(splitArgs[1]);
            var listener = new TcpListener(address, port);
            listener.Start();

            var cts = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var client = listener.AcceptTcpClient();

                        Task.Factory.StartNew(() =>
                            {
                                using (var stream = client.GetStream())
                                {
                                    using (var reader = new StreamReader(stream))
                                    {
                                        using (var writer = new StreamWriter(stream) { AutoFlush = true })
                                        {
                                            while (!cts.Token.IsCancellationRequested)
                                                writer.WriteLine(reader.ReadLine());
                                            client.Close();
                                        }
                                    }
                                }
                            }, cts.Token);
                    }
                }, cts.Token);

            Console.WriteLine("Press <ENTER> to quit");
            Console.ReadLine();

            listener.Stop();
            cts.Cancel();
            task.Wait(cts.Token);
        }
    }
}
