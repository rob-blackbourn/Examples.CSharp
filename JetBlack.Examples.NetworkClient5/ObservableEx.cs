using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient5
{
    public static class ObservableEx
    {
        public static IObservable<TOut> ToObservable<TSource, TOut>(this TSource source, Func<TSource, CancellationToken, Task<TOut>> asyncProducer, Func<Exception, bool> isSourceExceptionCompleted, Func<TOut, bool> isSourceCompleted)
        {
            return Observable.Create<TOut>(async (observer, token) =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var value = await asyncProducer(source, token);
                        if (isSourceCompleted(value))
                            break;

                        observer.OnNext(value);
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    if (isSourceExceptionCompleted(error))
                        observer.OnCompleted();
                    else
                        observer.OnError(error);
                }
            });
        }
    }
}
