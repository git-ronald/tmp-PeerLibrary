namespace PeerLibrary.UI
{
    // TODO NOW: try TestUI
    public interface IUI
    {
        ConsoleKeyInfo ReadKey();
        void WriteLine(object? value);
        void WriteLine();
    }
}
