using CoreLibrary.Helpers;

namespace PeerLibrary.TokenProviders.Models
{
    static internal class TokenInfoExtensions
    {
        public static TokenInfo ToTokenInfo(this Dictionary<string, object> tokenDict)
        {
            DateTime now = DateTime.UtcNow;

            TokenInfo tokenInfo = new()
            {
                AccessToken = tokenDict.GetOrFail("access_token").ToStringValue(),
                RefreshToken = tokenDict.GetOrFail("refresh_token").ToStringValue(),
                AccessTokenExpiration = now.AddSeconds(int.Parse(tokenDict.GetOrFail("expires_in").ToStringValue())),
                RefreshTokenExpiration = now.AddSeconds(tokenDict.GetOrDefault("refresh_token_expires_in", "").ToString().ParseToIntValue())
            };

            return tokenInfo;
        }
    }
}
