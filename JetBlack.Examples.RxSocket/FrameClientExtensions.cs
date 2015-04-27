using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.RxSocket
{
    public static class FrameClientExtensions
    {
        public static ISubject<ManagedBuffer, ManagedBuffer> ToFrameClientSubject(this IPEndPoint endpoint, SocketFlags socketFlags, BufferManager bufferManager, CancellationToken token)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            return socket.ToFrameClientSubject(socketFlags, bufferManager, token);
        }

        public static ISubject<ManagedBuffer, ManagedBuffer> ToFrameClientSubject(this Socket socket, SocketFlags socketFlags, BufferManager bufferManager, CancellationToken token)
        {
            return Subject.Create(socket.ToFrameClientObserver(socketFlags, token), socket.ToFrameClientObservable(socketFlags, bufferManager));
        }

        public static IObservable<ManagedBuffer> ToFrameClientObservable(this Socket socket, SocketFlags socketFlags, BufferManager bufferManager)
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

                    socket.Close();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObserver<ManagedBuffer> ToFrameClientObserver(this Socket socket, SocketFlags socketFlags, CancellationToken token)
        {
            return Observer.Create<ManagedBuffer>(async managedBuffer =>
            {
                await socket.SendCompletelyAsync(BitConverter.GetBytes(managedBuffer.Length), sizeof(int), socketFlags, token);
                await socket.SendCompletelyAsync(managedBuffer.Bytes, managedBuffer.Length, socketFlags, token);
            });
        }
    }
}
