namespace SecureFolderFS.Backend.Models.Transitions
{
    public abstract class TransitionModel
    {
        public bool IsCustom { get; protected set; } = false;
    }
}
