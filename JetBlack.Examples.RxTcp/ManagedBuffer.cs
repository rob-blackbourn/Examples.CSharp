using System;
using System.Reactive.Disposables;
using System.ServiceModel.Channels;

namespace JetBlack.Examples.RxTcp
{
    public class ManagedBuffer : DisposableBuffer
    {
        public ManagedBuffer(byte[] bytes, int length, BufferManager bufferManager)
            : base(bytes, length, Disposable.Create(() => bufferManager.ReturnBuffer(bytes)))
        {
            if (bufferManager == null)
                throw new ArgumentNullException("bufferManager");
        }

        ~ManagedBuffer()
        {
            Dispose();
        }
    }
}
