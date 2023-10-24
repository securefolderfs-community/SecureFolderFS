using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class CredentialsModel : IDisposable
    {
        private readonly List<IDisposable> _authenticationData;

        public CredentialsModel()
        {
            _authenticationData = new();
        }

        public void Add(IDisposable disposable)
        {
            _authenticationData.Add(disposable);
        }

        public IEnumerable<IDisposable> Retrieve()
        {
            return _authenticationData;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _authenticationData.DisposeElements();
            _authenticationData.Clear();
        }
    }
}
