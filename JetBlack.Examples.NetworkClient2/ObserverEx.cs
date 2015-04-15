using System;
using System.Reactive;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
{
    public static class ObserverEx
    {
        public static IObserver<TIn> Create<TIn>(Func<TIn, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return Observer.Create<TIn>(async value => await onNext(value), onError, onCompleted);
        }

        public static IObserver<TIn> ToObserver<TSource, TIn>(this TSource source, Func<TSource, TIn, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return Observer.Create<TIn>(async value => await onNext(source, value), onError, onCompleted);
        }

        public static IObserver<TIn> ToObserver<TSink, TIn>(this TSink sink, Func<TSink, TIn, Task> consumer, Func<Exception, bool> isSinkCompleted, Action<Exception> onError, Action onCompleted, Action<TSink> sinkDispose)
        {
            return Observer.Create<TIn>(async value =>
            {
                try
                {
                    await consumer(sink, value);
                }
                catch (Exception error)
                {
                    if (isSinkCompleted(error))
                        onCompleted();
                    else
                        onError(error);

                    sinkDispose(sink);
                }
            },
                onError,
                onCompleted);
        }
    }
}
