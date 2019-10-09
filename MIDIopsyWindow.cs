/* MIDIopsyWindow.cs - Implementation of MIDIopsyWindow class, which makes up this application's user interface.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Uri, Exception, Environment */
using System;

/* HorizontalAlignment, VerticalAlignment, GridLength, MessageBox,
 * MessageBoxButton, MessageBoxImage, RoutedEventArgs, RoutedEventHandler
 */
using System.Windows;

/* TextBox, ScrollBarVisibility, ScrollViewer, GridSplitter, Grid, ColumnDefinition, ScrollChangedEventArgs */
using System.Windows.Controls;

/* FontFamily, Brushes */
using System.Windows.Media;

/* BitmapFrame */
using System.Windows.Media.Imaging;


namespace JeffBourdier
{
    /// <summary>Represents a window that makes up the user interface for the MIDI Gutter application.</summary>
    public class MIDIopsyWindow : FileAppWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a MIDIopsyWindow object.</summary>
        public MIDIopsyWindow()
        {
            /* Initialize the hex text box. */
            this.HexTextBox = this.CreateTextBox();
            this.HexTextBox.Loaded += HexTextBox_Loaded;

            /* Initialize the grid splitter. */
            GridSplitter gridSplitter = new GridSplitter();
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Center;
            gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
            gridSplitter.Width = UI.UnitSpace;

            /* Initialize the comments text box. */
            this.CommentsTextBox = this.CreateTextBox();

            /* Build out the editing panel. */
            this.EditingPanel = new Grid();
            Grid grid = this.EditingPanel as Grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[0].Width = new GridLength(UI.ClientWidth / 2);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[1].Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(this.HexTextBox, 0);
            Grid.SetColumn(gridSplitter, 1);
            Grid.SetColumn(this.CommentsTextBox, 2);
            this.EditingPanel.Children.Add(this.HexTextBox);
            this.EditingPanel.Children.Add(gridSplitter);
            this.EditingPanel.Children.Add(this.CommentsTextBox);

            /* Initialize the window object. */
            Uri uri = AppHelper.CreateResourceUri(false, "MIDIopsy.ico");
            this.Icon = BitmapFrame.Create(uri);
            this.FileDialogFilter = Properties.Resources.MidiFiles;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static FontFamily FontFamily = new FontFamily("Courier New");

        private TextBox HexTextBox;
        private TextBox CommentsTextBox;
        private MidiFile MidiFile;
        private bool Highlighting;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>Prompts the user for information needed to create a new MIDI file.</summary>
        /// <returns>True if a new file is to be created; otherwise, false.</returns>
        protected override bool NewFile()
        {
            /* Prompt the user for header data.  If the user cancels, take no further action. */
            MidiHeaderDialog dialog = new MidiHeaderDialog();
            bool? result = dialog.ShowDialog(this);
            if (result == false) return false;

            /* The user did not cancel.  Reset the file path. */
            base.NewFile();

            /* Create a new file object using the header data supplied by the user. */
            MidiHeaderChunk chunk = dialog.CreateChunk();
            this.MidiFile = new MidiFile(chunk);

            /* Populate the UI from the file object. */
            this.Populate();
            return true;
        }

        /// <summary>Opens an existing MIDI file from disk.</summary>
        /// <param name="filePath">The path of the file being opened, or null/empty to prompt the user.</param>
        /// <returns>True if the file is to be opened; otherwise, false.</returns>
        protected override bool OpenFile(string filePath)
        {
            /* Make sure the user really wants to open a file. */
            if (!base.OpenFile(filePath)) return false;

            /* The user does want to open a file, so create a new file object using the file path. */
            try { this.MidiFile = new MidiFile(this.FilePath); }
            catch (Exception ex)
            {
                string s = Text.FormatErrorMessage(Common.Resources.FileNotOpened, ex);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            /* Populate the UI from the file object. */
            this.Populate();
            return true;
        }

        /// <summary>Saves the open MIDI file to disk.</summary>
        /// <param name="asNew">
        /// True if the file is to be saved "as new" (i.e., it has not
        /// already been saved at the current path); otherwise, false.
        /// </param>
        /// <returns>True if the file is to be saved; otherwise, false.</returns>
        protected override bool SaveFile(bool asNew)
        {
            /* First, find out if the user even wants to save the file. */
            if (!base.SaveFile(asNew)) return false;

            /* They do, so save it. */
            this.MidiFile.WriteToDisk(this.FilePath);
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        /* Give the hex text box initial focus. */
        private void HexTextBox_Loaded(object sender, RoutedEventArgs e) { this.HexTextBox.Focus(); }

        ///* When a text box gets focus, highlight accordingly. */
        //private void TextBox_GotFocus(object sender, RoutedEventArgs e) { this.Highlight(sender as TextBox); }

        /* When the caret (or current selection) of a text box changes, highlight accordingly. */
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e) { this.Highlight(sender as TextBox); }

        /* Synchronize the scroll position of the other text box. */
        private void TextBox_ScrollChanged(object sender, RoutedEventArgs e)
        { this.GetOtherTextBox(sender).ScrollToVerticalOffset((e as ScrollChangedEventArgs).VerticalOffset); }

        #endregion

        private TextBox CreateTextBox()
        {
            TextBox textBox = new TextBox();
            textBox.IsReadOnly = true;
            textBox.IsReadOnlyCaretVisible = true;
            textBox.IsInactiveSelectionHighlightEnabled = true;  /* requires .NET Framework 4.5 */
            textBox.FontFamily = MIDIopsyWindow.FontFamily;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.SelectionBrush = Brushes.DarkKhaki;
            //textBox.GotFocus += this.TextBox_GotFocus;
            textBox.SelectionChanged += this.TextBox_SelectionChanged;
            textBox.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(this.TextBox_ScrollChanged));
            return textBox;
        }

        /* Populate the text boxes with the appropriate flow content (document) of the open MIDI file. */
        private void Populate()
        {
            this.HexTextBox.Text = this.MidiFile.Hex;
            this.CommentsTextBox.Text = this.MidiFile.Comments;
        }

        /* Highlight a line in a text box. */
        private void Highlight(TextBox textBox)
        {
            /* Only one highlight operation at a time. */
            if (this.Highlighting) return;

            /* Determine which text box is being highlighted, and on which line the input caret is. */
            int n, j, i = 0;
            bool hex = (textBox == this.HexTextBox);
            if (textBox.CaretIndex == textBox.Text.Length)
            {
                n = this.MidiFile.LineCount;
                i = textBox.Text.Length;
                j = 0;
            }
            else
            {
                for (n = 1; (j = this.MidiFile.GetIndex(n, hex)) <= textBox.CaretIndex; ++n) i = j;
                j -= Environment.NewLine.Length + i;
            }

            /* If the text box (being highlighted) has the focus, synchronize the other text
             * box's input caret.  (This should trigger highlighting in the other text box.)
             */
            if (textBox.IsFocused) this.GetOtherTextBox(textBox).CaretIndex = this.MidiFile.GetIndex(n - 1, !hex);

            /* Highlight (select) the line where the input caret is. */
            this.Highlighting = true;
            textBox.Select(i, j);
            this.Highlighting = false;
        }

        private TextBox GetOtherTextBox(object textBox)
        { return (textBox == this.HexTextBox) ? this.CommentsTextBox : this.HexTextBox; }

        #endregion
    }
}
