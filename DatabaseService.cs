using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

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
            connection.Open(); // Explicitly open the connection

            // Correctly check if 'Wins' column exists by selecting the column names specifically
            var columns = connection.Query<string>("SELECT name FROM pragma_table_info('Characters');").ToList();
            bool needsMigration = columns.Contains("Wins");

            if (needsMigration)
            {
                // Safely drop any conflicting temp table if an old crash left it behind
                connection.Execute("DROP TABLE IF EXISTS Old_Characters;");
                connection.Execute("ALTER TABLE Characters RENAME TO Old_Characters;");
            }

            string initialTable = @"
        CREATE TABLE IF NOT EXISTS Characters (
            CharacterName TEXT PRIMARY KEY
        );

        CREATE TABLE IF NOT EXISTS Matchups (
            PlayerCharacter TEXT,
            OpponentCharacter TEXT,
            Wins INTEGER DEFAULT 0,
            Losses INTEGER DEFAULT 0,
            GamesPlayed INTEGER DEFAULT 0,
            Winrate INTEGER DEFAULT 0,
            Notes TEXT DEFAULT '',
            PRIMARY KEY (PlayerCharacter, OpponentCharacter),
            FOREIGN KEY (PlayerCharacter) REFERENCES Characters(CharacterName),
            FOREIGN KEY (OpponentCharacter) REFERENCES Characters(CharacterName)
        );

        INSERT OR IGNORE INTO Characters (CharacterName)
        VALUES
            ('MARIO'), ('DONKEY KONG'), ('LINK'), ('SAMUS'), ('DARK SAMUS'),
            ('YOSHI'), ('KIRBY'), ('FOX'), ('PIKACHU'), ('LUIGI'),
            ('NESS'), ('CAPTAIN FALCON'), ('JIGGLYPUFF'), ('PEACH'), ('DAISY'),
            ('BOWSER'), ('ICE CLIMBERS'), ('SHEIK'), ('ZELDA'), ('DR. MARIO'),
            ('PICHU'), ('FALCO'), ('MARTH'), ('LUCINA'), ('YOUNG LINK'),
            ('GANONDORF'), ('MEWTWO'), ('ROY'), ('CHROM'), ('MR. GAME & WATCH'),
            ('META KNIGHT'), ('PIT'), ('DARK PIT'), ('ZERO SUIT SAMUS'), ('WARIO'),
            ('SNAKE'), ('IKE'), ('POKEMON TRAINER'), ('DIDDY KONG'), ('LUCAS'),
            ('SONIC'), ('KING DEDEDE'), ('OLIMAR'), ('LUCARIO'), ('R.O.B.'),
            ('TOON LINK'), ('WOLF'), ('VILLAGER'), ('MEGA MAN'), ('WII FIT TRAINER'),
            ('ROSALINA & LUMA'), ('LITTLE MAC'), ('GRENINJA'), ('MII BRAWLER'),
            ('MII SWORDFIGHTER'), ('MII GUNNER'), ('PALUTENA'), ('PAC-MAN'), ('ROBIN'),
            ('SHULK'), ('BOWSER JR.'), ('DUCK HUNT'), ('RYU'), ('KEN'),
            ('CLOUD'), ('CORRIN'), ('BAYONETTA'), ('INKLING'), ('RIDLEY'),
            ('SIMON'), ('RICHTER'), ('KING K. ROOL'), ('ISABELLE'), ('INCINEROAR'),
            ('PIRANHA PLANT'), ('JOKER'), ('HERO'), ('BANJO & KAZOOIE'), ('TERRY'),
            ('BYLETH'), ('MIN MIN'), ('STEVE'), ('SEPHIROTH'), ('PYRA & MYTHRA'),
            ('KAZUYA'), ('SORA'), ('UNKNOWN');"; 

            connection.Execute(initialTable);

            if (needsMigration)
            {
                string migrationScript = @"
            INSERT INTO Matchups (PlayerCharacter, OpponentCharacter, Wins, Losses, GamesPlayed, Winrate, Notes)
            SELECT 'UNKNOWN', CharacterName, Wins, Losses, GamesPlayed, Winrate, Notes
            FROM Old_Characters
            WHERE GamesPlayed > 0 OR Notes != '';";

                connection.Execute(migrationScript);
                connection.Execute("DROP TABLE Old_Characters;");
            }
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

        public int GetTotalWins(string character)
        {
            using var db = this.GetConnection();
            string winTotalScript = "SELECT SUM(Wins) FROM Matchups WHERE PlayerCharacter = @player";
            return db.ExecuteScalar<int>(winTotalScript, new { player = character });
        }

        public int GetTotalLosses(string character)
        {
            using var db = this.GetConnection();
            string lossTotalScript = "SELECT SUM(Losses) FROM Matchups WHERE PlayerCharacter = @player";
            return db.ExecuteScalar<int>(lossTotalScript, new { player = character });
        }

        public int CalculateTotalWinrate(string character)
        {
            using var db = this.GetConnection();
            int totalLosses = GetTotalLosses(character);
            int totalWins = GetTotalWins(character);
            int totalGames = totalWins + totalLosses;

            int totalWinrate = (totalWins *= 100) / totalGames;

            return totalWinrate;       
        }

        public void AddWin(string character, string vsCharacter)
        {
            string winScript = @"
                INSERT INTO Matchups (PlayerCharacter, OpponentCharacter, Wins, GamesPlayed)
                VALUES (@player, @opponent, 1, 1)
                ON CONFLICT(PlayerCharacter, OpponentCharacter) DO UPDATE SET
                    Wins = Wins + 1,
                    GamesPlayed = GamesPlayed + 1;";

            using var db = this.GetConnection();
            db.Execute(winScript, new { player = character, opponent = vsCharacter });
            Console.WriteLine($"Added a won match Vs. {vsCharacter} with {character}");
        }

        public void DelWin(string character, string vsCharacter)
        {
            if (GetWins(character, vsCharacter) == 0) { return; }
            string delWinScript = @"
                UPDATE Matchups 
                SET Wins = MAX(0, Wins - 1), 
                    GamesPlayed = MAX(0, GamesPlayed - 1) 
                WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent;";

            using var db = this.GetConnection();
            db.Execute(delWinScript, new { player = character, opponent = vsCharacter });
            Console.WriteLine($"Deleted a won match Vs. {vsCharacter} from {character}");
        }

        public void AddLose(string character, string vsCharacter)
        {
            string loseScript = @"
                INSERT INTO Matchups (PlayerCharacter, OpponentCharacter, Losses, GamesPlayed)
                VALUES (@player, @opponent, 1, 1)
                ON CONFLICT(PlayerCharacter, OpponentCharacter) DO UPDATE SET
                    Losses = Losses + 1,
                    GamesPlayed = GamesPlayed + 1;";

            using var db = this.GetConnection();
            db.Execute(loseScript, new { player = character, opponent = vsCharacter });
            Console.WriteLine($"Added a lost match Vs. {vsCharacter} with {character}");
        }

        public void DelLose(string character, string vsCharacter)
        {
            if (GetLosses(character, vsCharacter) == 0) {  return; }
            string delLoseScript = @"
                UPDATE Matchups 
                SET Losses = MAX(0, Losses - 1), 
                    GamesPlayed = MAX(0, GamesPlayed - 1) 
                WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent;";

            using var db = this.GetConnection();
            db.Execute(delLoseScript, new { player = character, opponent = vsCharacter });
            Console.WriteLine($"Deleted a lost match Vs. {vsCharacter} from {character}");
        }

        public int GetWins(string character, string vsCharacter)
        {
            using var db = this.GetConnection();
            string getWins = "SELECT Wins FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            return db.ExecuteScalar<int>(getWins, new { player = character, opponent = vsCharacter });
        }

        public int GetLosses(string character, string vsCharacter)
        {
            using var db = this.GetConnection();
            string getLosses = "SELECT Losses FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            return db.ExecuteScalar<int>(getLosses, new { player = character, opponent = vsCharacter });
        }

        public string GetNotes(string character, string vsCharacter)
        {
            using var db = this.GetConnection();
            string getNotes = "SELECT Notes FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            return db.ExecuteScalar<string>(getNotes, new { player = character, opponent = vsCharacter }) ?? "";
        }

        public void SaveNotes(string character, string vsCharacter, string _notes)
        {
            using var db = this.GetConnection();
            string save = @"
                INSERT INTO Matchups (PlayerCharacter, OpponentCharacter, Notes)
                VALUES (@player, @opponent, @notes)
                ON CONFLICT(PlayerCharacter, OpponentCharacter) DO UPDATE SET Notes = @notes;";

            db.Execute(save, new { player = character, opponent = vsCharacter, notes = _notes });
        }

        public void CalculateWinrate(string character, string vsCharacter)
        {
            using var db = this.GetConnection();

            string getTotal = "SELECT GamesPlayed FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            int totalGames = db.ExecuteScalar<int>(getTotal, new { player = character, opponent = vsCharacter });
            if (totalGames == 0) { return; }

            string getWins = "SELECT Wins FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            int totalWins = db.ExecuteScalar<int>(getWins, new { player = character, opponent = vsCharacter });

            int winrate = (totalWins * 100) / totalGames;

            string winrateScript = "UPDATE Matchups SET Winrate = @winrate WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            db.Execute(winrateScript, new { player = character, opponent = vsCharacter, winrate = winrate });
        }

        public int GetWinrate(string character, string vsCharacter)
        {
            using var db = this.GetConnection();
            string getWinrate = "SELECT Winrate FROM Matchups WHERE PlayerCharacter = @player AND OpponentCharacter = @opponent";
            return db.ExecuteScalar<int>(getWinrate, new { player = character, opponent = vsCharacter });
        }

        public void SyncWinrates()
        {
            Console.WriteLine("Syncing all winrates...");
            using var db = this.GetConnection();

            string getMatchups = "SELECT PlayerCharacter, OpponentCharacter FROM Matchups";
            var matchups = db.Query<(string Player, string Opponent)>(getMatchups).ToList();

            try
            {
                foreach (var matchup in matchups)
                {
                    CalculateWinrate(matchup.Player, matchup.Opponent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during winrate sync: {ex.Message}");
            }
            Console.WriteLine("Done!");
        }

        public List<CharacterModel> FilterCharacters(string search)
        {
            Console.WriteLine($"Filtering by search: {search}");
            using var db = this.GetConnection();

            string filterCharacters = "SELECT CharacterName FROM Characters ORDER BY CASE WHEN CharacterName LIKE @searched THEN 0 ELSE 1 END";

            List<string> names = db.Query<string>(filterCharacters, new { searched = search}).ToList();
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

        public class CharacterModel
        {
            public string Name { get; set; }
            public string ImagePath { get; set; }
        }
    }
}