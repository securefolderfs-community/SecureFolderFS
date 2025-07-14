using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Shared.Helpers
{
    public sealed class FirstTimeHelper
    {
        private readonly List<string> _callers = new();

        public bool IsFirstTime([CallerMemberName] string callerId = "")
        {
            if (_callers.Contains(callerId))
                return false;

            _callers.Add(callerId);
            return true;
        }
    }
}
