using RTB_ToolTip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class Form1 : Form
    {
        private readonly List<Word> words;
        private readonly WordsDictionary wordsDictionary;
        private readonly eDictionary tooltipDictionary;

        public Form1()
        {
            InitializeComponent();

            tooltipDictionary = new eDictionary();
            var tooltip = new RichTextBoxToolTip
            {
                RichTextBox = textArea,
                Dictionary = tooltipDictionary,
                TitlePrefix = "Alternatywy dla wyrazu \"",
                TitleSuffix = "\": ",
                TitleBrush = Brushes.DarkBlue,
                TitleFont = new Font(textArea.SelectionFont, FontStyle.Bold),
                DescriptionFont = new Font(textArea.SelectionFont, FontStyle.Regular),
                DescriptionBrush = Brushes.Blue,
            };

            wordsDictionary = new WordsDictionary();
            words = new List<Word>();
        }

        private async void textArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && textArea.TextLength <= 1)
            {
                textArea.Clear();
                words.Clear();
                textArea.SelectionStart = 0;
                SetSelectionColor(Color.Black);
                SetSelectionUnderline(false);
            }

            if (e.KeyCode != Keys.Space && e.KeyCode != Keys.Enter)
            {
                return;
            }

            await HandleWords();
        }

        private async Task HandleWords()
        {
            var textAreaWords = GetWords(textArea.Text);
            if (!textAreaWords.Any())
            {
                return;
            }

            SynchronizeWords(textAreaWords);
            var task = ProcessWords();
            SetWordStyle();
            await task;
        }

        private static string[] GetWords(string text)
        {
            return Regex.Split(text, @"[(\d|\s|\W)]+")
                        .Where(w => !string.IsNullOrWhiteSpace(w))
                        .ToArray();
        }

        private void SynchronizeWords(string[] textAreaWords)
        {
            AppendNewWords(textAreaWords);
            RemoveOldWords(textAreaWords);
        }

        private void SetWordStyle()
        {
            var currentPosition = textArea.SelectionStart;
            var lastWord = GetWords(new string(textArea.Text.Take(currentPosition).ToArray())).LastOrDefault();
            if (string.IsNullOrWhiteSpace(lastWord))
            {
                return;
            }

            var wordObject = words.FirstOrDefault(w => w.Value.Equals(lastWord, StringComparison.OrdinalIgnoreCase));
            if (wordObject == null)
            {
                return;
            }

            if (!wordObject.IsCorrect)
            {
                UnderlineWord(lastWord);
            }
            else
            {
                RemoveUnderline(lastWord);
            }
        }

        private void AppendNewWords(IEnumerable<string> textAreaWords)
        {
            var newWords = textAreaWords.Where(
                taw => !words.Any(w => taw.Equals(w.Value, StringComparison.OrdinalIgnoreCase))
                    && !string.IsNullOrWhiteSpace(taw));

            foreach (var word in newWords)
            {
                var wordObject = new Word(word, wordsDictionary);
                words.Add(wordObject);
                if (!wordObject.IsCorrect)
                {
                    UnderlineWord(word);
                }
            }
        }

        private void RemoveOldWords(IEnumerable<string> textAreaWords)
        {
            words.RemoveAll(e => !textAreaWords.Contains(e.Value, StringComparer.OrdinalIgnoreCase));
        }

        private async Task ProcessWords()
        {
            while (true)
            {
                var firstNotProcessedWord = words.FirstOrDefault(w => !w.IsProcessed);
                if (firstNotProcessedWord == null)
                {
                    return;
                }

                await firstNotProcessedWord.Process(wordsDictionary);
                tooltipDictionary.Add(firstNotProcessedWord.Value, string.Join("\n", firstNotProcessedWord.SuggestedWords));
                tooltipDictionary.Add(firstNotProcessedWord.Value.Capitalize(), string.Join("\n", firstNotProcessedWord.SuggestedWords));
            }
        }

        private void UnderlineWord(string word)
        {
            var beginningIndex = textArea.SelectionStart;
            var startIndex = textArea.Text.LastIndexOf(word, StringComparison.OrdinalIgnoreCase);

            textArea.SelectionStart = startIndex;
            textArea.SelectionLength = word.Length;
            SetSelectionColor(Color.PaleVioletRed);
            SetSelectionUnderline(true);
            textArea.SelectionLength = 0;

            textArea.SelectionStart = beginningIndex;
            SetSelectionColor(Color.Black);
            SetSelectionUnderline(false);
        }

        private void RemoveUnderline(string word)
        {
            var beginningIndex = textArea.SelectionStart;
            var startIndex = textArea.Text.LastIndexOf(word, StringComparison.OrdinalIgnoreCase);

            textArea.SelectionStart = startIndex;
            textArea.SelectionLength = word.Length;
            SetSelectionColor(Color.Black);
            SetSelectionUnderline(false);
            textArea.SelectionLength = 0;

            textArea.SelectionStart = beginningIndex;
        }

        private void SetSelectionColor(Color color)
        {
            if (textArea.SelectionColor != color)
            {
                textArea.SelectionColor = color;
            }
        }

        private void SetSelectionUnderline(bool underline)
        {
            if (underline && !textArea.SelectionFont.Underline)
            {
                textArea.SelectionFont = new Font(textArea.SelectionFont, FontStyle.Underline);
                return;
            }

            if (!underline && textArea.SelectionFont.Underline)
            {
                textArea.SelectionFont = new Font(textArea.SelectionFont, FontStyle.Regular);
            }
        }

        private async void wczytajToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Text|*.txt" };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var reader = new StreamReader(dialog.OpenFile()))
            {
                var text = await reader.ReadToEndAsync();
                textArea.Text = text;
            }

            await HandleWords();
        }

        private async void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Text|*.txt" };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var writer = new StreamWriter(dialog.FileName))
            {
                await writer.WriteAsync(textArea.Text);
            }
        }
    }
}
