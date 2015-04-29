using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace JetBlack.Examples.SelectSocket
{
    class Selectable
    {
        private readonly object _gate = new object();
        private readonly IDictionary<Socket, Action<Socket>> _readCallbacks = new Dictionary<Socket, Action<Socket>>();
        private readonly IDictionary<Socket, Queue<Action<Socket>>> _writeCallbacks = new Dictionary<Socket, Queue<Action<Socket>>>();
        private readonly IDictionary<Socket, Action<Socket>> _errorCallbacks = new Dictionary<Socket, Action<Socket>>();
        private readonly Socket _reader, _writer;
        private readonly byte[] _readerBuffer = new byte[1024];
        private readonly byte[] _writeBuffer = { 0 };

        public Selectable()
        {
            MakeSocketPair(out _reader, out _writer);
            AddCallback(SelectMode.SelectRead, _reader, _ => _reader.Receive(_readerBuffer));
        }

        public void AddCallback(SelectMode mode, Socket socket, Action<Socket> callback)
        {
            lock (_gate)
            {
                switch (mode)
                {
                    case SelectMode.SelectRead:
                        _readCallbacks.Add(socket, callback);
                        break;
                    case SelectMode.SelectWrite:
                        Queue<Action<Socket>> callbackQueue;
                        if (!_writeCallbacks.TryGetValue(socket, out callbackQueue))
                            _writeCallbacks.Add(socket, callbackQueue = new Queue<Action<Socket>>());
                        callbackQueue.Enqueue(callback);
                        break;
                    case SelectMode.SelectError:
                        _errorCallbacks.Add(socket, callback);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }

                // If we have changed the selectable sockets interup the select to wait on the new sockets.
                if (socket != _reader)
                    InterruptSelect();
            }
        }

        public void RemoveCallback(SelectMode mode, Socket socket)
        {
            lock (_gate)
            {
                switch (mode)
                {
                    case SelectMode.SelectRead:
                        _readCallbacks.Remove(socket);
                        break;
                    case SelectMode.SelectWrite:
                        var callbackQueue = _writeCallbacks[socket];
                        callbackQueue.Dequeue();
                        if (callbackQueue.Count == 0)
                            _writeCallbacks.Remove(socket);
                        break;
                    case SelectMode.SelectError:
                        _errorCallbacks.Remove(socket);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
            }
        }

        private void InterruptSelect()
        {
            // Sending a byte to the writer wakes up the select loop.
            _writer.Send(_writeBuffer);
        }

        private List<SocketCallback> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Action<Socket>> dictionary)
        {
            var actions = new List<SocketCallback>();

            if (sockets == null) return actions;

            lock (_gate)
            {
                foreach (var socket in sockets)
                {
                    Action<Socket> action;
                    if (dictionary.TryGetValue(socket, out action))
                        actions.Add(new SocketCallback(socket, action));
                }
            }

            return actions;
        }

        private List<SocketCallback> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Queue<Action<Socket>>> dictionary)
        {
            var actions = new List<SocketCallback>();

            if (sockets == null) return actions;

            lock (_gate)
            {
                foreach (var socket in sockets)
                {
                    Queue<Action<Socket>> queue;
                    if (dictionary.TryGetValue(socket, out queue))
                        actions.Add(new SocketCallback(socket, queue.Peek()));
                }
            }

            return actions;
        }

        public Checkable CreateCheckable()
        {
            lock (_gate)
            {
                // When there are no sockets we cannot pass an empty list, we must pass null.
                return new Checkable(
                    _readCallbacks.Count == 0 ? null : _readCallbacks.Keys.ToList(),
                    _writeCallbacks.Count == 0 ? null : _writeCallbacks.Keys.ToList(),
                    _errorCallbacks.Count == 0 ? null : _errorCallbacks.Keys.ToList());
            }
        }

        public void InvokeCallbacks(Checkable checkable)
        {
            if (checkable.IsEmpty)
                return;

            CollectSockets(checkable.CheckRead, _readCallbacks).ForEach(pair => pair.Callback(pair.Socket));
            CollectSockets(checkable.CheckWrite, _writeCallbacks).ForEach(pair => pair.Callback(pair.Socket));
            CollectSockets(checkable.CheckError, _errorCallbacks).ForEach(pair => pair.Callback(pair.Socket));

        }

        struct SocketCallback
        {
            public readonly Socket Socket;
            public readonly Action<Socket> Callback;

            public SocketCallback(Socket socket, Action<Socket> callback)
                : this()
            {
                Socket = socket;
                Callback = callback;
            }
        }

        private static void MakeSocketPair(out Socket local, out Socket remote)
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