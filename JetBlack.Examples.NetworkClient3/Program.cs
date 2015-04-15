using System;

namespace JetBlack.Examples.NetworkClient3
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            using (var client = new NetworkClient())
            {
                client.Dispatch(splitArgs[0], int.Parse(splitArgs[1])).Wait();
            }
        }
    }
}
