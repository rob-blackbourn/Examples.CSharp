using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketClient2
{
    public static class SocketExtensions
    {
        public static async Task<Socket> AcceptAsync(this Socket socket)
        {
            return await Task<Socket>.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
        }

        public static async Task ConnectAsync(this Socket socket, string host, int port)
        {
            await Task.Factory.FromAsync((callback, state) => socket.BeginConnect(host, port, callback, state), ias => socket.EndConnect(ias), null);
        }

        public static async Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginSend(buffer, offset, size, flags, callback, state), ias => socket.EndSend(ias), null);
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int count, SocketFlags socketFlags)
        {
            return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginReceive(buffer, offset, count, socketFlags, callback, state), ias => socket.EndReceive(ias), null);
        }
    }
}
