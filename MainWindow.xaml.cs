using SmashExpTracker;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SmashExpTracker.DatabaseService;

namespace SmashTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseService database = new DatabaseService();
        public MainWindow()
        {
            InitializeComponent();
            database.SyncWinrates();
            LoadCharacterDropdown();
        }

        private void LoadCharacterDropdown()
        {
            var characters = database.GetAllCharacters();

            CharacterDropdown.ItemsSource = characters;
            VSDropdown.ItemsSource = characters;
        }

        
        private void CharacterDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshMatchupDisplay();
        }

        private void VSDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshMatchupDisplay();
        }

        private void RefreshMatchupDisplay()
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;

            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;

            int wins = database.GetWins(character, vsCharacter);
            int losses = database.GetLosses(character, vsCharacter);
            int winrate = database.GetWinrate(character, vsCharacter);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";
            NotesTextBox.Text = database.GetNotes(character, vsCharacter);
            OutputText.Text = "";
        }

        private void SaveNotesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;
            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;
            string notes = NotesTextBox.Text;
            database.SaveNotes(character, vsCharacter, notes);

            ShowTempMessage($"Notes saved for: {character} VS. {vsCharacter}.");
        }   
        private void WinButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;
            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;

            database.AddWin(character, vsCharacter);
            database.CalculateWinrate(character, vsCharacter);

            int wins = database.GetWins(character, vsCharacter);
            int losses = database.GetLosses(character, vsCharacter);
            int winrate = database.GetWinrate(character, vsCharacter);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Recorded WIN: {character} VS. {vsCharacter}.");
        }

        private void RemoveWinButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;
            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;

            database.DelWin(character, vsCharacter);
            database.CalculateWinrate(character, vsCharacter);

            int wins = database.GetWins(character, vsCharacter);
            int losses = database.GetLosses(character, vsCharacter);
            int winrate = database.GetWinrate(character, vsCharacter);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Deleted WIN: {character} VS. {vsCharacter}.");
        }

        private void LoseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;
            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;

            database.AddLose(character, vsCharacter);
            database.CalculateWinrate(character, vsCharacter);

            int wins = database.GetWins(character, vsCharacter);
            int losses = database.GetLosses(character, vsCharacter);
            int winrate = database.GetWinrate(character, vsCharacter);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Recorded LOSS: {character} VS. {vsCharacter}.");
        }

        private void RemoveLoseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null || VSDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            CharacterModel vsChar = (CharacterModel)VSDropdown.SelectedItem;
            string character = selectedChar.Name;
            string vsCharacter = vsChar.Name;

            database.DelLose(character, vsCharacter);
            database.CalculateWinrate(character, vsCharacter);

            int wins = database.GetWins(character, vsCharacter);
            int losses = database.GetLosses(character, vsCharacter);
            int winrate = database.GetWinrate(character, vsCharacter);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Deleted LOSS: {character} VS. {vsCharacter}.");
        }

        private async void ShowTempMessage(string message)
        {
            OutputText.Text = message;

            await Task.Delay (5000);

            if (OutputText.Text == message)
            {
                OutputText.Text = "";
            }
        }

    }
}