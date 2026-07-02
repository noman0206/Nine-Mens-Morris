using Program;

namespace Program
{
    public static class Logo
    {
        private static string logo = "\t    __  __   _   _   _       _\n\t   |  \\/  | (_) (_) | |     | |\n\t   | \\  / |  _   _  | |__   | |   ___\n\t   | |\\/| | | | | | | '_ \\  | |  / _ \\\n\t   | |  | | | |_| | | | | | | | |  __/\n\t   |_|  |_|  \\__,_| |_| |_| |_|  \\___|";
        public static void Print()
        {
            Console.Clear();
            Console.WriteLine(logo);
            Console.WriteLine();
        }
    }
    public class Menu
    {
        public List<string> options { get; private set; } = new();
        private string title { get; set; }
        private int index { get; set; } = 0;
        public Menu(string title, List<string> options)
        {
            this.title = title;
            this.options = options;
        }
        private void Print()
        {
            Logo.Print();
            
            if (title != "") Console.Write("\n\t" + title);
            foreach (string option in options)
            {
                if (index == options.IndexOf(option)) { Console.Write("\n\t\t\t"); Program.Colorwrite(option, ConsoleColor.Black, ConsoleColor.Gray); }
                else Console.Write("\n\t\t\t" + option);
            }
        }
        public int Start()
        {
            ConsoleKeyInfo CKI = default;
            Print();
            do
            {
                if(Program.resizesignal) Print();
                if (Console.KeyAvailable)
                {
                    CKI= Console.ReadKey(true);
                    if (CKI.Key == ConsoleKey.UpArrow && index > 0 && !Program.sizeError) { index--; Print(); }
                    else if (CKI.Key == ConsoleKey.DownArrow && index < (options.Count - 1) && !Program.sizeError) { index++; Print(); }
                }
                Thread.Sleep(20); // Pollingrate von 50 die sek
            
            } while (CKI.Key != ConsoleKey.Enter );
            return index;
        }
    }
}