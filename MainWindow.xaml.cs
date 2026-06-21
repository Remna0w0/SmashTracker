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
        }

        private void CharacterDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;

            string character = selectedChar.Name;

            int wins = database.GetWins(character);
            int losses = database.GetLosses(character);
            int winrate = database.GetWinrate(character);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            NotesTextBox.Text = database.GetNotes(character);

            OutputText.Text = "";
        }

        private void SaveNotesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;
            string character = selectedChar.Name;
            string notes = NotesTextBox.Text;
            database.SaveNotes(character, notes);

            ShowTempMessage($"Notes saved: {character}");
        }   
        private void WinButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;

            string character = selectedChar.Name;

            database.AddWin(character);
            database.CalculateWinrate(character);

            int wins = database.GetWins(character);
            int losses = database.GetLosses(character);
            int winrate = database.GetWinrate(character);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Recorded WIN: {character}.");
        }

        private void LoseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = (CharacterModel)CharacterDropdown.SelectedItem;

            string character = selectedChar.Name;

            database.AddLose(character);
            database.CalculateWinrate(character);

            int wins = database.GetWins(character);
            int losses = database.GetLosses(character);
            int winrate = database.GetWinrate(character);

            StatsDisplay.Text = $"Wins: {wins} | Losses: {losses} | Winrate: {winrate}%";

            ShowTempMessage($"Recorded LOSS: {character}.");
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