namespace JetBlack.Examples.RxProperties
{
    public struct PropertyChange
    {
        public PropertyChange(string propertyName, object value)
            : this()
        {
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return string.Format("PropertyName={0}, Value={1}", PropertyName, Value);
        }
    }
}
