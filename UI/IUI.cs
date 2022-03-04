namespace PeerLibrary.UI
{
    public interface IUI
    {
        ConsoleKeyInfo ReadKey();
        void WriteLine(object? value);
        void WriteLine();
        void WriteTimeAndLine(object? value);
    }
}
