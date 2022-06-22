namespace SecureFolderFS.Sdk.Messages
{
    public abstract class ValueMessage<T> : IMessage
    {
        public T Value { get; }

        protected ValueMessage(T value)
        {
            Value = value;
        }
    }
}
