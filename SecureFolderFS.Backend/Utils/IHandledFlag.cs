namespace SecureFolderFS.Backend.Utils
{
    public interface IHandledFlag
    {
        void Handle();

        void Handle(bool value);
    }
}
