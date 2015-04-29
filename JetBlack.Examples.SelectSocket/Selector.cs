using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace JetBlack.Examples.SelectSocket
{
    public class Selector
    {
        private readonly object _gate = new object();
        private readonly IDictionary<Socket, Action<Socket>> _readables = new Dictionary<Socket, Action<Socket>>();
        private readonly IDictionary<Socket, Queue<Action<Socket>>> _writeables = new Dictionary<Socket, Queue<Action<Socket>>>();
        private readonly IDictionary<Socket, Action<Socket>> _errorables = new Dictionary<Socket, Action<Socket>>();
        private readonly Socket _reader, _writer;

        public Selector()
        {
            MakeSocketPair(out _reader, out _writer);
            Add(SelectMode.SelectRead, _reader, _ => _reader.Receive(new byte[1024]));
        }
        public void Add(SelectMode mode, Socket socket, Action<Socket> action)
        {
            lock (_gate)
            {
                switch (mode)
                {
                    case SelectMode.SelectRead:
                        _readables.Add(socket, action);
                        break;
                    case SelectMode.SelectWrite:
                        Queue<Action<Socket>> actions;
                        if (!_writeables.TryGetValue(socket, out actions))
                            _writeables.Add(socket, actions = new Queue<Action<Socket>>());
                        actions.Enqueue(action);
                        break;
                    case SelectMode.SelectError:
                        _errorables.Add(socket, action);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }

                if (socket != _reader)
                    InterruptSelect();
            }
        }

        public void Remove(SelectMode mode, Socket socket)
        {
            lock (_gate)
            {
                switch (mode)
                {
                    case SelectMode.SelectRead:
                        _readables.Remove(socket);
                        break;
                    case SelectMode.SelectWrite:
                        var queue = _writeables[socket];
                        queue.Dequeue();
                        if (queue.Count == 0)
                            _writeables.Remove(socket);
                        break;
                    case SelectMode.SelectError:
                        _errorables.Remove(socket);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
            }
        }

        private void InterruptSelect()
        {
            _writer.Send(new byte[] {0});
        }

        public void Start(int microSeconds, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                List<Socket> checkRead, checkWrite, checkError;

                lock (_gate)
                {
                    checkRead = _readables.Count == 0 ? null : _readables.Keys.ToList();
                    checkWrite = _writeables.Count == 0 ? null : _writeables.Keys.ToList();
                    checkError = _errorables.Count == 0 ? null : _errorables.Keys.ToList();
                }

                if ((checkRead == null || checkRead.Count == 0) && (checkWrite == null || checkWrite.Count == 0) && (checkError == null || checkError.Count == 0))
                    continue;

                Socket.Select(checkRead, checkWrite, checkError, microSeconds);

                if (!token.IsCancellationRequested)
                {
                    CollectSockets(checkRead, _readables).ForEach(pair => pair.Value(pair.Key));
                    CollectSockets(checkWrite, _writeables).ForEach(pair => pair.Value(pair.Key));
                    CollectSockets(checkError, _errorables).ForEach(pair => pair.Value(pair.Key));
                }
            }
        }

        private List<KeyValuePair<Socket,Action<Socket>>> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Action<Socket>> dictionary)
        {
            var actions = new List<KeyValuePair<Socket,Action<Socket>>>();

            if (sockets != null)
            {
                lock (_gate)
                {
                    foreach (var socket in sockets)
                    {
                        Action<Socket> action;
                        if (dictionary.TryGetValue(socket, out action))
                            actions.Add(new KeyValuePair<Socket, Action<Socket>>(socket, action));
                    }
                }
            }

            return actions;
        }

        private List<KeyValuePair<Socket,Action<Socket>>> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Queue<Action<Socket>>> dictionary)
        {
            var actions = new List<KeyValuePair<Socket,Action<Socket>>>();

            if (sockets != null)
            {
                lock (_gate)
                {
                    foreach (var socket in sockets)
                    {
                        Queue<Action<Socket>> queue;
                        if (dictionary.TryGetValue(socket, out queue))
                            actions.Add(new KeyValuePair<Socket, Action<Socket>>(socket, queue.Peek()));
                    }
                }
            }

            return actions;
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
