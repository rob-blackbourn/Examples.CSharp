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
            var length = BitConverter.ToInt32(await stream.ReadBytesCompletelyAsync(new byte[HeaderLength], HeaderLength, token), 0);
            var buffer = await stream.ReadBytesCompletelyAsync(bufferManager.TakeBuffer(length), length, token);
            return new ManagedBuffer(buffer, length, bufferManager);
        }

        public async Task WriteFrameAsync(Stream stream, CancellationToken token)
        {
            var headerBuffer = BitConverter.GetBytes(Length);
            await stream.WriteAsync(headerBuffer, 0, headerBuffer.Length, token);
            await stream.WriteAsync(Buffer, 0, Length, token);
        }

        public static async Task<ManagedBuffer> ReadAsync(Stream source, BufferManager bufferManager, int bufferSize, CancellationToken token)
        {
            var buffer = bufferManager.TakeBuffer(bufferSize);
            var bytesRead = await source.ReadAsync(buffer, 0, bufferSize, token);
            return bytesRead == 0 ? null : new ManagedBuffer(buffer, bytesRead, bufferManager);
        }

        public async Task WriteAsync(Stream stream, CancellationToken token)
        {
            await stream.WriteAsync(Buffer, 0, Length, token);
        }
    }
}
