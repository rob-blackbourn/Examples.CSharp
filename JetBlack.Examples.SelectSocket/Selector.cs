using System;
using System.Net.Sockets;
using System.Threading;

namespace JetBlack.Examples.SelectSocket
{
    public class Selector
    {
        private readonly Selectable _selectable = new Selectable();

        public void AddCallback(SelectMode mode, Socket socket, Action<Socket> callback)
        {
            _selectable.AddCallback(mode, socket, callback);
        }

        public void RemoveCallback(SelectMode mode, Socket socket)
        {
            _selectable.RemoveCallback(mode, socket);
        }

        public void Start(int microSeconds, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var checkable = _selectable.CreateCheckable();

                if (checkable.IsEmpty)
                    continue;

                Socket.Select(checkable.CheckRead, checkable.CheckWrite, checkable.CheckError, microSeconds);

                // The select may have blocked for some time, so check the cancellationtoken again.
                if (token.IsCancellationRequested)
                    return;

                _selectable.InvokeCallbacks(checkable);
            }
        }
    }
}
