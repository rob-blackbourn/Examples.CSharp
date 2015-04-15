using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient3
{
    public static class NetworkExtensions
    {
        public static ISubject<byte[], byte[]> ToSubject(this Stream stream, Func<Stream, Task<byte[]>> producer, Func<Stream, byte[], Task> consumer, Func<Exception,bool> isClosed)
        {
            return SubjectEx.Create<Stream, byte[], byte[]>(stream, async x => await producer(x), isClosed, x => x == null, async (s, buf) => await consumer(s, buf), error => { }, () => { });
        }
    }
}
