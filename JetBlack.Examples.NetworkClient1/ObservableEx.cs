using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient1
{
    public static class ObservableEx
    {
        public static IObservable<TOut> Create<TOut>(Func<Task<TOut>> producer, Func<TOut, bool> isComplete)
        {
            return Observable.Create<TOut>(async (observer, token) =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var value = await producer();
                        if (isComplete(value))
                            break;

                        observer.OnNext(value);
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObservable<TOut> ToObservable<TIn, TOut>(this TIn source, Func<TIn, Task<TOut>> producer, Func<TOut, bool> isComplete)
        {
            return Observable.Create<TOut>(async (observer, token) =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var value = await producer(source);
                        if (isComplete(value))
                            break;

                        observer.OnNext(value);
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }
    }
}
