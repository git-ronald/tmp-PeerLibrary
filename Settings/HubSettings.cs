namespace PeerLibrary.Settings
{
    internal class HubSettings
    {
        public HubLocationSettings[] Locations { get; set; } = Array.Empty<HubLocationSettings>();
        public string TokenUrl { get; set; } = String.Empty;
        public string ClientId { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}
