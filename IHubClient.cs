﻿namespace PeerLibrary
{
    public interface IHubClient : IAsyncDisposable
    {
        Task Start();
    }
}