namespace SecureFolderFS.Backend.Messages
{
    public abstract class ValueMessage<T> : IMessage<T>
    {
        public T Value { get; }

        public ValueMessage(T value)
        {
            this.Value = value;
        }
    }
}
