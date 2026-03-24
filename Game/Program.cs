// Mühle Mini Game Regeln siehe: https://www.spielezar.ch/portal/blog/wissen/mill-spielregeln
using System.ComponentModel.Design;
using System.Data;
using System.Text;
using System.Xml.Serialization;
class Game
{
    public class Player
    {
        public class Color
        {
            public static List<Color> colors {get; private set;}= new(); // Liste Aller farben
            public string name {get; private set;} // Name der Farbe
            public ConsoleColor consolecolor {get; private set;} // Anwendungs Farbe der Farbe
            public Color(string name, ConsoleColor consolecolor) // Konstruktor
            {
                this.name = name;
                this.consolecolor = consolecolor;
                colors.Add(this); // füge Farbe zu Farben-Liste hinzu
            }
        }
        private Game_Board game_board {get; set;}
        public string name {get; private set;} // Name des Spielers
        public Color color {get; private set;}// Farbe des Spielers 
        public static List<Player> players {get; private set;} = new();
        private static Player? currentPlayer {get; set;}
        public Player(Game_Board game_board, int number) // Konstruktor
        {
            this.game_board =game_board;

            Console.Write("\n\tSpieler "+ number +" Gib deinen Namen ein.\n\t> ");
            string? input = Console.ReadLine(); // Lese Eingabe ein
            if(input != null) name = input; // Lese Namen ein
            else name = "unbekannter Spieler";
            
            Program.Menu playercolor_Menu = new Program.Menu("Wähle eine Farbe aus:",Color.colors.Select(color => color.name).ToList()); // Starte Menu mit der nach namen unformatierten Liste der verfügbaren Farben
            this.color = Color.colors[playercolor_Menu.start()]; // setze Spielerfarbe aus Menu ausgabe
            Color.colors.Remove(this.color); // Entferne Spielerfarbe aus möglciche Spielerfarben

            players.Add(this); // Spieler zur Spielerliste hinzufügen
        }
        public Player(string name, Color playercolor, Game_Board game_board) // Konstruktor überladen zum direkterstellen eines Spielers im Code // Debug
        {
            this.game_board = game_board;
            this.name = name;
            this.color = playercolor;
            players.Add(this);
        }
        public static void print_players() // Gebe alle Spieler aus| Spieler der am zug ist | Anzahl der Spielsteine jedes Spielers
        {
            foreach(Player player in players) // Für jeden Spieler in der Liste
            {
                if(currentPlayer == player){Console.Write("\t-> ");Program.print_colored(Game_Board.gamesymbol,player.color.consolecolor,ConsoleColor.Black);Console.Write(" "+player.name);Console.WriteLine("\t ("+player.get_slot_count()+")");} // Gebe Spieler 1 mit Farbe aus und markiere ihn das er dran ist mit ->
                else {Console.Write("\t   ");Program.print_colored(Game_Board.gamesymbol,player.color.consolecolor,ConsoleColor.Black);Console.Write(" "+player.name);Console.WriteLine("\t ("+player.get_slot_count()+")");} // Gebe Spieler 1 mit Farbe aus
            }
        }
        public int choose_slot(int index,int index_choosen_slot,string text) // Slotauswahl des Spielers
        {
            ConsoleKeyInfo CKI; // Registriere klick event
            do
            {
                print_players(); // gebe alle Spieler aus
                Console.WriteLine(text); // gebe evtle Fehlermeldung aus
                game_board.print(index,index_choosen_slot); // gebe Spielbrett mit auswahl aus

                CKI = Console.ReadKey(false); // Keyboard abfrage
                if(CKI.Key == ConsoleKey.UpArrow && Game_Board.Slot.slots[index-1].possible_Moves[0] != Game_Board.Slot.null_Slot) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game_Board.Slot.slots[index-1].possible_Moves[0].number;
                }
                else if(CKI.Key == ConsoleKey.RightArrow && Game_Board.Slot.slots[index-1].possible_Moves[1] != Game_Board.Slot.null_Slot) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game_Board.Slot.slots[index-1].possible_Moves[1].number;
                }
                else if(CKI.Key == ConsoleKey.DownArrow && Game_Board.Slot.slots[index-1].possible_Moves[2] != Game_Board.Slot.null_Slot) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game_Board.Slot.slots[index-1].possible_Moves[2].number;
                }
                else if(CKI.Key == ConsoleKey.LeftArrow && Game_Board.Slot.slots[index-1].possible_Moves[3] != Game_Board.Slot.null_Slot) // Wenn eingabe Pfeil nach Oben ist verändere index dementsprechend 
                {
                    index = Game_Board.Slot.slots[index-1].possible_Moves[3].number;
                }
            } while(!(CKI.Key == ConsoleKey.Enter));  // Falls enter gedrückt  
            return index; // gebe position der Auswahl zurück
        }
        public void assign_new_Pin() // setzte Spielstein
        {
            currentPlayer = this;
            int index = 1;
            string errortext = "";
            do // Spieler wählt Slot aus auf den ein neuer Stein plaziert werden soll
            {
                index = choose_slot(index,0,errortext); // Bekomme Spielerauswahl
                if( index != 0 &&Game_Board.Slot.slots[index-1].player != null) errortext = "Achtung! es dürfen nur Spielsteine auf unbesetzte Felder plaziert werden."; // Falls feld nicht frei ist gebe Fehler aus
            }while(Game_Board.Slot.slots[index-1].player != null); // Wiederhole solange Slot nicht unbesetzt ist
            Game_Board.Slot.slots[index-1].player = this; // Setzte Spielstein auf Slot
        }
        public void play(bool jump)
        {
            currentPlayer = this;
            int index = 1;
            string errortext = "";
            bool turn_ended = false;
            do
            {
                do // Spieler wählt einen Slot aus den er verschieben will
                {
                    index = choose_slot(index,0,errortext); // Bekomme Spielerauswahl
                    if(Game_Board.Slot.slots[index-1].player != this) errortext = "Du darfst nur deine Steine verschieben"; // Wenn Spielstein nicht Spieler gehört gebe Errortext aus
                    else errortext = ""; // Wenn lösche error text
                }while(Game_Board.Slot.slots[index-1].player != this); // Wiederhole Solange Spielstein nicht Spieler gehört  
                Game_Board.Slot slot = Game_Board.Slot.slots[index-1]; // Erstelle Original Slot

                if (jump == false) // Überprüfe auf hüpfen bei den letzten 3 steinen || False = normaler Zug
                {
                    do // Spieler Wählt Slot aus auf den zuvor gewählten Spielstein verschieben will (auf möglichen zug)
                    {
                        index = choose_slot(index,slot.number,errortext); // Bekomme Spielerauswahl
                    }while(!((slot.possible_Moves.Contains(Game_Board.Slot.slots[index-1]) && Game_Board.Slot.slots[index-1].player == null) || Game_Board.Slot.slots[index-1] == slot)); // Wiederhole solange neuer Slot in mögliche Züge des originalslots ist und kein stein auf dem slot bereits steht oder der ausgewählte slot dem originalen etspricht   
                    if(Game_Board.Slot.slots[index-1] != slot) turn_ended = true; // wenn neuer slot nicht der originale ist beende Spielzug 
                    else turn_ended = false; // Wenn neuer Slot der originale Slot führe den Auswahl des Spielers des originalen Slots noch einmal durch
                    slot.player = null; // entferne Spieler von vorherigem Spielstein
                    Game_Board.Slot.slots[index-1].player = this; // Setze Spielstein des Spieler auf neues Feld
                }
                else // Überprüfe auf hüpfen bei den letzten 3 steinen || True = hüpfen erlaubt
                {
                    do // Spieler wählt Slot aus auf den er mit dem zuvor ausgewählten Spielstein hüpfen will
                    {
                        index = choose_slot(index,slot.number,"Du hast nur noch 3 Spielsteine. Du darfst nun hüpfen."+errortext); // Bekomme Spielerauswahl
                    }while(Game_Board.Slot.slots[index-1].player != null || Game_Board.Slot.slots[index-1] == slot); // Wiederhole solange der ausgewählte slot nicht "leer" ist oder der ausgewählte slot dem originalen etspricht
                    if(Game_Board.Slot.slots[index-1] != slot) turn_ended = true; // wenn neuer slot nicht der originale ist beende Spielzug 
                    else turn_ended = false; // Wenn neuer Slot der originale Slot führe den Auswahl des Spielers des originalen Slots noch einmal durch
                    slot.player = null; // entferne Spieler von vorherigem Spielstein
                    Game_Board.Slot.slots[index-1].player = this; // Setze Spielstein des Spieler auf neues Feld
                }
            } while(turn_ended == false); // Wiederhole solange zug nicht beeendet ist
            
        }
        public void remove_pin_form_other_player() // Nehme Spielstein eines anderen
        {
            Player? other_player = null; // Anderer Spiler
            bool anyfreePin = false;
            foreach(Player player in players) // Für jeden Spiler in Spieler liste
            {
                if(player != this) other_player = player; // Falls der Spieler nicht ich bin ist es mein Gegner
            }
            foreach(Game_Board.Slot slot in Game_Board.Slot.slots)
            {
                if(slot.player == other_player && Game_Board.Mill.check_Slot_for_Meuhle(slot.number) == false) anyfreePin = true;
            }
            if(anyfreePin == false)
            {
                choose_slot(1,0,"\n\tDo hast zwar eine Mühle Gemacht, jedoch besitzt dein Gegner keinen Stein den du ihm nehmen könntest!\n\t(Wähle irgendeinen Feld an um fortzufahren)");
            }
            else
            {
                int index = 1;
                string errortext = "\tDu hast eine Mühle gemacht.\n\t Du darfst nun einen Stein deines Gegners entfernen.\n\t(Der Stein darf nicht aus einer gegnerischen Mühle sein)";

                do // Spieler wählt den Slot des Gegners aus den er löschen will
                {
                    index = choose_slot(index,0,errortext); // Slotauswahl des spielers
                }while(!(Game_Board.Slot.slots[index-1].player == other_player && Game_Board.Mill.check_Slot_for_Meuhle(index) == false)); // Wiederhole solange der ausgewählte pin nicht ein Pin ist, ein Pin des Gegners ist und der pin nicht in eine mühle ist
                Game_Board.Slot.slots[index-1].player = null; // Setzte pin auf null
            }
            
        }
        public int get_slot_count()
        {
            int i = 0;
            foreach(Game_Board.Slot slot in Game_Board.Slot.slots)
            {
                if(slot.player == this) i++;
            }
            return i;
        }
    }
    public class Game_Board
    {
        public class Slot
        {
            public static List<Slot> slots {get; private set;} = new(); // Liste aller slots
            public Player? player {get; set;} // Spieler der einen stein auf dem slot hat | standart: Null
            public int number {get; private set;} // Nummer des Felds | startet bei 1
            public List<Slot> possible_Moves {get; private set;} = new(); // liste der möglichen Züge von diesem Punkt aus | Liste aus 4 elementen 0: nach oben 1: nach rechts 2 : nach unten 3: nach links | wenn element 0 ist dann ist der zug nicht möglich
            public static Slot null_Slot {get; private set;} = new Slot(0); // Statischer null slot für nicht mögliche züge
            public Slot(int number) // Konstruktor
            {
                this.player = null; // Feld hat keine Spieler bei Erstellung des Slots
                this.number = number; 
                this.possible_Moves = []; // Feld der möglichen Zügen ist bei beginn leer
            }
            public void print(string game_char,string standard_char,int index,int index2) // Einzelnes spielfeld ausgeben
            {
                if(index == number)
                {
                    if(player != null) Program.print_colored(game_char,player.color.consolecolor,Program.selected_background_color); // Falls Slot einen spieler hat gebe das Gamesymbol in der Farbe des Spielers aus | Hintergrund ist dabei gehighlited wenn feld ausgewählt ist
                    else Program.print_colored(standard_char,Program.standard_foreground_color,Program.selected_background_color); // Falls Slot keinen Spieler hat gebe Standartzeichen des Slots in Standartfarbe aus | Hintergrund ist dabei gehighlited wenn feld ausgewählt ist
                }
                else if(index2 == number)
                {
                    if(player != null) Program.print_colored(game_char,player.color.consolecolor,Program.selected2_background_color); // Falls Slot einen spieler hat gebe das Gamesymbol in der Farbe des Spielers aus | Hintergrund ist dabei gehighlited wenn feld ausgewählt ist
                    else Program.print_colored(standard_char,Program.standard_foreground_color,Program.selected2_background_color); // Falls Slot keinen Spieler hat gebe Standartzeichen des Slots in Standartfarbe aus | Hintergrund ist dabei gehighlited wenn feld ausgewählt ist
                }
                else{
                    if(player != null)Program.print_colored(game_char,player.color.consolecolor,Program.standard_background_color); // Falls Slot einen spieler hat gebe das Gamesymbol in der Farbe des Spielers aus
                    else Console.Write(standard_char); // Gebe Standartzeichen des Slots aus
                }
            }
            public void set_possible_Moves(List<int> numbers) // Füge mögliche Spielzüge hinzu
            {
                foreach(int number in numbers) // Für Jede Slotnummer in Liste
                {
                    if(number !=0) this.possible_Moves.Add(slots[number-1]); // Füge diesen slot hinzu
                    else possible_Moves.Add(null_Slot); // wenn nummer 0 ist füge null für eine nciht mögliche Option hinzu
                }
            }
            public static void initialize_slots() // Inizialisiere Slots mit möglichen Spielzügen
            {
                for(int i = 0; i < 24; i++) // Erstelle 24 Slots
                {
                    slots.Add(new Slot(i+1)); // 24 Slots erstellen und nummerieren
                }
                
                // Slot Verzeichniss
                slots[0].set_possible_Moves([0,2,8,0]); // Slot Nummer: 1 Mögliche Spielzüge = 2,8
                slots[1].set_possible_Moves([0,3,10,1]); // Slot Nummer: 2 Mögliche Spielzüge = 1,3,10
                slots[2].set_possible_Moves([0,0,4,2]); // Slot Nummer: 3 Mögliche Spielzüge = 2,4
                slots[3].set_possible_Moves([3,0,5,12]); // Slot Nummer: 4 Mögliche Spielzüge = 3,5,12
                slots[4].set_possible_Moves([4,0,0,6]); // Slot Nummer: 5 Mögliche Spielzüge = 4,6
                slots[5].set_possible_Moves([14,5,0,7]); // Slot Nummer: 6 Mögliche Spielzüge = 5,7,14
                slots[6].set_possible_Moves([8,6,0,0]); // Slot Nummer: 7 Mögliche Spielzüge = 6,8
                slots[7].set_possible_Moves([1,16,7,0]); // Slot Nummer: 8 Mögliche Spielzüge = 1,16,7
                slots[8].set_possible_Moves([0,10,16,0]); // Slot Nummer: 9 Mögliche Spielzüge = 10,16
                slots[9].set_possible_Moves([2,11,18,9]); // Slot Nummer: 10 Mögliche Spielzüge = 2,9,11,18
                slots[10].set_possible_Moves([0,0,12,10]); // Slot Nummer: 11 Mögliche Spielzüge = 10,12
                slots[11].set_possible_Moves([11,4,13,20]); // Slot Nummer: 12 Mögliche Spielzüge = 4,11,13,20
                slots[12].set_possible_Moves([12,0,0,14]); // Slot Nummer: 13 Mögliche Spielzüge = 12,14
                slots[13].set_possible_Moves([22,13,6,15]); // Slot Nummer: 14 Mögliche Spielzüge = 6,13,15,22
                slots[14].set_possible_Moves([16,14,0,0]); // Slot Nummer: 15 Mögliche Spielzüge = 14,16
                slots[15].set_possible_Moves([9,24,15,8]); // Slot Nummer: 16 Mögliche Spielzüge = 8,9,15,24
                slots[16].set_possible_Moves([0,18,24,0]); // Slot Nummer: 17 Mögliche Spielzüge = 18,24
                slots[17].set_possible_Moves([10,19,0,17]); // Slot Nummer: 18 Mögliche Spielzüge = 10,17,19
                slots[18].set_possible_Moves([0,0,20,18]); // Slot Nummer: 19 Mögliche Spielzüge = 18,20
                slots[19].set_possible_Moves([19,12,21,0]); // Slot Nummer: 20 Mögliche Spielzüge = 12,19,21
                slots[20].set_possible_Moves([20,0,0,22]); // Slot Nummer: 21 Mögliche Spielzüge = 20,22
                slots[21].set_possible_Moves([0,21,14,23]); // Slot Nummer: 22 Mögliche Spielzüge = 14,21,23
                slots[22].set_possible_Moves([24,22,0,0]); // Slot Nummer: 23 Mögliche Spielzüge = 22,24
                slots[23].set_possible_Moves([17,0,23,16]); // Slot Nummer: 24 Mögliche Spielzüge = 16,17,23
            }
        }
        public class Combination
        {
            public List<Slot> slots {get; private set;} = new(); // Slot liste der beinhalteten Slots
            public static List<Combination> combinations {get; private set;} = new(); // Statische Liste aller Kombinationen
            public Combination(List<Slot> slots) // Konstruktor
            {
                this.slots = slots;
                combinations.Add(this); // Füge Slot zu Slotliste hinzu
            }
            public bool check() // überprüfen ob Kombination eine Mühle ist
            {
                bool status = false;
                if(slots[0].player== slots[1].player && slots[1].player == slots[2].player && slots[0].player == slots[2].player && slots[0].player != null && slots[1].player != null && slots[2].player != null) // Wenn alle 3 Felder den gleichen Spieler beinhalten 
                {
                    status = true;
                }
                return status;
            }
            public static void initialize_combinations() // Kombinationen Initialisieren
            {
                new Combination([Slot.slots[0],Slot.slots[1],Slot.slots[2]]);
                new Combination([Slot.slots[2],Slot.slots[3],Slot.slots[4]]);
                new Combination([Slot.slots[4],Slot.slots[5],Slot.slots[6]]);
                new Combination([Slot.slots[6],Slot.slots[7],Slot.slots[0]]);
                new Combination([Slot.slots[8],Slot.slots[9],Slot.slots[10]]);
                new Combination([Slot.slots[10],Slot.slots[11],Slot.slots[12]]);
                new Combination([Slot.slots[12],Slot.slots[13],Slot.slots[14]]);
                new Combination([Slot.slots[14],Slot.slots[15],Slot.slots[8]]);
                new Combination([Slot.slots[16],Slot.slots[17],Slot.slots[18]]);
                new Combination([Slot.slots[18],Slot.slots[19],Slot.slots[20]]);
                new Combination([Slot.slots[20],Slot.slots[21],Slot.slots[22]]);
                new Combination([Slot.slots[22],Slot.slots[23],Slot.slots[16]]);
                new Combination([Slot.slots[1],Slot.slots[9],Slot.slots[17]]);
                new Combination([Slot.slots[3],Slot.slots[11],Slot.slots[19]]);
                new Combination([Slot.slots[5],Slot.slots[13],Slot.slots[21]]);
                new Combination([Slot.slots[7],Slot.slots[15],Slot.slots[23]]);
            }
        }
        public class Mill
        {
            public static List<Mill> existing_Mills {get; private set;} = new(); // Liste aller Mühlem auf dem Spielfeld
            public Combination combination {get; private set;} // Kombination der Mühle
            public Mill(Combination combination) // Konstruktor
            {
                this.combination = combination;
                existing_Mills.Add(this); // Füge Mühle zur Liste der existiedenden Mühlen hinzu
            }
            public static int check() // Überprüfe ob alte mühlen weiter bestehen un suche nach neuen Mühlen || Wenn Mühle gefundenwurde gebe  
            {
                List<Mill> delList = new(); // Liste der zu löschenden Listen
                foreach(Mill mill in existing_Mills) // für jede Mühle die existiert
                {
                    if(mill.combination.check() == false) delList.Add(mill); // wenn combination der mühle nicht mehr erüllt ist, füge diese zur Löschungsliste hinzu
                }
                foreach(Mill mill in delList) // für jede Mühle in der Löschungsliste
                {
                    existing_Mills.Remove(mill); // lösche Mühle
                }

                int status = 0; // anzahl der neu gefundenen Mühlen
                foreach(Combination combination in Combination.combinations) // Für jede Kombination an Mühlen
                {
                    if(combination.check() == true) // Falls Kombination noch eine Mühle ist
                    {
                        List<Combination> all_combinations_used = new(); // Liste aller benutzten Muhlen
                        foreach(Mill mill in existing_Mills) // Füge jede Mühle der benutzten Mühlen hinzu
                        {
                            all_combinations_used.Add(mill.combination); // Füge Mühle zu bereits existierenden Mühlen hinzu 
                        }
                        if(all_combinations_used.Contains(combination) == false) // überprüfe ob eine neue Mühle unter den bereits existierenden Mühlen ist
                        {
                            new Mill(combination); // füge neue Mühle hinzu
                            status++; // inkrementiere Anzahl der gefundenen Mühlen
                        }
                    }
                }
                return status; // Gebe Anzahl der gefundenen Mühlen zurück
            }
            public static bool check_Slot_for_Meuhle(int index) // überprüfe ob Pin in einer Mühle ist
            {
                bool status = false;
                foreach(Mill mill in existing_Mills) // Für jede Mühle in den existierenden Mühlen
                {
                    if (mill.combination.slots.Contains(Slot.slots[index-1]) == true) // falls index in eriner bestehenden mühle ist
                    {
                        status = true; // gebe true zurück
                    }
                }
                return status;
            } 
        }
        public static string gamesymbol {get; private set;}= "●"; // Symbol des Spielsteines
        public Game_Board() // Konstruktor
        {
            Slot.initialize_slots();// Sobald das Spielbrett erstellt ist, inizialisieren sich die Slots
            Combination.initialize_combinations(); // Inizialisier mögliche mühlen kombinationen
        }
        public void print(int index,int index2) // Gebe Spielbrett aus
        {
            Console.Write("\n\n");// absatz
            Console.Write("\n\t");Slot.slots[0].print(gamesymbol,"┌",index,index2);Console.Write("────────");Slot.slots[1].print(gamesymbol,"┬",index,index2);Console.Write("────────");Slot.slots[2].print(gamesymbol,"┐",index,index2);                                                                                                                                                                                                                       // ┌────────┬────────┐
            Console.Write("\n\t│  ");Slot.slots[8].print(gamesymbol,"┌",index,index2);Console.Write("─────");Slot.slots[9].print(gamesymbol,"┼",index,index2);Console.Write("─────");Slot.slots[10].print(gamesymbol,"┐",index,index2);Console.Write("  │");                                                                                                                                                                                                    // │  ┌─────┼─────┐  │
            Console.Write("\n\t│  │  ");Slot.slots[16].print(gamesymbol,"┌",index,index2);Console.Write("──");Slot.slots[17].print(gamesymbol,"┴",index,index2);Console.Write("──");Slot.slots[18].print(gamesymbol,"┐",index,index2);Console.Write("  │  │");                                                                                                                                                                                                  // │  │  ┌──┴──┐  │  │
            Console.Write("\n\t");Slot.slots[7].print(gamesymbol,"├",index,index2);Console.Write("──");Slot.slots[15].print(gamesymbol,"┼",index,index2);Console.Write("──");Slot.slots[23].print(gamesymbol,"┤",index,index2);Console.Write("     ");Slot.slots[19].print(gamesymbol,"├",index,index2);Console.Write("──");Slot.slots[11].print(gamesymbol,"┼",index,index2);Console.Write("──");Slot.slots[3].print(gamesymbol,"┤",index,index2);             // ├──┼──┤     ├──┼──┤
            Console.Write("\n\t│  │  ");Slot.slots[22].print(gamesymbol,"└",index,index2);Console.Write("──");Slot.slots[21].print(gamesymbol,"┬",index,index2);Console.Write("──");Slot.slots[20].print(gamesymbol,"┘",index,index2);Console.Write("  │  │");                                                                                                                                                                                                  // │  │  └──┬──┘  │  │
            Console.Write("\n\t│  ");Slot.slots[14].print(gamesymbol,"└",index,index2);Console.Write("─────");Slot.slots[13].print(gamesymbol,"┼",index,index2);Console.Write("─────");Slot.slots[12].print(gamesymbol,"┘",index,index2);Console.Write("  │");                                                                                                                                                                                                  // │  └─────┼─────┘  │
            Console.Write("\n\t");Slot.slots[6].print(gamesymbol,"└",index,index2);Console.Write("────────");Slot.slots[5].print(gamesymbol,"┴",index,index2);Console.Write("────────");Slot.slots[4].print(gamesymbol,"┘",index,index2);                                                                                                                                                                                                                       // └────────┴────────┘
        }
    }
    
    private Game_Board game_board {get;set;}
    public Game()
    {
        Player.Color playercolor_red = new Player.Color("Rot", ConsoleColor.Red); // Erstelle Spielerfarben
        Player.Color playercolor_blue = new Player.Color("Blau", ConsoleColor.Blue); // Erstelle Spielerfarben
        Player.Color playercolor_green = new Player.Color("Grün", ConsoleColor.Green); // Erstelle Spielerfarben
        Player.Color playercolor_yellow = new Player.Color("Gelb", ConsoleColor.Yellow); // Erstelle Spielerfarben

        game_board = new Game_Board(); // Erstelle Gameboard

        new Player(game_board,1); // Erstelle Spieler 1
        new Player(game_board,2); // Erstelle Spieler 2
    }
    public void start() // Starte das Spiel
    {
        for(int i = 0; i < 9; i++) // Beginne Spiel damit, dass beide Spleier ihre 9 Steine Setzen dürfen
        {   
            foreach(Player player in Player.players) // Jeder Spieler
            {
                player.assign_new_Pin(); // lasse Spieler einen Stein seten
                int found_Muehles = Game_Board.Mill.check(); // überprüfe ob er eine neue Mühle gemacht hat bzw ob allte Mühlen noch da sind
                while(found_Muehles >= 1) // Für jeder Mühle die er gemacht hat
                {
                    player.remove_pin_form_other_player(); // Entferne ein Spielstein des Gegners
                    found_Muehles--; // dekrementiere die anzahl der gefundenen Mühlen
                }
            }
        }
        do
        {
            foreach(Player player in Player.players) // Jeder Spieler
            {
                if(player.get_slot_count() == 3)player.play(true); // wenn Spieler über 3 Spielsteine hat spielt er normal (ohne hüpfen)
                else player.play(false); // Wenn Spieler 3 Spielsteine hat darf er hüpfen

                int found_Muehles = Game_Board.Mill.check(); // überprüfe ob er eine neue Mühle gemacht hat bzw ob allte Mühlen noch da sind
                while(found_Muehles >= 1) // Für jeder Mühle die er gemacht hat
                {
                    player.remove_pin_form_other_player(); // Entferne ein Spielstein des Gegners
                    found_Muehles--; // dekrementiere die anzahl der gefundenen Mühlen
                }
            }
        }while(Player.players[0].get_slot_count() >= 3 && Player.players[1].get_slot_count() >= 3); // Widerhole solange beider Spieler min 3 Spielsteine haben

        Player? Winner = null;
        if(Player.players[0].get_slot_count() > Player.players[1].get_slot_count()) Winner = Player.players[0];
        else if(Player.players[0].get_slot_count() < Player.players[1].get_slot_count()) Winner = Player.players[1];


        if(Winner != null)Console.WriteLine("\n\n\t"+Winner.name+" hat gewonnen! \n\n\tDrücke eine Taste um das Spiel zu beenden");
        Console.Read();
    }
}
class Program
{
    public static class Logo
    {
        private static string logo = "\t    __  __   _   _   _       _\n\t   |  \\/  | (_) (_) | |     | |\n\t   | \\  / |  _   _  | |__   | |   ___\n\t   | |\\/| | | | | | | '_ \\  | |  / _ \\\n\t   | |  | | | |_| | | | | | | | |  __/\n\t   |_|  |_|  \\__,_| |_| |_| |_|  \\___|";
        public static void print()
        {
            Console.Clear();
            Console.WriteLine(logo);
        }
    }
    static public ConsoleColor selected_foreground_color {get; private set;} = ConsoleColor.Black; // Ausgewählte Fordergrundfarbe
    static public ConsoleColor selected_background_color {get; private set;} = ConsoleColor.Gray; // Ausgewählte Hintergrundfarbe
    static public ConsoleColor selected2_foreground_color {get; private set;} = ConsoleColor.White; // Ausgewählte Fordergrundfarbe 2
    static public ConsoleColor selected2_background_color {get; private set;} = ConsoleColor.DarkGray; // Ausgewählte Hintergrundfarbe 2
    static public ConsoleColor standard_foreground_color {get; private set;} = ConsoleColor.White; // Standart Fordergrundfarbe
    static public ConsoleColor standard_background_color {get; private set;} = ConsoleColor.Black; // Standart Hintergrundfarbe
    static public void print_colored(string text, ConsoleColor foregroundcolor, ConsoleColor backgroundcolor) // Gebe Text farbig aus 
    {
        Console.ForegroundColor = foregroundcolor; // Setze ausgewählte Fordergrundfarbe
        Console.BackgroundColor = backgroundcolor; // Setzte ausgewählte Hintergrundfarbe
        Console.Write(text); // Gebe Text aus
        Console.ForegroundColor = standard_foreground_color; // Setzte standart Fordergrundfarbe
        Console.ResetColor();
    }

    public class Menu
    {
        public List<string> options {get; set;} = new(); 
        private string title {get; set;} 
        private int index {get; set;} = 0; 
        public Menu(string title,List<string> options) 
        {
            this.title = title;
            this.options = options;
        }
        private void print() 
        {
            Logo.print();
            if(title != "") Console.Write("\n\t"+title); 
            foreach(string option in options) 
            {
                if(index == options.IndexOf(option)) {Console.Write("\n\t\t\t");Program.print_colored(option,Program.selected_foreground_color,Program.selected_background_color);}  
                else Console.Write("\n\t\t\t"+option); 
            }
        }
        public int start() 
        {
            ConsoleKeyInfo CKI;
            do
            {
                print(); 
                CKI = Console.ReadKey(false); 
                if(CKI.Key == ConsoleKey.UpArrow && index > 0 ) index--;
                else if (CKI.Key == ConsoleKey.DownArrow && index < (options.Count-1)) index++;
            } while(CKI.Key != ConsoleKey.Enter); 
            return index; 
        }
    }
    static void play()
    {
        Menu playMenu = new Menu("",["Einzelspieler","Mehrspieler","Zurück"]);
        switch (playMenu.start())
        {
            case 0: Console.Clear();Console.Write("\n\t\tIn arbeit!"); Console.ReadKey();break;
            case 1: Console.Clear();Game multiplayerGame = new Game();multiplayerGame.start();break;
            case 2: break;
        }
    }
    static void rules()
    {
        Logo.print();
        Console.WriteLine("\n\t- Ziel des Spiels: Den Gegner auf zwei Steine reduzierenoder ihn so blockieren, dass er keinen gültigen Zug mehr machen kann.");
        Console.WriteLine("\t- Phase 1 (Setzen): Die Spieler setzen abwechselnd ihre 9 Steineauf die Kreuzungspunkte und Ecken des Spielbretts.");
        Console.WriteLine("\t- Phase 2 (Ziehen): Wenn alle Steine gesetzt sind, wirdabwechselnd ein Stein auf einen angrenzenden, freien Punkt entlang der Linien bewegt.");
        Console.WriteLine("\t- Die Mühle: Drei Steine der eigenen Farbe in einer geradenLinie bilden eine Mühle.");
        Console.WriteLine("\t- Stein schlagen: Wer eine Mühle schließt, darf einen Steindes Gegners vom Brett nehmen.");
        Console.WriteLine("\t- Schutzregel: Steine aus einer geschlossenen Mühle des Gegnersdürfen nicht entfernt werden, es sei denn, er hat nur noch Steine in Mühlen.");
        Console.WriteLine("\t- Phase 3 (Springen): Sobald ein Spieler nur noch 3 Steine hat,darf er mit diesen frei auf jeden beliebigen freien Punkt auf dem Brett springen.");
        Console.WriteLine("\t- Spielende: Das Spiel endet, wenn ein Spieler nur noch 2 Steinehat oder keine Züge mehr ausführen kann.\n");
        print_colored("\tZurück",selected_foreground_color,selected_background_color);
        Console.ReadKey();
    }
    static void credits()
    {
        
    }
    static void exit()
    {
        Menu exitMenu = new Menu("  Willst du das Spiel wirklich beenden?\n",["Nein","Ja"]);
        switch (exitMenu.start())
        {
            case 0: break;
            case 1: running = false;break;
        }
    }
    static bool running = true;
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8; // Setze Ausgabeversion auf UTF8
        
        while (running)
        {
            Menu mainMenu = new Menu("",["Spielen","Regeln","Credits","Beenden"]);        
            switch (mainMenu.start())
            {
                case 0: play();break;
                case 1: rules();break;
                case 2: credits();break;
                case 3: exit();break;
            }
        }
        
    }
}

