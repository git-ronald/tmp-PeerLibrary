namespace PeerLibrary.Settings
{
    internal class HubSettings
    {
        public string HubUrl { get; set; } = String.Empty; // TODO: Some kind of deployment mechanism should overwrite this setting for a pasticular environment
        public string TokenUrl { get; set; } = String.Empty;
        public string ClientId { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}
