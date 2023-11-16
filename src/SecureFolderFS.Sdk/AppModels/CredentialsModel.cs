using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class CredentialsModel : IDisposable, IEnumerable<IDisposable>
    {
        private readonly List<IDisposable> _authenticationData;

        public CredentialsModel()
        {
            _authenticationData = new();
        }

        public void Add(IDisposable magic)
        {
            _authenticationData.Add(magic);
        }

        public IEnumerator<IDisposable> GetEnumerator()
        {
            return _authenticationData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _authenticationData.DisposeElements();
            _authenticationData.Clear();
        }
    }
}
