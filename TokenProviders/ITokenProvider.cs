namespace PeerLibrary.TokenProviders
{
    internal interface ITokenProvider
    {
        Task<string?> GetToken();
    }
}
