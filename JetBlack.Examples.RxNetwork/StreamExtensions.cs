using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxNetwork
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadBytesCompletelyAsync(this Stream stream, byte[] buf, int length, CancellationToken token)
        {
            var offset = 0;
            while (offset < length)
            {
                var remaining = length - offset;
                var read = await stream.ReadAsync(buf, offset, remaining, token);
                if (read == 0)
                    throw new EndOfStreamException();

                offset += read;
            }
            return buf;
        }
    }
}
