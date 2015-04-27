using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.RxSocket
{
    public static class ReactiveEx
    {
        public static IObservable<Socket> ToListener(this Socket socket, int backlog)
        {
            return Observable.Create<Socket>(async (observer, token) =>
            {
                socket.Listen(10);

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
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static ISubject<ArraySegment<byte>, ArraySegment<byte>> ToSenderReceiver(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Subject.Create(socket.ToSender(size, socketFlags), socket.ToReceiver(size, socketFlags));
        }

        public static IObservable<ArraySegment<byte>> ToReceiver(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Observable.Create<ArraySegment<byte>>(async (observer, token) =>
            {
                var buffer = new byte[size];

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var received = await socket.ReceiveAsync(buffer, 0, size, socketFlags);
                        if (received == 0)
                            break;
                        observer.OnNext(new ArraySegment<byte>(buffer, 0, received));
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObservable<ManagedBuffer> ToFrameReceiver(this Socket socket, SocketFlags socketFlags, BufferManager bufferManager)
        {
            return Observable.Create<ManagedBuffer>(async (observer, token) =>
            {
                var headerBuffer = new byte[sizeof(int)];

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (await socket.ReceiveCompletelyAsync(headerBuffer, headerBuffer.Length, socketFlags, token) != headerBuffer.Length)
                            break;
                        var length = BitConverter.ToInt32(headerBuffer, 0);

                        var buffer = bufferManager.TakeBuffer(length);
                        if (await socket.ReceiveCompletelyAsync(buffer, length, socketFlags, token) != length)
                            break;

                        observer.OnNext(new ManagedBuffer(buffer, length, bufferManager));
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObserver<ArraySegment<byte>> ToSender(this Socket socket, int size, SocketFlags socketFlags)
        {
            return Observer.Create<ArraySegment<byte>>(async arraySegment =>
            {
                var sent = 0;
                while (sent < arraySegment.Count)
                    sent += await socket.SendAsync(arraySegment.Array, arraySegment.Offset + sent, arraySegment.Count - sent, socketFlags);
            });
        }

        public static IObserver<ManagedBuffer> ToFrameSender(this Socket socket, SocketFlags socketFlags, CancellationToken token)
        {
            return Observer.Create<ManagedBuffer>(async managedBuffer =>
                {
                    await socket.SendCompletelyAsync(BitConverter.GetBytes(managedBuffer.Length), sizeof(int), socketFlags, token);
                    await socket.SendCompletelyAsync(managedBuffer.Bytes, managedBuffer.Length, socketFlags, token);
                });
        }
    }
}
