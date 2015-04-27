﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using JetBlack.Examples.RxSocket;

namespace JetBlack.Examples.RxSocketClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            var endpoint = new IPEndPoint(IPAddress.Parse(splitArgs[0]), int.Parse(splitArgs[1]));

            var cts = new CancellationTokenSource();
            var bufferManager = BufferManager.CreateBufferManager(2 << 16, 2 << 8);

            var frameClientSubject = endpoint.ToFrameClientSubject(SocketFlags.None, bufferManager, cts.Token);

            var observerDisposable =
                frameClientSubject
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(
                        managedBuffer =>
                        {
                            Console.WriteLine("Read: " + Encoding.UTF8.GetString(managedBuffer.Bytes, 0, managedBuffer.Length));
                            managedBuffer.Dispose();
                        },
                        error => Console.WriteLine("Error: " + error.Message),
                        () => Console.WriteLine("OnCompleted: FrameReceiver"));

            Console.In.ToLineObservable()
                .Subscribe(
                    line =>
                    {
                        var writeBuffer = Encoding.UTF8.GetBytes(line);
                        frameClientSubject.OnNext(new ManagedBuffer(writeBuffer, writeBuffer.Length, null));
                    },
                    error => Console.WriteLine("Error: " + error.Message),
                    () => Console.WriteLine("OnCompleted: LineReader"));

            observerDisposable.Dispose();

            cts.Cancel();
        }
    }
}
