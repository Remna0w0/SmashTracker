using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SmashExpTracker
{
    public class DatabaseService
    {
        static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _dbPath = Path.Combine(baseDir, "SmashExpTracker.db");

        public DatabaseService() { Initialize(); }

        private void Initialize()
        {
            string directory = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            string initialTable = @"
                CREATE TABLE IF NOT EXISTS Characters (
                    CharacterName TEXT PRIMARY KEY,
                    Wins INTEGER DEFAULT 0,
                    Losses INTEGER DEFAULT 0,
                    GamesPlayed INTEGER DEFAULT 0,
                    Winrate INTEGER DEFAULT 0,
                    Notes TEXT DEFAULT ''
                );


                INSERT OR IGNORE INTO Characters (CharacterName)
                    VALUES
                        ('MARIO'),
                        ('DONKEY KONG'),
                        ('LINK'),
                        ('SAMUS'),
                        ('DARK SAMUS'),
                        ('YOSHI'),
                        ('KIRBY'),
                        ('FOX'),
                        ('PIKACHU'),
                        ('LUIGI'),
                        ('NESS'),
                        ('CAPTAIN FALCON'),
                        ('JIGGLYPUFF'),
                        ('PEACH'),
                        ('DAISY'),
                        ('BOWSER'),
                        ('ICE CLIMBERS'),
                        ('SHEIK'),
                        ('ZELDA'),
                        ('DR. MARIO'),
                        ('PICHU'),
                        ('FALCO'),
                        ('MARTH'),
                        ('LUCINA'),
                        ('YOUNG LINK'),
                        ('GANONDORF'),
                        ('MEWTWO'),
                        ('ROY'),
                        ('CHROM'),
                        ('MR. GAME & WATCH'),
                        ('META KNIGHT'),
                        ('PIT'),
                        ('DARK PIT'),
                        ('ZERO SUIT SAMUS'),
                        ('WARIO'),
                        ('SNAKE'),
                        ('IKE'),
                        ('POKEMON TRAINER'),
                        ('DIDDY KONG'),
                        ('LUCAS'),
                        ('SONIC'),
                        ('KING DEDEDE'),
                        ('OLIMAR'),
                        ('LUCARIO'),
                        ('R.O.B.'),
                        ('TOON LINK'),
                        ('WOLF'),
                        ('VILLAGER'),
                        ('MEGA MAN'),
                        ('WII FIT TRAINER'),
                        ('ROSALINA & LUMA'),
                        ('LITTLE MAC'),
                        ('GRENINJA'),
                        ('MII BRAWLER'),
                        ('MII SWORDFIGHTER'),
                        ('MII GUNNER'),
                        ('PALUTENA'),
                        ('PAC-MAN'),
                        ('ROBIN'),
                        ('SHULK'),
                        ('BOWSER JR.'),
                        ('DUCK HUNT'),
                        ('RYU'),
                        ('KEN'),
                        ('CLOUD'),
                        ('CORRIN'),
                        ('BAYONETTA'),
                        ('INKLING'),
                        ('RIDLEY'),
                        ('SIMON'),
                        ('RICHTER'),
                        ('KING K. ROOL'),
                        ('ISABELLE'),
                        ('INCINEROAR'),
                        ('PIRANHA PLANT'),
                        ('JOKER'),
                        ('HERO'),
                        ('BANJO & KAZOOIE'),
                        ('TERRY'),
                        ('BYLETH'),
                        ('MIN MIN'),
                        ('STEVE'),
                        ('SEPHIROTH'),
                        ('PYRA & MYTHRA'),
                        ('KAZUYA'),
                        ('SORA');";

            connection.Execute(initialTable);



        }

        public IDbConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={_dbPath}");
        }
        public List<CharacterModel> GetAllCharacters()
        {
            using var db = this.GetConnection();
            string query = "SELECT CharacterName FROM Characters ORDER BY CharacterName ASC";

            List<string> names = db.Query<string>(query).ToList();

            List<CharacterModel> characterList = new List<CharacterModel>();
            string iconDictionary = Path.Combine(baseDir, "Icons");

            foreach (string name in names)
            {
                characterList.Add(new CharacterModel
                {
                    Name = name,

                    ImagePath = Path.Combine(iconDictionary, $"{name.ToLower()}.png")
                });
                    
            }

            return characterList;

        }

        public void AddWin(string character)
        {
            string winScript = @"UPDATE Characters SET Wins = Wins + 1, GamesPlayed = GamesPlayed + 1 WHERE CharacterName = @name";

            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            connection.Execute(winScript, new
            {
                name = character
            });
            Console.WriteLine($"Added a won match to {character}");
        }

        public void DelWin(string character)
        {
            string winScript = @"UPDATE Characters SET Wins = Wins - 1, GamesPlayed = GamesPlayed - 1 WHERE CharacterName = @name";

            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            connection.Execute(winScript, new
            {
                name = character
            });
            Console.WriteLine($"Deleted a won match from {character}");
        }

        public void AddLose(string character)
        {
            string loseScript = @"UPDATE Characters SET Losses = Losses + 1, GamesPlayed = GamesPlayed + 1 WHERE CharacterName = @name";

            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            connection.Execute(loseScript, new
            {
                name = character
            });
            Console.WriteLine($"Added a lost match to {character}");
        }

        public void DelLose(string character)
        {
            string loseScript = @"UPDATE Characters SET Losses = Losses - 1, GamesPlayed = GamesPlayed - 1 WHERE CharacterName = @name";

            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            connection.Execute(loseScript, new
            {
                name = character
            });
            Console.WriteLine($"Added a lost match to {character}");
        }

        public int GetWins(string character)
        {
            using var db = this.GetConnection();

            string getWins = "SELECT Wins FROM Characters WHERE CharacterName = @name";
            return db.ExecuteScalar<int>(getWins, new { name = character });
            
        }


        public int GetLosses(string character)
        {
            using var db = this.GetConnection();

            string getLosses = "SELECT Losses FROM Characters WHERE CharacterName = @name";
            return db.ExecuteScalar<int>(getLosses, new { name = character });
        }

        public string GetNotes(string character)
        {
            using var db = this.GetConnection();

            string getNotes = "SELECT Notes FROM Characters WHERE CharacterName = @name";
            return db.ExecuteScalar<string>(getNotes, new { name = character }) ?? "";

        }

        public void SaveNotes(string character, string _notes)
        {
            using var db = this.GetConnection();
            string save = "UPDATE Characters SET Notes = @notes WHERE CharacterName = @name";

            db.Execute(save, new { name = character, notes = _notes });
        }

        public void CalculateWinrate(string character)
        {
            using var db = this.GetConnection();

            string getTotal = "SELECT GamesPlayed FROM Characters WHERE CharacterName = @name";
            int totalGames = db.ExecuteScalar<int>(getTotal, new { name = character });
            if (totalGames == 0) { return; }

            string getWins = "SELECT Wins FROM Characters WHERE CharacterName = @name";
            int totalWins = db.ExecuteScalar<int>(getWins, new { name = character });
            if (totalWins == 0) { return; }

            int winrate = (totalWins * 100) / totalGames;

            string winrateScript = @"Update Characters SET Winrate = @_winrate WHERE CharacterName = @name";
            db.Execute(winrateScript, new
            {
                name = character,
                _winrate = winrate
            });

        }

        public int GetWinrate(string character)
        {
            using var db = this.GetConnection();

            string getWinrate = "SELECT Winrate FROM Characters WHERE CharacterName = @name";
            int winrate = db.ExecuteScalar<int>(getWinrate, new { name = character });

            return winrate;
        }

        public void SyncWinrates()
        {
            Console.WriteLine("Syncing all winrates...");
            using var db = this.GetConnection();
            string getCharacters = "SELECT CharacterName FROM Characters";
            List<string> characters = new List<string>();
            using (var reader = db.ExecuteReader(getCharacters))
            {
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        characters.Add(reader.GetString(0));
                    }
                }
            }

            db.Open();
            try
            {
                foreach (var character in characters)
                {

                    CalculateWinrate(character);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("Done!");
        }

        public class CharacterModel
        {
            public string Name { get; set; }
            public string ImagePath { get; set; }
        }
    }
}