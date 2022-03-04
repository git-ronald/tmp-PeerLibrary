namespace PeerLibrary.TokenProviders.Models
{
    internal class TokenInfo
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; } = DateTime.MinValue;
        public DateTime RefreshTokenExpiration { get; set; } = DateTime.MinValue;
    }
}
