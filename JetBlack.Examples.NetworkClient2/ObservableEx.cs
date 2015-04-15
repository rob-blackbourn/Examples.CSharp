using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
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

        public static IObservable<TOut> ToObservable<TSource, TOut>(this TSource source, Func<TSource, Task<TOut>> producer, Func<Exception, bool> isSourceExceptionCompleted, Func<TOut, bool> isSourceCompleted, Action<TSource> sourceDispose)
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

                    sourceDispose(source);
                }
                catch (Exception error)
                {
                    if (isSourceExceptionCompleted(error))
                        observer.OnCompleted();
                    else
                        observer.OnError(error);
                    sourceDispose(source);
                }
            });
        }

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
