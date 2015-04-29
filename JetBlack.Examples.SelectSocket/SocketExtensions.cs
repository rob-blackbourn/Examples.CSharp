using System;
using System.IO;
using System.Net.Sockets;

namespace JetBlack.Examples.SelectSocket
{
    public static class SocketExtensions
    {
        public static bool Send(this Socket socket, SocketFlags socketFlags, BufferState state)
        {
            try
            {
                state.Advance(socket.Send(state.Bytes, state.Offset, state.Length, socketFlags));
                return state.Length == 0;
            }
            catch (Exception exception)
            {
                if (IsWouldBlock(exception))
                    return false;
                throw;
            }
        }

        public static bool IsWouldBlock(this Exception exception)
        {
            var ioException = exception as IOException;
            var socketException = ioException == null ? exception as SocketException : ioException.InnerException as SocketException;
            return socketException != null && socketException.SocketErrorCode == SocketError.WouldBlock;
        }
    }
}
