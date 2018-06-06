using RTB_ToolTip;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            var isSpace = e.KeyCode == Keys.Space;
            SynchronizeWords(isSpace);
            if (!isSpace)
            {
                return;
            }

            var task = ProcessWords();
            UnderlineIncorrectWords();
            await task;
        }

        private void SynchronizeWords(bool isSpace)
        {
            var textAreaWords = Regex.Split(textArea.Text, @"[(\d|\s|\W)]+");
            if (textAreaWords.Any() && !isSpace)
            {
                Array.Resize(ref textAreaWords, textAreaWords.Length - 1);
            }

            AppendNewWords(textAreaWords);
            RemoveOldWords(textAreaWords);
        }

        private void UnderlineIncorrectWords()
        {
            foreach (var word in words.Where(w => !w.IsCorrect))
            {
                UnderlineWord(word.Value);
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
            }
        }

        private void UnderlineWord(string word)
        {
            var startIndices = textArea.Text.GetAllIndices(word);

            foreach (var startIndex in startIndices)
            {
                textArea.SelectionStart = startIndex;
                textArea.SelectionLength = word.Length;
                SetSelectionColor(Color.PaleVioletRed);
                SetSelectionUnderline(true);
                textArea.SelectionLength = 0;
            }

            textArea.SelectionStart = textArea.TextLength;
            SetSelectionColor(Color.Black);
            SetSelectionUnderline(false);
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
    }
}
