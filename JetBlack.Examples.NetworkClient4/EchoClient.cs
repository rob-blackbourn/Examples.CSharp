using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient4
{
    class EchoClient
    {
        public async Task<CancellationToken> Dispatch(string hostname, int port, CancellationToken token)
        {
            var observable = Observable.Create<byte[]>(
                o =>
                {
                    for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                        o.OnNext(Encoding.UTF8.GetBytes(line));
                    return Disposable.Empty;
                });

            var observer = Observer.Create<byte[]>(buf => Console.WriteLine(Encoding.UTF8.GetString(buf)));

            return await NetworkExtensions.Dispatch(hostname, port, observable, observer, token);
        }
    }
}
