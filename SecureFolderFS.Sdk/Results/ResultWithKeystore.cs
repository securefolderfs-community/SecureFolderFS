using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.Results
{
    public sealed class ResultWithKeystore : CommonResult<VaultLoginStateType>
    {
        public IKeystoreModel Keystore { get; }

        public ResultWithKeystore(IKeystoreModel keystore, VaultLoginStateType value)
            : base(value, true)
        {
            Keystore = keystore;
        }
    }
}
