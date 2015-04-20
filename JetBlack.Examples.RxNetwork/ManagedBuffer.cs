using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxNetwork
{
    public class ManagedBuffer : IDisposable
    {
        private const int HeaderLength = sizeof(int);

        BufferManager _bufferManager;

        public ManagedBuffer(byte[] buffer, int length, BufferManager bufferManager)
        {
            Buffer = buffer;
            Length = length;
            _bufferManager = bufferManager;
        }

        ~ManagedBuffer()
        {
            Dispose();
        }

        public byte[] Buffer { get; private set; }
        public int Length { get; private set; }

        public void Dispose()
        {
            var bufferManager = Interlocked.CompareExchange(ref _bufferManager, null, _bufferManager);
            if (bufferManager != null && Buffer != null)
                bufferManager.ReturnBuffer(Buffer);
        }

        public static async Task<ManagedBuffer> ReadFrameAsync(Stream stream, BufferManager bufferManager, CancellationToken token)
        {
            var length = BitConverter.ToInt32(await ReadBytesCompletelyAsync(stream, new byte[HeaderLength], HeaderLength, token), 0);
            var buffer = await ReadBytesCompletelyAsync(stream, bufferManager.TakeBuffer(length), length, token);
            return new ManagedBuffer(buffer, length, bufferManager);
        }

        public async Task WriteFrameAsync(Stream stream, CancellationToken token)
        {
            var headerBuffer = BitConverter.GetBytes(Length);
            await WriteBytesAsync(stream, headerBuffer, headerBuffer.Length, token);
            await WriteBytesAsync(stream, Buffer, Length, token);
        }

        public static async Task<ManagedBuffer> ReadBytesAvailableAsync(Stream source, BufferManager bufferManager, int bufferSize, CancellationToken token)
        {
            var buffer = bufferManager.TakeBuffer(bufferSize);
            var bytesRead = await source.ReadAsync(buffer, 0, bufferSize, token);
            if (bytesRead == 0)
                return null;
            return new ManagedBuffer(buffer, bytesRead, bufferManager);
        }

        public async Task WriteBytesAsync(Stream stream, CancellationToken token)
        {
            await WriteBytesAsync(stream, Buffer, Length, token);
        }

        private static async Task<byte[]> ReadBytesCompletelyAsync(Stream stream, byte[] buf, int length, CancellationToken token)
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

        private static async Task WriteBytesAsync(Stream stream, byte[] buf, int length, CancellationToken token)
        {
            await stream.WriteAsync(buf, 0, length, token);
        }
    }
}
