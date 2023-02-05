namespace SecureFolderFS.Sdk.Messages.Navigation
{
    /// <summary>
    /// Represents a request message used to navigate back.
    /// The navigation availability is determined by preceding pages on the stack.
    /// </summary>
    public sealed class BackNavigationRequestedMessage : IMessage
    {
    }
}
