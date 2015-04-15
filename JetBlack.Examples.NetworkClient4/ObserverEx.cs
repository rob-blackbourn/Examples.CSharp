using System;
using System.Reactive;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient4
{
    public static class ObserverEx
    {
        public static IObserver<TIn> ToObserver<TSink, TIn>(this TSink sink, Func<TSink, TIn, Task> asyncConsumer, Func<Exception, bool> isSinkCompleted, Action<Exception> onError, Action onCompleted)
        {
            return Observer.Create<TIn>(async value =>
            {
                try
                {
                    await asyncConsumer(sink, value);
                }
                catch (Exception error)
                {
                    if (isSinkCompleted(error))
                        onCompleted();
                    else
                        onError(error);
                }
            },
                onError,
                onCompleted);
        }
    }
}
