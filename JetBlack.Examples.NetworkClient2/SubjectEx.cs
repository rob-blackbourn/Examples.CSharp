using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
{
    static class SubjectEx
    {
        public static ISubject<TIn, TOut> Create<TSource, TSink, TIn, TOut>(TSource source, Func<TSource, Task<TOut>> producer, Func<Exception, bool> isSourceExceptionCompleted, Func<TOut, bool> isSourceCompleted, Action<TSource> sourceDispose, TSink sink, Func<TSink, TIn, Task> consumer, Func<Exception, bool> isSinkCompleted, Action<Exception> onError, Action onCompleted, Action<TSink> sinkDispose)
        {
            return
                Subject.Create(
                    sink.ToObserver(consumer, isSinkCompleted, onError, onCompleted, sinkDispose),
                    source.ToObservable(producer, isSourceExceptionCompleted, isSourceCompleted, sourceDispose));
        }

        public static ISubject<TIn, TOut> Create<TSourceSink, TIn, TOut>(TSourceSink sourceSink, Func<TSourceSink, Task<TOut>> producer, Func<Exception, bool> isSourceExceptionCompleted, Func<TOut, bool> isSourceCompleted, Func<TSourceSink, TIn, Task> consumer, Func<Exception, bool> isSinkCompleted, Action<Exception> onError, Action onCompleted, Action<TSourceSink> dispose)
        {
            return
                Subject.Create(
                    sourceSink.ToObserver(consumer, isSinkCompleted, onError, onCompleted),
                    sourceSink.ToObservable(producer, isSourceExceptionCompleted, isSourceCompleted, dispose));
        }

        public static ISubject<TIn, TOut> Create<TSourceSink, TIn, TOut>(TSourceSink sourceSink, Func<TSourceSink, Task<TOut>> producer, Func<Exception, bool> isExceptionCompleted, Func<TOut, bool> isSourceCompleted, Func<TSourceSink, TIn, Task> consumer, Action<Exception> onError, Action onCompleted)
        {
            return
                Subject.Create(
                    sourceSink.ToObserver(consumer, isExceptionCompleted, onError, onCompleted),
                    sourceSink.ToObservable(producer, isExceptionCompleted, isSourceCompleted));
        }
    }
}
