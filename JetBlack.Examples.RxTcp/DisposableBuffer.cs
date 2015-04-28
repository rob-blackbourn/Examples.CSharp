using System;

namespace JetBlack.Examples.RxTcp
{
    public class DisposableBuffer : Buffer, IDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableBuffer(byte[] bytes, int length, IDisposable disposable)
            : base(bytes, length)
        {
            if (disposable == null)
                throw new ArgumentNullException("disposable");
            _disposable = disposable;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
