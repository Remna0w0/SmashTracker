using SmashExpTracker;
using System.ComponentModel;
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
        private ICollectionView _characterView;
        private ICollectionView _vsView;

        private List<CharacterModel> _allCharacters = new List<CharacterModel>();
        public MainWindow()
        {
            InitializeComponent();
            database.SyncWinrates();
            LoadCharacterDropdown();

        }

        private void LoadCharacterDropdown()
        {
            _allCharacters = database.GetAllCharacters();

            CharacterDropdown.ItemsSource = new List<CharacterModel>(_allCharacters); ;
            VSDropdown.ItemsSource = new List<CharacterModel>(_allCharacters); ;
        }

        private void  FilterAndSortDropdown(ComboBox target, string search)
        {
            if (_allCharacters == null || !_allCharacters.Any()) { return; }

            if (string.IsNullOrWhiteSpace(search))
            {
                target.ItemsSource = new List<CharacterModel>(_allCharacters);
                return;
            }

            var sortedList = _allCharacters
                .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c =>
                {
                    if (c.Name.Equals(search, StringComparison.OrdinalIgnoreCase)) return 0;
                    if (c.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase)) return 1;
                    return 2;
                })
                .ThenBy(c => c.Name)
                .ToList();

            var textBox = target.Template.FindName("PART_EditableTextBox", target) as TextBox;
            int caretIndex = textBox?.CaretIndex ?? 0;

            target.ItemsSource = sortedList;

            target.IsDropDownOpen = true;

            if(textBox != null)
            {
                textBox.CaretIndex = caretIndex;
            }
        }



        private void CharacterDropdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CharacterDropdown.SelectedIndex != -1 && !CharacterDropdown.IsDropDownOpen)
            {
                var textBox = CharacterDropdown.Template.FindName("PART_EditableTextBox", CharacterDropdown) as TextBox;
                if (textBox != null)
                {
                    string currentText = CharacterDropdown.Text;
                    int caretIndex = textBox.CaretIndex;


                    CharacterDropdown.SelectedIndex = -1;
                    CharacterDropdown.Text = currentText;


                    textBox.CaretIndex = caretIndex;
                }
            }

            if (CharacterDropdown.IsDropDownOpen || CharacterDropdown.SelectedIndex == -1)
            {
                FilterAndSortDropdown(CharacterDropdown, CharacterDropdown.Text);
            }
        }

        private void VSDropdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VSDropdown.SelectedIndex != -1 && !VSDropdown.IsDropDownOpen)
            {

                var textBox = VSDropdown.Template.FindName("PART_EditableTextBox", CharacterDropdown) as TextBox;
                if (textBox != null)
                {
                    string currentText = VSDropdown.Text;
                    int caretIndex = textBox.CaretIndex;


                    VSDropdown.SelectedIndex = -1;
                    VSDropdown.Text = currentText;


                    textBox.CaretIndex = caretIndex;
                }
                ;
            }

            if (VSDropdown.IsDropDownOpen || VSDropdown.SelectedIndex == -1)
            {
                FilterAndSortDropdown(VSDropdown, VSDropdown.Text);
            }
        }

        private void CharacterDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshMatchupDisplay();
            RefreshOverallDisplay();
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

        private void RefreshOverallDisplay()
        {
            if (CharacterDropdown.SelectedItem == null) return;

            CharacterModel selectedChar = ( CharacterModel)CharacterDropdown.SelectedItem;

            string character = selectedChar.Name;

            int totalWins = database.GetTotalWins(character);
            int totalLosses = database.GetTotalLosses(character);
            int totalWinrate = database.CalculateTotalWinrate(character);

            OverallDisplay.Text = $"Total Wins: {totalWins} | Total Losses: {totalLosses} | Total Winrate: {totalWinrate}%";

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

            RefreshOverallDisplay();

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

            RefreshOverallDisplay();

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

            RefreshOverallDisplay();

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

            RefreshOverallDisplay();

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