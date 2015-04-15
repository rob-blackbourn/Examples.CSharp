using System;
using System.IO;
using System.Reactive.Linq;

namespace JetBlack.Examples.RxConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Test1();
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
}
