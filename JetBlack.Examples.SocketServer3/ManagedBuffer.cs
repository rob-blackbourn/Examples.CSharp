using System;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.SocketServer3
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
    }
}
