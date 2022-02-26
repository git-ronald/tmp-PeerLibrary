namespace PeerLibrary.UI.Default
{
    internal class DefaultUI : IUI
    {
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(string value) => Console.WriteLine(value);

        public void WaitForExit()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
    }
}
