using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace JetBlack.Examples.RxSocket
{
    public static class ClientExtensions
    {
        public static ISubject<Buffer, Buffer> ToClientSubject(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Subject.Create(socket.ToClientObserver(size, socketFlags), socket.ToClientObservable(size, socketFlags));
        }

        public static IObservable<Buffer> ToClientObservable(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Observable.Create<Buffer>(async (observer, token) =>
            {
                var buffer = new byte[size];

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var received = await socket.ReceiveAsync(buffer, 0, size, socketFlags);
                        if (received == 0)
                            break;

                        observer.OnNext(new Buffer(buffer, received));
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObserver<Buffer> ToClientObserver(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Observer.Create<Buffer>(async buffer =>
            {
                var sent = 0;
                while (sent < buffer.Length)
                    sent += await socket.SendAsync(buffer.Bytes, sent, buffer.Length - sent, socketFlags);
            });
        }
    }
}
