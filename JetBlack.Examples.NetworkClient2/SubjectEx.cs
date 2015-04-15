using System;
using System.IO;
using System.Net.Sockets;
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
    }
}
