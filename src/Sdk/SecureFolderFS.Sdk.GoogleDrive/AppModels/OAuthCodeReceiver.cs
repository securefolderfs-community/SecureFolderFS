using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.GoogleDrive.AppModels
{
    internal sealed class OAuthCodeReceiver : ICodeReceiver
    {
        private readonly IOAuthHandler _oauthHandler;

        /// <inheritdoc/>
        public string RedirectUri => _oauthHandler.RedirectUrl;

        public OAuthCodeReceiver(IOAuthHandler oauthHandler)
        {
            _oauthHandler = oauthHandler;
        }

        /// <inheritdoc/>
        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            var urlString = url.Build().ToString();
            var result = await _oauthHandler.GetCodeAsync(urlString, taskCancellationToken);
            if (!result.Successful)
            {
                return new()
                {
                    Error = result.Value?.Error?.ToString(),
                    State = result.Value?.State?.ToString()
                };
            }

            return new()
            {
                Code = result.Value!.Code!.ToString(),
                State = result.Value?.State?.ToString()
            };
        }
    }
}