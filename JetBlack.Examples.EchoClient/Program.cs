using System;
using System.Net.Sockets;
using System.IO;

namespace JetBlack.Examples.EchoClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[]{':'}, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            var client = new TcpClient(splitArgs[0], int.Parse(splitArgs[1]));

            using (var stream = client.GetStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        string line;
                        while ((line = Console.ReadLine()).Length != 0)
                        {
                            writer.WriteLine(line);
                            Console.WriteLine(reader.ReadLine());
                        }
                    }
                }
            }

            client.Close();
        }
    }
}
