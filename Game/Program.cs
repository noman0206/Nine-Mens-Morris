// Mühle Mini Game Regeln siehe: https://www.spielezar.ch/portal/blog/wissen/mill-spielregeln
using System.Data;
using System.Text;
static class Game
{
    public abstract class Player
    {
        public static List<Player> players { get; set; } = [];
        /// <summary>
        /// Ausgabe aller Spieler
        /// </summary>
        public static void Print_players()
        {
            string gamesymbol = "●";

            foreach (Player player in players) // Für jeden Spieler in der Liste
            {
                if (Gameboard.currentPlayer == player && player.color != null) { Console.Write("\t-> "); Program.Colorwrite(gamesymbol, player.color.consolecolor); Console.Write(" " + player.name); Console.WriteLine("\t (" + player.Get_slot_count() + ")"); } // Gebe Spieler 1 mit Farbe aus und markiere ihn das er dran ist mit ->
                else if(player.color != null){ Console.Write("\t   "); Program.Colorwrite(gamesymbol, player.color.consolecolor); Console.Write(" " + player.name); Console.WriteLine("\t (" + player.Get_slot_count() + ")"); } // Gebe Spieler 1 mit Farbe aus
            }
        }
        public class Color
        {
            public static List<Color> colors = new();
            /// <summary>
            /// Initialisierung aller Spielerfarben
            /// </summary>
            public static void Init()
            {
                new Color("Blau", ConsoleColor.Blue);
                new Color("Grün", ConsoleColor.Green);
                new Color("Rot", ConsoleColor.Red);
                new Color("Gelb", ConsoleColor.Yellow);
            }
            public ConsoleColor consolecolor { get; private set; }
            public string name { get; private set; }
            /// <summary>
            /// Konstruktor
            /// </summary>
            /// <param name="name">Name der Farbe</param>
            /// <param name="consolcolor">Consolecolor der Farbe</param>
            public Color(string name, ConsoleColor consolecolor)
            {
                this.name = name;
                this.consolecolor = consolecolor;
                colors.Add(this);
            }
        }
        public string name { get; set; } = "";
        public Color? color { get; set; } = null;
        /// <summary>
        /// Funktion des Spielzugs eines Spilers
        /// </summary>
        public abstract void Play();
        /// <summary>
        /// Funktion um einen neuen Spielstein zu setzen
        /// </summary>
        public abstract void Assign_new_Pin();
        /// <summary>
        /// Gebe Anzahl der Spilesteine eines Spielers auf dem Spielfeld
        /// </summary>
        /// <returns>Anzahl Spielsteine</returns>
        public int Get_slot_count()
        {
            int i = 0;
            foreach (Gameboard.Slot slot in Gameboard.Slot.slots)
            {
                if (slot.player == this) i++;
            }
            return i;
        }
    }
    public class RealPlayer : Player
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public RealPlayer() : base()
        {
            Program.Logo.Print();
            Console.Write("\n\t  Spieler " + (players.Count() + 1) + " Gib deinen Namen ein.\n\t  > ");
            string? input = Console.ReadLine();

            if (input != null && input != "") name = input;
            else name = "unbekannter Spieler";

            Program.Menu playercolor_Menu = new Program.Menu("\tWähle eine Farbe aus:", Color.colors.Select(color => color.name).ToList());
            this.color = Color.colors[playercolor_Menu.Start()];
            Color.colors.Remove(this.color);

            players.Add(this);
        }
        /// <summary>
        /// Funktion um einen neuen Spielstein zu setzen
        /// </summary>
        public override void Assign_new_Pin()
        {
            Gameboard.currentPlayer = this;
            int index = 1;
            string infotext = "";
            do
            {
                index = Get_slot(index, 0, infotext);
                if (index != 0 && Gameboard.Slot.slots[index].player != null) infotext = "\tAchtung! es dürfen nur Spielsteine auf unbesetzte Felder plaziert werden.";
            } while (Gameboard.Slot.slots[index].player != null); // Wiederhole solange Slot nicht unbesetzt ist
            Gameboard.Slot.slots[index].player = this;

            Check_new_Mills();
        }
        /// <summary>
        /// Entferne ein Spielstein des Gegners
        /// </summary>
        private void Remove_pin_form_other_player()
        {
            Player? other_player = null;
            List<Gameboard.Slot> possible_slots = [];
            foreach (Player player in players) if (player != this) other_player = player;

            foreach (Gameboard.Slot slot in Gameboard.Slot.slots) if (slot.player == other_player && Gameboard.Mill.Check_Slot_for_Meuhle(Gameboard.Slot.slots.IndexOf(slot)) == false) possible_slots.Add(slot); // Wenn Spielstein dem Gegnergehört und nicht in einer Mühle ist

            if (possible_slots.Count() >= 1)
            {
                int index = 1;
                string infotext = "\tDu hast eine Mühle gemacht.\n\tDu darfst nun einen Stein deines Gegners entfernen.\n\t(Der Stein darf nicht aus einer gegnerischen Mühle sein)";
                do
                {
                    index = Get_slot(index, 0, infotext);
                } while (!possible_slots.Contains(Gameboard.Slot.slots[index])); // Wiederhole solange der ausgewählte pin nicht ein Pin ist, ein Pin des Gegners ist und der pin nicht in eine mühle ist
                Gameboard.Slot.slots[index].player = null;
            }
            else Get_slot(1, 0, "\n\tDu hast zwar eine Mühle Gemacht, jedoch besitzt dein Gegner keinen Stein den du ihm nehmen könntest!\n\t(Wähle irgendeinen Feld an um fortzufahren)");
        }
        /// <summary>
        /// Spieler input zur Slotauswahl
        /// </summary>
        /// <param name="index">Ausgewähltes Element 1</param>
        /// <param name="index_choosen_slot">Ausgewähltes Element 2</param>
        /// <param name="text">Infotext</param>
        /// <returns>Index des ausgewählten Slots</returns>
        public int Get_slot(int index, int index_choosen_slot, string text)
        {
            ConsoleKeyInfo CKI; // Registriere klick event
            do
            {
                Program.Logo.Print();
                Print_players();
                Console.WriteLine(text);
                Gameboard.Print(index, index_choosen_slot);

                CKI = Console.ReadKey(false); // Keyboard abfrage
                if (CKI.Key == ConsoleKey.UpArrow && Gameboard.Slot.slots[index].possible_Moves[0] != Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Gameboard.Slot.slots.IndexOf(Gameboard.Slot.slots[index].possible_Moves[0]);
                }
                else if (CKI.Key == ConsoleKey.RightArrow && Gameboard.Slot.slots[index].possible_Moves[1] != Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Gameboard.Slot.slots.IndexOf(Gameboard.Slot.slots[index].possible_Moves[1]);
                }
                else if (CKI.Key == ConsoleKey.DownArrow && Gameboard.Slot.slots[index].possible_Moves[2] != Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Gameboard.Slot.slots.IndexOf(Gameboard.Slot.slots[index].possible_Moves[2]);
                }
                else if (CKI.Key == ConsoleKey.LeftArrow && Gameboard.Slot.slots[index].possible_Moves[3] != Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Gameboard.Slot.slots.IndexOf(Gameboard.Slot.slots[index].possible_Moves[3]);
                }
            } while (!(CKI.Key == ConsoleKey.Enter));  // Falls enter gedrückt  
            return index;
        }
        /// <summary>
        /// Überprüfe das Spielfeld auf neu gemachte Mühlen und Entferne bei bei neu gefundenen Mühlen einen Spielstein des Gegners
        /// </summary>
        private void Check_new_Mills()
        {
            int found_Muehles = Gameboard.Mill.Check(); // überprüfe ob er eine neue Mühle gemacht hat bzw ob allte Mühlen noch da sind
            while (found_Muehles >= 1)
            {
                Remove_pin_form_other_player();
                found_Muehles--;
            }
        }
        /// <summary>
        /// Spielfunktion des realen Spielers
        /// </summary>
        public override void Play()
        {
            Gameboard.currentPlayer = this;
            int index = 1;
            string infotext = "";
            bool turn_ended = false;
            do
            {
                do
                {
                    index = Get_slot(index, 0, infotext);
                    if (Gameboard.Slot.slots[index].player != this) infotext = "Du darfst nur deine Steine verschieben";
                    else infotext = "";
                } while (Gameboard.Slot.slots[index].player != this);
                Gameboard.Slot slot = Gameboard.Slot.slots[index];

                if (Get_slot_count() > 3)
                {
                    do
                    {
                        index = Get_slot(index, Gameboard.Slot.slots.IndexOf(slot), infotext);
                    } while (!((slot.possible_Moves.Contains(Gameboard.Slot.slots[index]) && Gameboard.Slot.slots[index].player == null) || Gameboard.Slot.slots[index] == slot));
                }
                else
                {
                    do
                    {
                        index = Get_slot(index, Gameboard.Slot.slots.IndexOf(slot), "Du hast nur noch 3 Spielsteine. Du darfst nun hüpfen." + infotext);
                    } while (Gameboard.Slot.slots[index].player != null || Gameboard.Slot.slots[index] == slot);
                }
                if (Gameboard.Slot.slots[index] != slot)
                {
                    turn_ended = true;
                    slot.player = null;
                    Gameboard.Slot.slots[index].player = this;
                }
            } while (turn_ended == false);

            Check_new_Mills();
        }
    }
    public class AiPlayer : Player
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public AiPlayer() : base()
        {
            this.name = "KI - John";
            this.color = Color.colors[new Random().Next(0, Color.colors.Count())];
            Color.colors.Remove(this.color);
            players.Add(this);
        }
        public override void Assign_new_Pin()
        {
            throw new NotImplementedException();
        }
        public override void Play()
        {
            throw new NotImplementedException();
        }
    }
    public static class Gameboard
    {
        public static Player? currentPlayer { get; set; }
        /// <summary>
        /// Spielfeldausgabe
        /// </summary>
        /// <param name="index">ausgewählter Slot 1</param>
        /// <param name="index2">ausgewählter Slot 2</param>
        public static void Print(int index, int index2)
        {
            string gamesymbol = "●"; // Symbol des Spielsteines
            Console.Write("");
            Console.Write("\n\t\t"); Slot.slots[1].Print(gamesymbol, "┌", index, index2); Console.Write("────────"); Slot.slots[2].Print(gamesymbol, "┬", index, index2); Console.Write("────────"); Slot.slots[3].Print(gamesymbol, "┐", index, index2);                                                                                                                                                                                                                       // ┌────────┬────────┐
            Console.Write("\n\t\t│  "); Slot.slots[9].Print(gamesymbol, "┌", index, index2); Console.Write("─────"); Slot.slots[10].Print(gamesymbol, "┼", index, index2); Console.Write("─────"); Slot.slots[11].Print(gamesymbol, "┐", index, index2); Console.Write("  │");                                                                                                                                                                                                    // │  ┌─────┼─────┐  │
            Console.Write("\n\t\t│  │  "); Slot.slots[17].Print(gamesymbol, "┌", index, index2); Console.Write("──"); Slot.slots[18].Print(gamesymbol, "┴", index, index2); Console.Write("──"); Slot.slots[19].Print(gamesymbol, "┐", index, index2); Console.Write("  │  │");                                                                                                                                                                                                  // │  │  ┌──┴──┐  │  │
            Console.Write("\n\t\t"); Slot.slots[8].Print(gamesymbol, "├", index, index2); Console.Write("──"); Slot.slots[16].Print(gamesymbol, "┼", index, index2); Console.Write("──"); Slot.slots[24].Print(gamesymbol, "┤", index, index2); Console.Write("     "); Slot.slots[20].Print(gamesymbol, "├", index, index2); Console.Write("──"); Slot.slots[12].Print(gamesymbol, "┼", index, index2); Console.Write("──"); Slot.slots[4].Print(gamesymbol, "┤", index, index2);             // ├──┼──┤     ├──┼──┤
            Console.Write("\n\t\t│  │  "); Slot.slots[23].Print(gamesymbol, "└", index, index2); Console.Write("──"); Slot.slots[22].Print(gamesymbol, "┬", index, index2); Console.Write("──"); Slot.slots[21].Print(gamesymbol, "┘", index, index2); Console.Write("  │  │");                                                                                                                                                                                                  // │  │  └──┬──┘  │  │
            Console.Write("\n\t\t│  "); Slot.slots[15].Print(gamesymbol, "└", index, index2); Console.Write("─────"); Slot.slots[14].Print(gamesymbol, "┼", index, index2); Console.Write("─────"); Slot.slots[13].Print(gamesymbol, "┘", index, index2); Console.Write("  │");                                                                                                                                                                                                  // │  └─────┼─────┘  │
            Console.Write("\n\t\t"); Slot.slots[7].Print(gamesymbol, "└", index, index2); Console.Write("────────"); Slot.slots[6].Print(gamesymbol, "┴", index, index2); Console.Write("────────"); Slot.slots[5].Print(gamesymbol, "┘", index, index2);                                                                                                                                                                                                                       // └────────┴────────┘
        }
        public class Slot
        {
            public static List<Slot> slots { get; private set; } = new();
            /// <summary>
            /// Initialisierung aller Slots auf dem Spielfeld
            /// </summary>
            public static void Init()
            {
                for (int i = 0; i < 25; i++)
                {
                    new Slot(); // Slot[0] bis Slot[24] erzeugen 
                }
                slots[0].Set_possible_Moves([0, 0, 0, 0]);//slots[0] => Nullslot
                slots[1].Set_possible_Moves([0, 2, 8, 0]); // Slot Nummer: 1 Mögliche Spielzüge = 2,8
                slots[2].Set_possible_Moves([0, 3, 10, 1]); // Slot Nummer: 2 Mögliche Spielzüge = 1,3,10
                slots[3].Set_possible_Moves([0, 0, 4, 2]); // Slot Nummer: 3 Mögliche Spielzüge = 2,4
                slots[4].Set_possible_Moves([3, 0, 5, 12]); // Slot Nummer: 4 Mögliche Spielzüge = 3,5,12
                slots[5].Set_possible_Moves([4, 0, 0, 6]); // Slot Nummer: 5 Mögliche Spielzüge = 4,6
                slots[6].Set_possible_Moves([14, 5, 0, 7]); // Slot Nummer: 6 Mögliche Spielzüge = 5,7,14
                slots[7].Set_possible_Moves([8, 6, 0, 0]); // Slot Nummer: 7 Mögliche Spielzüge = 6,8
                slots[8].Set_possible_Moves([1, 16, 7, 0]); // Slot Nummer: 8 Mögliche Spielzüge = 1,16,7
                slots[9].Set_possible_Moves([0, 10, 16, 0]); // Slot Nummer: 9 Mögliche Spielzüge = 10,16
                slots[10].Set_possible_Moves([2, 11, 18, 9]); // Slot Nummer: 10 Mögliche Spielzüge = 2,9,11,18
                slots[11].Set_possible_Moves([0, 0, 12, 10]); // Slot Nummer: 11 Mögliche Spielzüge = 10,12
                slots[12].Set_possible_Moves([11, 4, 13, 20]); // Slot Nummer: 12 Mögliche Spielzüge = 4,11,13,20
                slots[13].Set_possible_Moves([12, 0, 0, 14]); // Slot Nummer: 13 Mögliche Spielzüge = 12,14
                slots[14].Set_possible_Moves([22, 13, 6, 15]); // Slot Nummer: 14 Mögliche Spielzüge = 6,13,15,22
                slots[15].Set_possible_Moves([16, 14, 0, 0]); // Slot Nummer: 15 Mögliche Spielzüge = 14,16
                slots[16].Set_possible_Moves([9, 24, 15, 8]); // Slot Nummer: 16 Mögliche Spielzüge = 8,9,15,24
                slots[17].Set_possible_Moves([0, 18, 24, 0]); // Slot Nummer: 17 Mögliche Spielzüge = 18,24
                slots[18].Set_possible_Moves([10, 19, 0, 17]); // Slot Nummer: 18 Mögliche Spielzüge = 10,17,19
                slots[19].Set_possible_Moves([0, 0, 20, 18]); // Slot Nummer: 19 Mögliche Spielzüge = 18,20
                slots[20].Set_possible_Moves([19, 12, 21, 0]); // Slot Nummer: 20 Mögliche Spielzüge = 12,19,21
                slots[21].Set_possible_Moves([20, 0, 0, 22]); // Slot Nummer: 21 Mögliche Spielzüge = 20,22
                slots[22].Set_possible_Moves([0, 21, 14, 23]); // Slot Nummer: 22 Mögliche Spielzüge = 14,21,23
                slots[23].Set_possible_Moves([24, 22, 0, 0]); // Slot Nummer: 23 Mögliche Spielzüge = 22,24
                slots[24].Set_possible_Moves([17, 0, 23, 16]); // Slot Nummer: 24 Mögliche Spielzüge = 16,17,23

            }
            public Player? player { get; set; } // Spieler der einen stein auf dem slot hat | standart: Null
            public List<Slot> possible_Moves { get; private set; } = new(); // liste der möglichen Züge von diesem Punkt aus | Liste aus 4 elementen 0: nach oben 1: nach rechts 2 : nach unten 3: nach links | wenn element 0 ist dann ist der zug nicht möglich     
            /// <summary>
            ///  Konstruktor
            /// </summary> 
            public Slot()
            {
                this.player = null;
                this.possible_Moves = [];
                slots.Add(this);
            }
            /// <summary>
            /// Ausgabe des Slots
            /// </summary>
            /// <param name="game_char">Steinsymbol in der Konsole</param>
            /// <param name="standard_char">Standartzeichen des Feldes falls es leer ist</param>
            /// <param name="index">Auswahl 1</param>
            /// <param name="index2">Auswahl 2</param>
            public void Print(string game_char, string standard_char, int index, int index2)
            {
                if (index == slots.IndexOf(this) )
                {
                    if (player != null && player.color != null) Program.Colorwrite(game_char, player.color.consolecolor, ConsoleColor.Gray);
                    else Program.Colorwrite(standard_char, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else if (index2 == slots.IndexOf(this))
                {
                    if (player != null && player.color != null) Program.Colorwrite(game_char, player.color.consolecolor, ConsoleColor.Gray);
                    else Program.Colorwrite(standard_char, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else
                {
                    if (player != null && player.color != null) Program.Colorwrite(game_char, player.color.consolecolor);
                    else Console.Write(standard_char);
                }
            }
            /// <summary>
            /// Festlegung der möglichen Spielzüge von diesem Slot aus
            /// </summary>
            /// <param name="numbers">Liste der Slotnummern aus Slot.slots</param>
            public void Set_possible_Moves(List<int> numbers)
            {
                foreach (int number in numbers)
                {
                    if (number >= 0 && number <= 24) this.possible_Moves.Add(slots[number]);
                }
            }
        }
        public class Mill
        {
            public class Combination
            {
                public static List<Combination> combinations { get; private set; } = new(); // Liste aller Kombinatione
                /// <summary>
                /// Initialisierung aller möglichen Kombinationen
                /// </summary>
                public static void Init()
                {
                    new Combination([Slot.slots[1], Slot.slots[2], Slot.slots[3]]);
                    new Combination([Slot.slots[3], Slot.slots[4], Slot.slots[5]]);
                    new Combination([Slot.slots[5], Slot.slots[6], Slot.slots[4]]);
                    new Combination([Slot.slots[7], Slot.slots[8], Slot.slots[1]]);
                    new Combination([Slot.slots[9], Slot.slots[10], Slot.slots[11]]);
                    new Combination([Slot.slots[11], Slot.slots[12], Slot.slots[13]]);
                    new Combination([Slot.slots[13], Slot.slots[14], Slot.slots[15]]);
                    new Combination([Slot.slots[15], Slot.slots[16], Slot.slots[9]]);
                    new Combination([Slot.slots[17], Slot.slots[18], Slot.slots[19]]);
                    new Combination([Slot.slots[19], Slot.slots[20], Slot.slots[21]]);
                    new Combination([Slot.slots[21], Slot.slots[22], Slot.slots[23]]);
                    new Combination([Slot.slots[23], Slot.slots[24], Slot.slots[17]]);
                    new Combination([Slot.slots[2], Slot.slots[10], Slot.slots[18]]);
                    new Combination([Slot.slots[4], Slot.slots[12], Slot.slots[20]]);
                    new Combination([Slot.slots[6], Slot.slots[14], Slot.slots[22]]);
                    new Combination([Slot.slots[8], Slot.slots[16], Slot.slots[24]]);
                }
                public List<Slot> slots { get; private set; } = new(); // Slots in einer Kombination
                /// <summary>
                /// Konstruktor
                /// </summary>
                /// <param name="slots">Liste der Slots die in der Kombination sind</param>
                public Combination(List<Slot> slots)
                {
                    this.slots = slots;
                    combinations.Add(this);
                }
                /// <summary>
                /// Kombination überprüfen
                /// </summary>
                /// <returns>Status der Kombination ob sie noche eine Mühle ist oder nicht</returns>
                public bool Check()
                {
                    if (slots[0].player == slots[1].player && slots[1].player == slots[2].player && slots[0].player == slots[2].player && slots[0].player != null && slots[1].player != null && slots[2].player != null) return true; // Wenn alle 3 Felder den gleichen Spieler beinhalten 
                    else return false;
                }
            }
            public static List<Mill> existing_Mills { get; private set; } = new(); // Liste aller im Spiel momentan exestierender Mühlen
            /// <summary>
            /// Überprüfe Spielfeld auf neue Mühlen
            /// </summary>
            /// <returns>Anzahl der neu Gefundenen Mühlen</returns>
            public static int Check()
            {
                int status = 0;
                List<Combination> all_combinations_used = new();
                List<Mill> delMills = [];
                foreach (Mill mill in existing_Mills)
                {
                    if (!mill.combination.Check()) delMills.Add(mill);
                    else all_combinations_used.Add(mill.combination);
                }
                foreach (Mill mill in delMills) existing_Mills.Remove(mill);
                foreach (Combination combination in Combination.combinations)
                {
                    if (combination.Check() == true)
                    {
                        if (!all_combinations_used.Contains(combination))
                        {
                            new Mill(combination);
                            status++;
                        }
                    }
                }
                return status;
            }
            public Combination combination { get; private set; }
            /// <summary>
            /// Konstruktor
            /// </summary>
            /// <param name="combination">Kombination der Mühle</param>
            public Mill(Combination combination)
            {
                this.combination = combination;
                existing_Mills.Add(this);
            }
            /// <summary>
            /// Überprüfe einen Slot ober er in einer Mühle ist
            /// </summary>
            /// <param name="index">Index des Slots</param>
            /// <returns>Status ob er in einer Mühle ist</returns>
            public static bool Check_Slot_for_Meuhle(int index) // überprüfe ob Pin in einer Mühle ist
            {
                foreach (Mill mill in existing_Mills)
                {
                    if (mill.combination.slots.Contains(Slot.slots[index]) == true) return true;
                }
                return false;
            }
        }
    }
    /// <summary>
    /// Spiel Starten
    /// </summary>
    /// <param name="singelplayer">Status ob Spiel eine Einzelspielerspiel ist</param>
    public static void Start(List<Player> players)
    {
        Player.Color.Init();
        Gameboard.Slot.Init();
        Gameboard.Mill.Combination.Init();
        Player.players = players;
        bool gameend = false;
        
        for (int i = 0; i < 9; i++) foreach (Player player in Player.players) player.Assign_new_Pin();

        do
        {
            foreach (Player player in Player.players)
            {
                player.Play();
                if(Player.players[0].Get_slot_count() < 3 || Player.players[1].Get_slot_count() < 3) {gameend = true; break;}
            }  
        }
        while (!gameend); 

        Player Winner;
        if (Player.players[0].Get_slot_count() > Player.players[1].Get_slot_count()) Winner = Player.players[0];
        else Winner = Player.players[1];

        Program.Logo.Print();
        if (Winner != null) Console.WriteLine("\n\n\t" + Winner.name + " hat gewonnen!");
        Gameboard.Print(0,0);
        Console.WriteLine("\n\n\tDrücke eine Taste um das Spiel zu beenden\n");
        Console.Read();
    }
}
class Program
{
    public static class Logo
    {
        private static string logo = "\t    __  __   _   _   _       _\n\t   |  \\/  | (_) (_) | |     | |\n\t   | \\  / |  _   _  | |__   | |   ___\n\t   | |\\/| | | | | | | '_ \\  | |  / _ \\\n\t   | |  | | | |_| | | | | | | | |  __/\n\t   |_|  |_|  \\__,_| |_| |_| |_|  \\___|";
        /// <summary>
        /// Ausgabe des Logos
        /// </summary>
        public static void Print()
        {
            Console.Clear();
            Console.WriteLine(logo);
            Console.WriteLine();
        }
    }
    /// <summary>
    /// farbliche Ausgabe eines Textes
    /// </summary>
    /// <param name="text">auszugebender text</param>
    /// <param name="fgc">Vordergundfarbe</param>
    /// <param name="bgc">Hintergrundfarbe</param>
    public static void Colorwrite(string text, ConsoleColor fgc, ConsoleColor bgc)
    {
        Console.ForegroundColor = fgc;
        Console.BackgroundColor = bgc;
        Console.Write(text);
        Console.ResetColor();
    }
    /// <summary>
    /// farbliche Ausgabe eines Textes mit standart Hintergundfarbe
    /// </summary>
    /// <param name="text">auszugebender Text</param>
    /// <param name="fgc">Vordergrundfarbe</param>
    public static void Colorwrite(string text, ConsoleColor fgc)
    {
        //Console.ResetColor();
        Console.ForegroundColor = fgc;
        Console.Write(text);
        Console.ResetColor();
    }
    public class Menu
    {
        public List<string> options { get; set; } = new();
        private string title { get; set; }
        private int index { get; set; } = 0;
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="title">Titel des Menus</param>
        /// <param name="options">Lister der Optionen des Menus</param>
        public Menu(string title, List<string> options)
        {
            this.title = title;
            this.options = options;
        }
        /// <summary>
        /// Ausgabe des Menus
        /// </summary>
        private void Print()
        {
            Logo.Print();
            if (title != "") Console.Write("\n\t" + title);
            foreach (string option in options)
            {
                if (index == options.IndexOf(option)) { Console.Write("\n\t\t\t"); Colorwrite(option, ConsoleColor.Black, ConsoleColor.Gray); }
                else Console.Write("\n\t\t\t" + option);
            }
        }
        /// <summary>
        /// Start des Menus
        /// </summary>
        /// <returns>Index der ausgewählten Option</returns>
        public int Start()
        {
            ConsoleKeyInfo CKI;
            do
            {
                Print();
                CKI = Console.ReadKey(false);
                if (CKI.Key == ConsoleKey.UpArrow && index > 0) index--;
                else if (CKI.Key == ConsoleKey.DownArrow && index < (options.Count - 1)) index++;
            } while (CKI.Key != ConsoleKey.Enter);
            return index;
        }
    }
    /// <summary>
    /// Mainmenu funktion Spielen
    /// </summary>
    static void Play()
    {
        Menu playMenu = new Menu("", ["Einzelspieler", "Mehrspieler", "Zurück"]);
        switch (playMenu.Start())
        {
            case 0: Console.Clear(); Console.Write("\n\t\tIn arbeit!"); Console.ReadKey(); break;
            case 1: Console.Clear(); Game.Start([new Game.RealPlayer(),new Game.RealPlayer()]); break;
            case 2: break;
        }
    }
    /// <summary>
    /// Mainmenu funktion Regeln
    /// </summary>
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
    /// <summary>
    /// Mainmenu funktion Credits
    /// </summary>
    static void Credits()
    {

    }
    /// <summary>
    /// Mainmenu funktion Beenden
    /// </summary>
    static void Exit()
    {
        Menu exitMenu = new Menu("  Willst du das Spiel wirklich beenden?\n", ["Nein", "Ja"]);
        switch (exitMenu.Start())
        {
            case 0: break;
            case 1: running = false; break;
        }
    }
    static bool running = true;
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8; // Setze Ausgabeversion auf UTF8

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

