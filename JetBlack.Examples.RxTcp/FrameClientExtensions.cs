using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.RxTcp
{
    public static class FrameClientExtensions
    {
        public static ISubject<DisposableBuffer, DisposableBuffer> ToFrameClientSubject(this IPEndPoint endpoint, BufferManager bufferManager, CancellationToken token)
        {
            var client = new TcpClient();
            client.Connect(endpoint);
            return client.ToFrameClientSubject(bufferManager, token);
        }

        public static ISubject<DisposableBuffer, DisposableBuffer> ToFrameClientSubject(this TcpClient client, BufferManager bufferManager, CancellationToken token)
        {
            return Subject.Create(client.ToFrameClientObserver(token), client.ToFrameClientObservable(bufferManager));
        }

        public static IObservable<DisposableBuffer> ToFrameClientObservable(this TcpClient client, BufferManager bufferManager)
        {
            return client.GetStream().ToFrameStreamObservable(bufferManager);
        }

        public static IObserver<DisposableBuffer> ToFrameClientObserver(this TcpClient client, CancellationToken token)
        {
            return client.GetStream().ToFrameStreamObserver(token);
        }
    }
}
