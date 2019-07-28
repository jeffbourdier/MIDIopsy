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


/* Uri, Exception, Environment, Math */
using System;

/* HorizontalAlignment, VerticalAlignment, GridLength, MessageBox,
 * MessageBoxButton, MessageBoxImage, RoutedEventArgs, RoutedEventHandler
 */
using System.Windows;

/* RichTextBox, ScrollBarVisibility, ScrollViewer, GridSplitter, Grid, ColumnDefinition, ScrollChangedEventArgs */
using System.Windows.Controls;

/* Run, TextPointer, LogicalDirection, Paragraph */
using System.Windows.Documents;

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
            FontFamily fontFamily = new FontFamily("Courier New");

            /* Initialize the hex text box. */
            this.HexTextBox = new RichTextBox();
            this.HexTextBox.IsReadOnly = true;
            this.HexTextBox.IsReadOnlyCaretVisible = true;
            this.HexTextBox.FontFamily = fontFamily;
            this.HexTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.HexTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.HexTextBox.Loaded += HexTextBox_Loaded;
            this.HexTextBox.GotFocus += HexTextBox_GotFocus;
            this.HexTextBox.SelectionChanged += HexTextBox_SelectionChanged;
            this.HexTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(HexTextBox_ScrollChanged));

            /* Initialize the grid splitter. */
            GridSplitter gridSplitter = new GridSplitter();
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Center;
            gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
            gridSplitter.Width = UI.UnitSpace;

            /* Initialize the comments text box. */
            this.CommentsTextBox = new RichTextBox();
            this.CommentsTextBox.IsReadOnly = true;
            this.CommentsTextBox.IsReadOnlyCaretVisible = true;
            this.CommentsTextBox.FontFamily = fontFamily;
            this.CommentsTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.CommentsTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.CommentsTextBox.GotFocus += CommentsTextBox_GotFocus;
            this.CommentsTextBox.SelectionChanged += CommentsTextBox_SelectionChanged;
            this.CommentsTextBox.AddHandler(ScrollViewer.ScrollChangedEvent,
                new RoutedEventHandler(CommentsTextBox_ScrollChanged));

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

        private RichTextBox HexTextBox;
        private RichTextBox CommentsTextBox;
        private MidiFile MidiFile;
        private Run HexRun;
        private Run CommentsRun;

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

        /* When the hex text box gets focus, highlight accordingly. */
        private void HexTextBox_GotFocus(object sender, RoutedEventArgs e)
        { MIDIopsyWindow.Highlight(ref this.HexRun, this.HexTextBox, ref this.CommentsRun, this.CommentsTextBox); }

        /* When the caret (or current selection) of the hex text box changes position, highlight accordingly. */
        private void HexTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        { MIDIopsyWindow.Highlight(ref this.HexRun, this.HexTextBox, ref this.CommentsRun, this.CommentsTextBox); }

        /* Synchronize the scroll position of the comments text box with that of the hex text box. */
        private void HexTextBox_ScrollChanged(object sender, RoutedEventArgs e)
        { this.CommentsTextBox.ScrollToVerticalOffset((e as ScrollChangedEventArgs).VerticalOffset); }

        /* When the comments text box gets focus, highlight accordingly. */
        private void CommentsTextBox_GotFocus(object sender, RoutedEventArgs e)
        { MIDIopsyWindow.Highlight(ref this.CommentsRun, this.CommentsTextBox, ref this.HexRun, this.HexTextBox); }

        /* When the caret (or current selection) of the comments text box changes position, highlight accordingly. */
        private void CommentsTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        { MIDIopsyWindow.Highlight(ref this.CommentsRun, this.CommentsTextBox, ref this.HexRun, this.HexTextBox); }

        /* Synchronize the scroll position of the hex text box with that of the comments text box. */
        private void CommentsTextBox_ScrollChanged(object sender, RoutedEventArgs e)
        { this.HexTextBox.ScrollToVerticalOffset((e as ScrollChangedEventArgs).VerticalOffset); }

        #endregion

        /* Populate the text boxes with the appropriate flow content (document) of the open MIDI file. */
        private void Populate()
        {
            this.HexTextBox.Document = this.MidiFile.HexDocument;
            this.CommentsTextBox.Document = this.MidiFile.CommentsDocument;
        }

        /* Highlight a run in a (rich) text box. */
        private static void Highlight(ref Run run, RichTextBox textBox, ref Run otherRun, RichTextBox otherTextBox)
        {
            /* Unhighlight the currently highlighted run (if any). */
            if (run != null) run.Background = null;

            /* Get a text pointer to the beginning of the line where the input caret is. */
            TextPointer pointer = textBox.CaretPosition.GetLineStartPosition(0);

            /* If the text box has the focus... */
            if (textBox.IsFocused)
            {
                /* Synchronize the other text box's input caret.  (This should trigger highlighting in the other text box.) */
                MIDIopsyWindow.SetCaretPosition(pointer, otherTextBox);

                /* If there is a selection in the text box, don't try to highlight anything. */
                if (!textBox.Selection.IsEmpty) return;
            }
            /* Otherwise, if there is a selection in the other text box, don't try to highlight anything. */
            else if (!otherTextBox.Selection.IsEmpty) return;

            /* Highlight the run where the input caret is. */
            run = pointer.GetAdjacentElement(LogicalDirection.Forward) as Run;
            run.Background = Brushes.Khaki;
        }

        /* Set the position of the input caret in a (rich) text box based on the line # of a text pointer. */
        private static void SetCaretPosition(TextPointer pointer, RichTextBox textBox)
        {
            /* Get the line # of the text pointer. */
            int count;
            pointer.GetLineStartPosition(int.MinValue, out count);
            count = Math.Abs(count);

            /* Set the position of the text box's input caret to the beginning of the corresponding line. */
            Paragraph paragraph = textBox.Document.ContentStart.GetAdjacentElement(LogicalDirection.Forward) as Paragraph;
            textBox.CaretPosition = paragraph.ContentStart.GetLineStartPosition(count);
        }

        #endregion
    }
}
