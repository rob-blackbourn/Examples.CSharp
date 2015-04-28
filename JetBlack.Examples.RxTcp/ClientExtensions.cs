using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;

namespace JetBlack.Examples.RxTcp
{
    public static class ClientExtensions
    {
        public static ISubject<Buffer, Buffer> ToClientSubject(this TcpClient client, int size, CancellationToken token)
        {
            return Subject.Create(client.ToClientObserver(token), client.ToClientObservable(size));
        }

        public static IObservable<Buffer> ToClientObservable(this TcpClient client, int size)
        {
            return client.GetStream().ToStreamObservable(size);
        }

        public static IObserver<Buffer> ToClientObserver(this TcpClient client, CancellationToken token)
        {
            return client.GetStream().ToStreamObserver(token);
        }
    }
}
