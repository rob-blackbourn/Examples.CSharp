using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxNetwork
{
    static class SubjectEx
    {
        public static ISubject<TIn, TOut> ToSubject<TSourceSink, TIn, TOut>(
            this TSourceSink sourceSink,
            Func<TSourceSink, CancellationToken, Task<TOut>> asyncProducer,
            Func<Exception, bool> isExceptionCompleted,
            Func<TOut, bool> isSourceCompleted,
            Func<TSourceSink, TIn, Task> asyncConsumer,
            Action<Exception> onError,
            Action onCompleted)
        {
            return
                Subject.Create(
                    sourceSink.ToObserver(asyncConsumer, isExceptionCompleted, onError, onCompleted),
                    sourceSink.ToObservable(asyncProducer, isExceptionCompleted, isSourceCompleted));
        }
    }
}
