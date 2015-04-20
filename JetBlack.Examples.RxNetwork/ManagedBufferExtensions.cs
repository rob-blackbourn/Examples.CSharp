using System.ServiceModel.Channels;
using System.Text;

namespace JetBlack.Examples.RxNetwork
{
    public static class ManagedBufferExtensions
    {
        public static ManagedBuffer ToManagedBuffer(this string text, BufferManager bufferManager)
        {
            var length = Encoding.UTF8.GetByteCount(text);
            var buffer = bufferManager.TakeBuffer(length);
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            return new ManagedBuffer(buffer, length, bufferManager);
        }
    }
}
