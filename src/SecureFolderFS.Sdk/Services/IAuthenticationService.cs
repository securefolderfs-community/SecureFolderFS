using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    internal interface IAuthenticationService
    {
        Task<IIdentity> SystemAsync();

        Task<IIdentity> HardwareKeyAsync();
    }
}
