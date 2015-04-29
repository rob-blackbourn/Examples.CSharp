﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace JetBlack.Examples.SelectSocket
{
    public static class SocketEx
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

        public static void MakeSocketPair(out Socket local, out Socket remote)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);
            local = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            local.Connect(listener.LocalEndPoint);
            remote = listener.Accept();
            listener.Close();
        }
    }
}
