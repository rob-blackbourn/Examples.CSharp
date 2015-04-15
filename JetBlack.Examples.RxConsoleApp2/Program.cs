using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Test3();
        }

        static void Test1()
        {
            Console.WriteLine("Press <ENTER> to exit");
            var observable = Console.In.ToObservable();
            observable.Subscribe(Console.WriteLine, error => Console.WriteLine("Error: {0}\r\n{1}", error.Message, error.StackTrace), () => Console.WriteLine("Complete"));
        }

        static void Test2()
        {
            Console.WriteLine("Press <ENTER> to exit");
            var observable = Console.In.ToObservable();
            var connectable = observable.Publish();
            connectable.Subscribe(x => Console.WriteLine("First: {0}", x));
            connectable.Subscribe(x => Console.WriteLine("Second: {0}", x));
            connectable.Connect();
        }

        static void Test3()
        {
            Console.WriteLine("Press <ENTER> to exit");
            var observable = ObservableEx.Create(async () => await Console.In.ReadLineAsync(), string.IsNullOrEmpty);
            observable.Subscribe(Console.WriteLine, error => Console.WriteLine("Error: {0}\r\n{1}", error.Message, error.StackTrace), () => Console.WriteLine("Complete"));
        }
    }

    public static class Extensions
    {
        public static IObservable<string> ToObservable(this TextReader reader)
        {
            return Observable.Create<string>(async (observer, token) =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line))
                            break;

                        observer.OnNext(line);
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

        public static IObserver<TIn> Create<TIn>(Func<TIn, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return Observer.Create<TIn>(async value => await onNext(value), onError, onCompleted);
        }
    }
}
