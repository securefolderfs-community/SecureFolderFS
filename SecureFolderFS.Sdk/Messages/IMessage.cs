namespace SecureFolderFS.Sdk.Messages
{
    public interface IMessage
    {
    }

    public interface IMessage<T> : IMessage
    {
        T? Value { get; }
    }

    public interface IMessageWithSender : IMessage
    {
        public Lazy<object?> Sender { get; }
    }
}
