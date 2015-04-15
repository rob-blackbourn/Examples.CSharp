using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkServer1
{
    class NetworkServer : IDisposable
    {
        private readonly TcpListener _tcpListener;

        public NetworkServer(IPAddress address, int port)
        {
            _tcpListener = new TcpListener(address, port);
        }

        public void Dispatch(CancellationToken token)
        {
            _tcpListener.Start();
            while (!token.IsCancellationRequested)
            {
                var client = _tcpListener.AcceptTcpClient();

                Task.Factory.StartNew(() =>
                {
                    using (var echoClient = new ServerNetworkClient(client))
                        echoClient.Dispatch(token);
                }, token);
            }
        }

        public void Dispose()
        {
            _tcpListener.Stop();
        }
    }
}