namespace SecureFolderFS.Sdk.Messages
{
    public abstract class ValueMessage<T> : IMessage<T>
    {
        public T Value { get; }

        protected ValueMessage(T value)
        {
            Value = value;
        }
    }
}
