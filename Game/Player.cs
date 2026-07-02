namespace Program
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
                if (Game.currentPlayer == player && player.color != null) { Console.Write("\t-> "); Program.Colorwrite(gamesymbol, player.color.consolecolor); Console.Write(" " + player.name); Console.WriteLine("\t (" + player.Get_slot_count() + ")"); } // Gebe Spieler 1 mit Farbe aus und markiere ihn das er dran ist mit ->
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
            foreach (Game.Gameboard.Slot slot in Game.Gameboard.Slot.slots)
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
            Logo.Print();
            Console.Write("\n\t  Spieler " + (players.Count() + 1) + " Gib deinen Namen ein.\n\t  > ");
            string? input = Console.ReadLine();

            if (input != null && input != "") name = input;
            else name = "unbekannter Spieler";

            Menu playercolor_Menu = new Menu("\tWähle eine Farbe aus:", Color.colors.Select(color => color.name).ToList());
            this.color = Color.colors[playercolor_Menu.Start()];
            Color.colors.Remove(this.color);

            players.Add(this);
        }
        /// <summary>
        /// Funktion um einen neuen Spielstein zu setzen
        /// </summary>
        public override void Assign_new_Pin()
        {
            Game.currentPlayer = this;
            int index = 1;
            string infotext = "";
            do
            {
                index = Get_slot(index, 0, infotext);
                if (index != 0 && Game.Gameboard.Slot.slots[index].player != null) infotext = "\tAchtung! es dürfen nur Spielsteine auf unbesetzte Felder plaziert werden.";
            } while (Game.Gameboard.Slot.slots[index].player != null); // Wiederhole solange Slot nicht unbesetzt ist
            Game.Gameboard.Slot.slots[index].player = this;

            Check_new_Mills();
        }
        /// <summary>
        /// Entferne ein Spielstein des Gegners
        /// </summary>
        private void Remove_pin_form_other_player()
        {
            Player? other_player = null;
            List<Game.Gameboard.Slot> possible_slots = [];
            foreach (Player player in players) if (player != this) other_player = player;

            foreach (Game.Gameboard.Slot slot in Game.Gameboard.Slot.slots) if (slot.player == other_player && Game.Gameboard.Mill.Check_Slot_for_Meuhle(Game.Gameboard.Slot.slots.IndexOf(slot)) == false) possible_slots.Add(slot); // Wenn Spielstein dem Gegnergehört und nicht in einer Mühle ist

            if (possible_slots.Count() >= 1)
            {
                int index = 1;
                string infotext = "\tDu hast eine Mühle gemacht.\n\tDu darfst nun einen Stein deines Gegners entfernen.\n\t(Der Stein darf nicht aus einer gegnerischen Mühle sein)";
                do
                {
                    index = Get_slot(index, 0, infotext);
                } while (!possible_slots.Contains(Game.Gameboard.Slot.slots[index])); // Wiederhole solange der ausgewählte pin nicht ein Pin ist, ein Pin des Gegners ist und der pin nicht in eine mühle ist
                Game.Gameboard.Slot.slots[index].player = null;
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
                Logo.Print();
                Print_players();
                Console.WriteLine(text);
                Game.Gameboard.Print(index, index_choosen_slot);

                CKI = Console.ReadKey(false); // Keyboard abfrage
                if (CKI.Key == ConsoleKey.UpArrow && Game.Gameboard.Slot.slots[index].possible_Moves[0] != Game.Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game.Gameboard.Slot.slots.IndexOf(Game.Gameboard.Slot.slots[index].possible_Moves[0]);
                }
                else if (CKI.Key == ConsoleKey.RightArrow && Game.Gameboard.Slot.slots[index].possible_Moves[1] != Game.Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game.Gameboard.Slot.slots.IndexOf(Game.Gameboard.Slot.slots[index].possible_Moves[1]);
                }
                else if (CKI.Key == ConsoleKey.DownArrow && Game.Gameboard.Slot.slots[index].possible_Moves[2] != Game.Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game.Gameboard.Slot.slots.IndexOf(Game.Gameboard.Slot.slots[index].possible_Moves[2]);
                }
                else if (CKI.Key == ConsoleKey.LeftArrow && Game.Gameboard.Slot.slots[index].possible_Moves[3] != Game.Gameboard.Slot.slots[0]) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game.Gameboard.Slot.slots.IndexOf(Game.Gameboard.Slot.slots[index].possible_Moves[3]);
                }
            } while (!(CKI.Key == ConsoleKey.Enter));  // Falls enter gedrückt  
            return index;
        }
        /// <summary>
        /// Überprüfe das Spielfeld auf neu gemachte Mühlen und Entferne bei bei neu gefundenen Mühlen einen Spielstein des Gegners
        /// </summary>
        private void Check_new_Mills()
        {
            int found_Muehles = Game.Gameboard.Mill.Check(); // überprüfe ob er eine neue Mühle gemacht hat bzw ob allte Mühlen noch da sind
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
            Game.currentPlayer = this;
            int index = 1;
            string infotext = "";
            bool turn_ended = false;
            do
            {
                do
                {
                    index = Get_slot(index, 0, infotext);
                    if (Game.Gameboard.Slot.slots[index].player != this) infotext = "Du darfst nur deine Steine verschieben";
                    else infotext = "";
                } while (Game.Gameboard.Slot.slots[index].player != this);
                Game.Gameboard.Slot slot = Game.Gameboard.Slot.slots[index];

                if (Get_slot_count() > 3)
                {
                    do
                    {
                        index = Get_slot(index, Game.Gameboard.Slot.slots.IndexOf(slot), infotext);
                    } while (!((slot.possible_Moves.Contains(Game.Gameboard.Slot.slots[index]) && Game.Gameboard.Slot.slots[index].player == null) || Game.Gameboard.Slot.slots[index] == slot));
                }
                else
                {
                    do
                    {
                        index = Get_slot(index, Game.Gameboard.Slot.slots.IndexOf(slot), "Du hast nur noch 3 Spielsteine. Du darfst nun hüpfen." + infotext);
                    } while (Game.Gameboard.Slot.slots[index].player != null || Game.Gameboard.Slot.slots[index] == slot);
                }
                if (Game.Gameboard.Slot.slots[index] != slot)
                {
                    turn_ended = true;
                    slot.player = null;
                    Game.Gameboard.Slot.slots[index].player = this;
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
}
