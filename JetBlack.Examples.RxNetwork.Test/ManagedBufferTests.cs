using System;
using System.ServiceModel.Channels;
using NUnit.Framework;

namespace JetBlack.Examples.RxNetwork.Test
{
    [TestFixture]
    public class ManagedBufferTests
    {
        [Test]
        public void TestDispose()
        {
            const int maxBufferPoolSize = 2 << 32;
            const int maxBufferSize = 2 << 16;
            var bufferManager = BufferManager.CreateBufferManager(maxBufferPoolSize, maxBufferSize);

            var len = 13;
            var buf = bufferManager.TakeBuffer(len);
            
            var managedBuffer = new ManagedBuffer(buf, len, bufferManager);

            managedBuffer.Dispose();
            managedBuffer.Dispose();
        }

        [Test]
        public void TestFinalise()
        {
            const int maxBufferPoolSize = 2 << 32;
            const int maxBufferSize = 2 << 16;
            var bufferManager = BufferManager.CreateBufferManager(maxBufferPoolSize, maxBufferSize);

            const int len = 13;
            var buf = bufferManager.TakeBuffer(len);

            var managedBuffer = new ManagedBuffer(buf, len, bufferManager);
            managedBuffer.Dispose();
            managedBuffer = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
