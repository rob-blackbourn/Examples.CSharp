using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxConsoleApp1
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var observable = Observable.Create<int>((observer, token) => 
                Task.Factory.StartNew(() =>
                    {
                        var i = 0;
                        while (!(token.WaitHandle.WaitOne(500) || token.IsCancellationRequested))
                            observer.OnNext(i++);
                    }, token));

            var disposable = observable.Subscribe(
                                 x => Console.WriteLine("OnNext: {0}", x),
                                 error => Console.WriteLine("OnError: {0}", error),
                                 () => Console.WriteLine("OnCompleted"));

            Console.WriteLine("Press <ENTER> to stop subscriptions");
            Console.ReadLine();

            Console.WriteLine("Disposing");
            disposable.Dispose();

            Console.WriteLine("Press <ENTER> to quit");
            Console.ReadLine();
        }
    }
}
