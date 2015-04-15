using System;
using System.Reactive;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient1
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

        public static IObserver<TIn> ToObserver<TSource, TIn>(this TSource source, Func<TSource, TIn, Task> onNext, Func<Exception, bool> isCompleted, Action<Exception> onError, Action onCompleted)
        {
            return Observer.Create<TIn>(async value =>
            {
                try
                {
                    await onNext(source, value);
                }
                catch (Exception error)
                {
                    if (isCompleted(error))
                        onCompleted();
                    else
                        onError(error);
                }
            });
        }
    }
}
