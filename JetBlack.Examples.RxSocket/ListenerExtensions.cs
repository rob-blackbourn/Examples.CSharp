﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;

namespace JetBlack.Examples.RxSocket
{
    public static class ListenerExtensions
    {
        public static IObservable<Socket> ToListenerObservable(this IPEndPoint endpoint, int backlog)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            return socket.ToListenerObservable(10);
        }

        public static IObservable<Socket> ToListenerObservable(this Socket socket, int backlog)
        {
            return Observable.Create<Socket>(async (observer, token) =>
            {
                socket.Listen(backlog);

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var client = await socket.AcceptAsync();
                        if (client == null)
                            break;

                        observer.OnNext(client);
                    }

                    observer.OnCompleted();

                    socket.Close();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }
    }
}
