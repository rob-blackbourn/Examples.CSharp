using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace JetBlack.Examples.SelectSocket
{
    public class Selector
    {
        private readonly object _gate = new object();
        private readonly IDictionary<Socket, Action> _readables = new Dictionary<Socket, Action>();
        private readonly IDictionary<Socket, Queue<Action>> _writeables = new Dictionary<Socket, Queue<Action>>();
        private readonly IDictionary<Socket, Action> _errorables = new Dictionary<Socket, Action>();
        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(false);

        public void Add(SelectMode mode, Socket socket, Action action)
        {
            lock (_gate)
            {
                switch (mode)
                {
                    case SelectMode.SelectRead:
                        _readables.Add(socket, action);
                        break;
                    case SelectMode.SelectWrite:
                        Queue<Action> actions;
                        if (!_writeables.TryGetValue(socket, out actions))
                            _writeables.Add(socket, actions = new Queue<Action>());
                        actions.Enqueue(action);
                        break;
                    case SelectMode.SelectError:
                        _errorables.Add(socket, action);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }

                _waitEvent.Set();
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

                if (_readables.Count == 0 && _writeables.Count == 0 && _errorables.Count == 0)
                    _waitEvent.Reset();
            }
        }

        public void Start(int microSeconds, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                List<Socket> checkRead, checkWrite, checkError;

                _waitEvent.WaitOne();

                lock (_gate)
                {
                    checkRead = _readables.Count == 0 ? null : _readables.Keys.ToList();
                    checkWrite = _writeables.Count == 0 ? null : _writeables.Keys.ToList();
                    checkError = _errorables.Count == 0 ? null : _errorables.Keys.ToList();
                }

                Socket.Select(checkRead, checkWrite, checkError, microSeconds);

                CollectSockets(checkRead, _readables).ForEach(action => action());
                CollectSockets(checkWrite, _writeables).ForEach(action => action());
                CollectSockets(checkError, _errorables).ForEach(action => action());
            }
        }

        private List<Action> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Action> dictionary)
        {
            var actions = new List<Action>();

            if (sockets != null)
            {
                lock (_gate)
                {
                    foreach (var socket in sockets)
                    {
                        Action action;
                        if (dictionary.TryGetValue(socket, out action))
                            actions.Add(action);
                    }
                }
            }

            return actions;
        }

        private List<Action> CollectSockets(IEnumerable<Socket> sockets, IDictionary<Socket, Queue<Action>> dictionary)
        {
            var actions = new List<Action>();

            if (sockets != null)
            {
                lock (_gate)
                {
                    foreach (var socket in sockets)
                    {
                        Queue<Action> queue;
                        if (dictionary.TryGetValue(socket, out queue))
                            actions.Add(queue.Peek());
                    }
                }
            }

            return actions;
        }
    }
}
