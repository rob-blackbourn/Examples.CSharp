using System;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.RxTcp
{
    public class ManagedBuffer : Buffer, IDisposable
    {
        BufferManager _bufferManager;

        public ManagedBuffer(byte[] bytes, int length, BufferManager bufferManager)
            : base(bytes, length)
        {
            _bufferManager = bufferManager;
        }

        ~ManagedBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            var bufferManager = Interlocked.CompareExchange(ref _bufferManager, null, _bufferManager);
            if (bufferManager != null && Bytes != null)
                bufferManager.ReturnBuffer(Bytes);
        }
    }
}
