using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkServer1
{
    static class SubjectEx
    {
        public static ISubject<TIn, TOut> Create<TSourceSink, TIn, TOut>(TSourceSink sourceSink, Func<TSourceSink, Task<TOut>> producer, Func<Exception, bool> isExceptionCompleted, Func<TOut, bool> isSourceCompleted, Func<TSourceSink, TIn, Task> consumer, Action<Exception> onError, Action onCompleted)
        {
            return
                Subject.Create(
                    sourceSink.ToObserver(consumer, isExceptionCompleted, onError, onCompleted),
                    sourceSink.ToObservable(producer, isExceptionCompleted, isSourceCompleted));
        }
    }
}
