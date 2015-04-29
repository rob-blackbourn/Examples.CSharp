﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace JetBlack.Examples.SelectSocket
{
    public static class ClientExtensions
    {
        public static IObservable<Buffer> ToClientObservable(this Socket socket, int size, SocketFlags socketFlags, Selector selector)
        {
            return Observable.Create<Buffer>(observer =>
            {
                var buffer = new byte[size];

                selector.AddCallback(SelectMode.SelectRead, socket, _ =>
                {
                    try
                    {
                        var bytes = socket.Receive(buffer, 0, size, socketFlags);
                        if (bytes == 0)
                            observer.OnCompleted();
                        else
                            observer.OnNext(new Buffer(buffer, bytes));
                    }
                    catch (Exception error)
                    {
                        if (error.IsWouldBlock())
                            return;
                        observer.OnError(error);
                    }
                });

                return Disposable.Create(() => selector.RemoveCallback(SelectMode.SelectRead, socket));
            });
        }

        public static IObserver<Buffer> ToClientObserver(this Socket socket, SocketFlags socketFlags, Selector selector, CancellationToken token)
        {
            return Observer.Create<Buffer>(
                buffer =>
                {
                    var state = new BufferState(buffer.Bytes, 0, buffer.Length);

                    // Try to write as much as possible without registering a callback.
                    if (socket.Poll(0, SelectMode.SelectWrite) && socket.Send(socketFlags, state))
                        return;

                    var waitEvent = new AutoResetEvent(false);
                    var waitHandles = new[] {token.WaitHandle, waitEvent};

                    selector.AddCallback(SelectMode.SelectWrite, socket,
                        _ =>
                        {
                            try
                            {
                                if (socket.Send(socketFlags, state))
                                    selector.RemoveCallback(SelectMode.SelectWrite, socket);
                            }
                            finally
                            {
                                waitEvent.Set();
                            }
                        });

                    while (state.Length > 0)
                    {
                        if (WaitHandle.WaitAny(waitHandles) == 0)
                            token.ThrowIfCancellationRequested();
                    }
                });
        }
    }
}
