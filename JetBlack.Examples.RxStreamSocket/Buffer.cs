namespace JetBlack.Examples.RxStreamSocket
{
    public class Buffer
    {
        public Buffer(byte[] bytes, int length)
        {
            Bytes = bytes;
            Length = length;
        }
    
        public byte[] Bytes { get; private set; }
        public int Length { get; private set; }
    }
}
