using CoreLibrary.Helpers;

namespace PeerLibrary.Models
{
    static internal class TokenInfoExtensions
    {
        public static TokenInfo ToTokenInfo(this Dictionary<string, object> tokenDict)
        {
            DateTime now = DateTime.UtcNow;

            TokenInfo tokenInfo = new()
            {
                AccessToken = tokenDict.GetOrThrow("access_token").ToStringValue(),
                RefreshToken = tokenDict.GetOrThrow("refresh_token").ToStringValue(),
                AccessTokenExpiration = now.AddSeconds(int.Parse(tokenDict.GetOrThrow("expires_in").ToStringValue())),
                RefreshTokenExpiration = now.AddSeconds(tokenDict.GetOrDefault("refresh_token_expires_in", "").ToString().ParseToIntValue())
            };

            return tokenInfo;
        }
    }
}
