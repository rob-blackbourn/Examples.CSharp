using System.Net.Sockets;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketServer2
{
    public static class SocketExtensions
    {
        public static async Task<Socket> AcceptAsync(this Socket socket)
        {
            return await Task<Socket>.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
        }

        //public static async Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int count, SocketFlags socketFlags)
        //{
        //    return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginSend(buffer, offset, count, socketFlags, callback, state), socket.EndSend, null);
        //}

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
