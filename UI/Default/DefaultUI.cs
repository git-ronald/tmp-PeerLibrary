namespace PeerLibrary.UI.Default
{
    internal class DefaultUI : IUI
    {
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(object? value) => Console.WriteLine(value);
        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public void WriteTimeAndLine(object? value)
        {
            Console.Write($"{DateTime.Now} ");
            Console.WriteLine(value);
        }
    }
}
