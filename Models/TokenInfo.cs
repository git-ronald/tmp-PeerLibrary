namespace PeerLibrary.Models
{
    internal class TokenInfo
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime AccessTokenExpiration { get; set; } = DateTime.MinValue;
        public DateTime RefreshTokenExpiration { get; set; } = DateTime.MinValue;
    }
}
