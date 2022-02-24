using Microsoft.Extensions.Options;
using PeerLibrary.Models;
using PeerLibrary.Settings;
using System.Text.Json;

namespace PeerLibrary.TokenProviders
{
    internal class RopcTokenProvider : ITokenProvider
    {
        private readonly HubSettings _settings;
        private readonly Dictionary<string, string> _tokenPostData;
        private readonly Dictionary<string, string> _refreshPostData;

        private TokenInfo _tokenInfo = new();

        public RopcTokenProvider(IOptions<HubSettings> options)
        {
            _settings = options.Value;
            _tokenPostData = BuildTokenPostData();
            _refreshPostData = BuildBasicRefreshPostData();
        }

        private Dictionary<string, string> BuildTokenPostData()
        {
            return new Dictionary<string, string>()
            {
                { "username", _settings.Username },
                { "password", _settings.Password },
                { "grant_type", "password" },
                { "scope", $"openid {_settings.ClientId} offline_access" },
                { "client_id", _settings.ClientId },
                { "response_type", "token id_token" }
            };
        }

        private Dictionary<string, string> BuildBasicRefreshPostData()
        {
            return new Dictionary<string, string>()
            {
                { "grant_type", "refresh_token" },
                { "response_type", "id_token" },
                { "client_id", _settings.ClientId },
                { "resource", _settings.ClientId }
            };
        }

        public async Task<string?> GetToken()
        {
            DateTime threshold = DateTime.Now.AddMinutes(-3);

            if (_tokenInfo.AccessTokenExpiration > threshold)
            {
                Console.WriteLine("Reuse token.");
                return _tokenInfo.AccessToken;
            }

            if (_tokenInfo.RefreshTokenExpiration > threshold)
            {
                _tokenInfo = await RefreshToken();
                Console.WriteLine("Refreshed token.");
                return _tokenInfo.AccessToken;
            }

            _tokenInfo = await GetNewToken();
            Console.WriteLine($"Got new token. Expiry time: {_tokenInfo.AccessTokenExpiration}.");
            return _tokenInfo.AccessToken;
        }

        private async Task<TokenInfo> GetNewToken()
        {
            FormUrlEncodedContent content = new(_tokenPostData);

            using HttpClient client = new();
            HttpResponseMessage response = await client.PostAsync(_settings.TokenUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failure calling token end-point.");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent) ?? new Dictionary<string, object>();

            return responseDict.ToTokenInfo();
        }

        private async Task<TokenInfo> RefreshToken()
        {
            Console.WriteLine("Refreshing token...");

            FormUrlEncodedContent content = new(_refreshPostData.Concat(new Dictionary<string, string>
            {
                { "refresh_token", _tokenInfo.RefreshToken }
            }));

            using HttpClient client = new();
            HttpResponseMessage response = await client.PostAsync(_settings.TokenUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failure calling refresh token end-point");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent) ?? new Dictionary<string, object>();

            return responseDict.ToTokenInfo();
        }
    }
}
