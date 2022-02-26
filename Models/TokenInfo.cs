namespace PeerLibrary.Models
{
    internal class TokenInfo
    {
        public string AccessToken { get; set; } = String.Empty;
        public string RefreshToken { get; set; } = String.Empty;
        public DateTime AccessTokenExpiration { get; set; } = DateTime.MinValue;
        public DateTime RefreshTokenExpiration { get; set; } = DateTime.MinValue;
    }
}
