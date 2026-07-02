// Mühle Mini Game Regeln siehe: https://www.spielezar.ch/portal/blog/wissen/mill-spielregeln
using System.Data;
using System.Text;
namespace Program
{
    class Program
    {
        static bool running = true;
        public static bool resizesignal = false; // Aktuallisierungs-Signal für die Gui
        public static bool sizeError = false; 
        private static int minWidth = 50; // Mindestfenstergröße
        private static int minHeight = 20;
        public static void Colorwrite(string text, ConsoleColor fgc, ConsoleColor bgc)
        {
            Console.ForegroundColor = fgc; // Farben setzten
            Console.BackgroundColor = bgc;
            Console.Write(text); // text in Farbe ausgeben
            Console.ResetColor();
        }
        public static void Colorwrite(string text, ConsoleColor fgc)
        {
            Console.ResetColor(); // Farben zurücksetzten
            Console.ForegroundColor = fgc; // NUR Fordergrundfarbe setzten
            Console.Write(text); // Textausgeben
            Console.ResetColor();
        }
        public static void checkproportions()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            while (running)
            {
                if(Console.WindowHeight < minHeight || Console.WindowWidth < minWidth) // Wenn Konsolenfenster zu klein ist
                {
                    sizeError = true;
                    Console.Clear();
                    Colorwrite("Konsolenfenster zu klein.\nBitte Vergrößern!",ConsoleColor.Red);
                }
                if((Console.WindowWidth != width || Console.WindowHeight != height)&&Console.WindowHeight >= minHeight && Console.WindowWidth >= minWidth ) // Wenn sich fenstergröße verändert hat UND die Konsolengröße groß genug ist
                {
                    resizesignal = true; // Resize-Signal senden
                    width = Console.WindowWidth; // Breite aktuallisieren
                    height = Console.WindowHeight; // Höhe aktuallisieren
                } 
                else if(Console.WindowHeight >= minHeight && Console.WindowWidth >= minWidth && sizeError) // Wenn Fenstergröße wieer in ordnung ist sende Resize-Signal an Gui
                {
                    resizesignal = true; // Resize-Signal senden
                    sizeError = false;
                } 
                else resizesignal = false; // Resize-Signal zurücksetzten
                Thread.Sleep(20); // Pollingrate auf 50 pro sek
            }
        }
        static void Play()
        {
            Menu playMenu = new Menu("", ["Einzelspieler", "Mehrspieler", "Zurück"]);
            switch (playMenu.Start())
            {
                case 0: Console.Clear(); Console.Write("\n\t\tIn arbeit!"); Console.ReadKey(); break;
                case 1: Console.Clear(); Game.Start([new RealPlayer(),new RealPlayer()]); break;
                case 2: break;
            }
        }
        static void Rules()
        {
            Logo.Print();
            Console.WriteLine("\n\t- Ziel des Spiels: Den Gegner auf zwei Steine reduzierenoder ihn so blockieren, dass er keinen gültigen Zug mehr machen kann.");
            Console.WriteLine("\t- Phase 1 (Setzen): Die Spieler setzen abwechselnd ihre 9 Steineauf die Kreuzungspunkte und Ecken des Spielbretts.");
            Console.WriteLine("\t- Phase 2 (Ziehen): Wenn alle Steine gesetzt sind, wirdabwechselnd ein Stein auf einen angrenzenden, freien Punkt entlang der Linien bewegt.");
            Console.WriteLine("\t- Die Mühle: Drei Steine der eigenen Farbe in einer geradenLinie bilden eine Mühle.");
            Console.WriteLine("\t- Stein schlagen: Wer eine Mühle schließt, darf einen Steindes Gegners vom Brett nehmen.");
            Console.WriteLine("\t- Schutzregel: Steine aus einer geschlossenen Mühle des Gegnersdürfen nicht entfernt werden, es sei denn, er hat nur noch Steine in Mühlen.");
            Console.WriteLine("\t- Phase 3 (Springen): Sobald ein Spieler nur noch 3 Steine hat,darf er mit diesen frei auf jeden beliebigen freien Punkt auf dem Brett springen.");
            Console.WriteLine("\t- Spielende: Das Spiel endet, wenn ein Spieler nur noch 2 Steinehat oder keine Züge mehr ausführen kann.\n");
            Colorwrite("\tZurück", ConsoleColor.Black, ConsoleColor.Gray);
            Console.ReadKey();
        }
        static void Credits()
        {

        }
        static void Exit()
        {
            Menu exitMenu = new Menu("  Willst du das Spiel wirklich beenden?\n", ["Nein", "Ja"]);
            switch (exitMenu.Start())
            {
                case 0: break;
                case 1: running = false; break;
            }
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // Setze Ausgabeversion auf UTF8
            
            Thread testproportions = new Thread(checkproportions);
            testproportions.Start();
            
            while (running)
            {
                Menu mainMenu = new Menu("", ["Spielen", "Regeln", "Credits", "Beenden"]);
                switch (mainMenu.Start())
                {
                    case 0: Play(); break;
                    case 1: Rules(); break;
                    case 2: Credits(); break;
                    case 3: Exit(); break;
                }
            }
        }
    }
}


