using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkServer1
{
    public static class ObservableEx
    {
        public static IObservable<TOut> ToObservable<TSource, TOut>(this TSource source, Func<TSource, Task<TOut>> producer, Func<Exception, bool> isSourceExceptionCompleted, Func<TOut, bool> isSourceCompleted)
        {
            return Observable.Create<TOut>(async (observer, token) =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var value = await producer(source);
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
