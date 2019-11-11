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


/* Uri, Exception, Environment, EventArgs */
using System;

/* CancelEventArgs */
using System.ComponentModel;

/* EventWaitHandle, EventResetMode, ThreadPool, Thread */
using System.Threading;

/* Thickness, HorizontalAlignment, VerticalAlignment, GridLength, MessageBox,
 * MessageBoxButton, MessageBoxImage, Visibility, RoutedEventArgs, RoutedEventHandler
 */
using System.Windows;

/* StackPanel, Orientation, Canvas, TextBox, GridSplitter, Grid, ColumnDefinition,
 * DockPanel, Dock, ScrollChangedEventArgs, ScrollBarVisibility, ScrollViewer, Label
 */
using System.Windows.Controls;

/* KeyGesture, MediaCommands, CommandBinding, ExecutedRoutedEventArgs, CanExecuteRoutedEventArgs */
using System.Windows.Input;

/* FontFamily, Brushes, MediaPlayer */
using System.Windows.Media;

/* BitmapFrame */
using System.Windows.Media.Imaging;


namespace JeffBourdier
{
    public delegate void PlaybackMethod();

    /// <summary>Represents a window that makes up the user interface for the MIDIopsy application.</summary>
    public class MIDIopsyWindow : FileAppWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a MIDIopsyWindow object.</summary>
        public MIDIopsyWindow()
        {
            KeyGesture gesture;
            CommandBinding binding;
            Grid grid;
            StandardLabel label;

            /* The duration of a MIDI file cannot be determined until it has been
             * opened for media playback (i.e., the MediaOpened event has been raised).
             */
            this.Player.MediaOpened += this.Player_MediaOpened;

            /* Bind the Play/Stop command (for MIDI file playback). */
            gesture = new KeyGesture(Key.F5);
            MediaCommands.Play.InputGestures.Add(gesture);
            binding = new CommandBinding(MediaCommands.Play, this.PlayStopExecuted, this.PlaybackCommandCanExecute);
            this.CommandBindings.Add(binding);
            this.PlayStopButton = new CommandButton(MediaCommands.Play);

            /* Initialize the Starting Position control. */
            this.StartingPositionControl = new PositionControl(Properties.Resources.StartingPosition, false);
            this.StartingPositionControl.Margin = new Thickness(UI.UnitSpace);

            /* Bind the Select command (to set starting position). */
            gesture = new KeyGesture(Key.F6);
            MediaCommands.Select.InputGestures.Add(gesture);
            binding = new CommandBinding(MediaCommands.Select, this.SelectExecuted, this.PlaybackCommandCanExecute);
            this.CommandBindings.Add(binding);
            this.SelectButton = new CommandButton(MediaCommands.Select);

            /* Initialize the grid for the Position & Duration controls (to be added shortly). */
            grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.Margin = new Thickness(UI.UnitSpace);

            /* Add the Position controls. */
            label = new StandardLabel(Properties.Resources.Position, false);
            grid.Children.Add(label);
            this.PositionTextBox = this.CreateTextBox(label);
            grid.Children.Add(this.PositionTextBox);

            /* Add the Duration controls. */
            label = new StandardLabel(Properties.Resources.Duration, false);
            Grid.SetRow(label, 1);
            grid.Children.Add(label);
            this.DurationTextBox = this.CreateTextBox(label);
            Grid.SetRow(this.DurationTextBox, 1);
            grid.Children.Add(this.DurationTextBox);

            /* Build the panel (for all the controls above). */
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(this.PlayStopButton);
            panel.Children.Add(this.StartingPositionControl);
            panel.Children.Add(this.SelectButton);
            panel.Children.Add(grid);
            panel.Children.Add(new Canvas());

            /* Initialize the text boxes. */
            this.HexTextBox = this.CreateTextBox(null);
            this.HexTextBox.Loaded += this.HexTextBox_Loaded;
            this.CommentsTextBox = this.CreateTextBox(null);

            /* Initialize the grid splitter. */
            GridSplitter splitter = new GridSplitter();
            splitter.HorizontalAlignment = HorizontalAlignment.Center;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.Width = UI.UnitSpace;

            /* Build the grid for the text boxes. */
            grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[0].Width = new GridLength(UI.ClientWidth / 2);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[1].Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(this.HexTextBox, 0);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(this.CommentsTextBox, 2);
            grid.Children.Add(this.HexTextBox);
            grid.Children.Add(splitter);
            grid.Children.Add(this.CommentsTextBox);

            /* Build out the editing panel. */
            DockPanel.SetDock(panel, Dock.Top);
            this.EditingPanel = new DockPanel();
            this.EditingPanel.Children.Add(panel);
            this.EditingPanel.Children.Add(grid);

            /* Initialize the window object. */
            Uri uri = AppHelper.CreateResourceUri(false, "MIDIopsy.ico");
            this.Icon = BitmapFrame.Create(uri);
            this.FileDialogFilter = Properties.Resources.MidiFiles;
            this.Closing += this.MIDIopsyWindow_Closing;
            this.MinWidth = 640;
            this.MinHeight = 240;

            /* Start another thread (initally blocked) to update the Position control during MIDI file playback. */
            this.EventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            ThreadPool.QueueUserWorkItem(this.UpdatePosition);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static FontFamily Font = new FontFamily("Courier New");

        private MediaPlayer Player = new MediaPlayer();
        private CommandButton PlayStopButton;
        private PositionControl StartingPositionControl;
        private CommandButton SelectButton;
        private TextBox PositionTextBox;
        private TextBox DurationTextBox;
        private TextBox HexTextBox;
        private TextBox CommentsTextBox;
        private EventWaitHandle EventHandle;
        private MidiFile MidiFile;
        private bool _IsPlaybackStopped = true;
        private bool Highlighting = false;

        #endregion

        /**************
         * Properties *
         **************/

        #region Private Properties

        /* Indicate whether or not MIDI file playback is stopped. */
        private bool IsPlaybackStopped { get { return this._IsPlaybackStopped; } }

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

            /* Load the file object into the UI. */
            this.LoadFile();
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
                this.FilePath = null;
                return false;
            }

            /* Load the file object into the UI. */
            this.LoadFile();
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

        /// <summary>Closes the open MIDI file.</summary>
        /// <returns>True if the file is to be closed; otherwise, false.</returns>
        protected override bool CloseFile()
        {
            /* Confirm that the user is really done. */
            if (!base.CloseFile()) return false;

            /* They're really done, so unload the file. */
            this.StopPlayback();
            this.Player.Close();
            this.MidiFile = null;
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        /* Once a MIDI file has been opened for media playback, set its initial position and determine its duration. */
        private void Player_MediaOpened(object sender, EventArgs e)
        {
            this.SetPosition();
            this.DurationTextBox.Text = this.Player.NaturalDuration.TimeSpan.ToString();
        }

        /* Toggle (play/stop) MIDI file playback. */
        private void PlayStopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* If we're stopping, we can be done right here. */
            if (this.StopPlayback()) return;

            /* We're playing, so validate the starting position. */
            if (this.StartingPositionControl.Position > this.Player.NaturalDuration.TimeSpan)
            {
                string s = Text.ParseLabel(Properties.Resources.StartingPosition);
                s = string.Format(Common.Resources.ValueRangeFormat, s, "00:00:00", this.Player.NaturalDuration.TimeSpan);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            /* Start playing at the starting position. */
            this.Player.Position = this.StartingPositionControl.Position;
            this.PlayStopButton.Text = MediaCommands.Stop.Text;
            this.Player.Play();
            this._IsPlaybackStopped = false;

            /* Signal the other thread so that it can proceed with updating the Position control. */
            this.EventHandle.Set();
        }

        /* Set the starting position to the current position. */
        private void SelectExecuted(object sender, ExecutedRoutedEventArgs e)
        { this.StartingPositionControl.Position = TimeSpan.Parse(this.PositionTextBox.Text); }

        /* A playback command can execute as long as there is a file open. */
        private void PlaybackCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.None); }

        /* Give the hex text box initial focus. */
        private void HexTextBox_Loaded(object sender, RoutedEventArgs e) { this.HexTextBox.Focus(); }

        /* When the caret (or current selection) of a text box changes, highlight accordingly. */
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e) { this.Highlight(sender as TextBox); }

        /* Synchronize the scroll position of the other text box. */
        private void TextBox_ScrollChanged(object sender, RoutedEventArgs e)
        { this.GetOtherTextBox(sender).ScrollToVerticalOffset((e as ScrollChangedEventArgs).VerticalOffset); }

        /* Stop MIDI file playback and then signal the other thread so that it will exit. */
        private void MIDIopsyWindow_Closing(object sender, CancelEventArgs e)
        {
            this.StopPlayback();
            this.EventHandle.Set();
        }

        #endregion

        private TextBox CreateTextBox(Label label)
        {
            TextBox textBox = new TextBox();
            textBox.IsReadOnly = true;

            if (label != null)
            {
                label.Target = textBox;
                textBox.GotFocus += UI.TextBox_GotFocus;
                textBox.Margin = new Thickness(0, UI.HalfSpace, 0, UI.HalfSpace);
                Grid.SetColumn(textBox, 1);
                return textBox;
            }

            textBox.IsReadOnlyCaretVisible = true;
            textBox.IsInactiveSelectionHighlightEnabled = true;  /* requires .NET Framework 4.5 */
            textBox.FontFamily = MIDIopsyWindow.Font;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.SelectionBrush = Brushes.DarkKhaki;
            textBox.SelectionChanged += this.TextBox_SelectionChanged;
            textBox.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(this.TextBox_ScrollChanged));
            return textBox;
        }

        /* This method runs in its own thread to update the Position control during MIDI file playback. */
        private void UpdatePosition(object state)
        {
            while (true)
            {
                /* Block until signaled. */
                this.EventHandle.WaitOne();

                /* If MIDI file playback is stopped, then exit. */
                if (this.IsPlaybackStopped) return;

                /* Because the media player and the Position control are dispatcher objects owned by the main UI thread,
                 * this thread cannot access them, so the position must be set on the main UI thread via the dispatcher.
                 */
                this.Dispatcher.Invoke(this.SetPosition);

                /* Wait a second, then repeat. */
                Thread.Sleep(1000);
            }
        }

        /* Perform all actions necessary to load the MIDI file object into the UI. */
        private void LoadFile()
        {
            /* Open the MIDI file for media playback.
             * (Note: duration cannot be determined until the MediaOpened event has been raised.)
             */
            this.Player.Open(new Uri(this.FilePath));
            this.StartingPositionControl.Position = new TimeSpan();

            /* Populate the text boxes with the appropriate contents from the MIDI file. */
            this.HexTextBox.Text = this.MidiFile.Hex;
            this.CommentsTextBox.Text = this.MidiFile.Comments;
        }

        /* If applicable, stop MIDI file playback. */
        private bool StopPlayback()
        {
            /* If MIDI file playback is already stopped, then not applicable. */
            if (this.IsPlaybackStopped) return false;

            /* Block the other thread so that it does not continue to update the Position control after playback is stopped. */
            this.EventHandle.Reset();

            /* The Position control should be accurate (or pretty close) since the other thread has been updating it, but it
             * could be a little off due to minor discrepancies in thread synchronization, so make sure it is accurate now.
             */
            this.SetPosition();

            /* The MIDI file is playing, so stop it. */
            this.Player.Stop();
            this.PlayStopButton.Text = MediaCommands.Play.Text;
            this._IsPlaybackStopped = true;
            return true;
        }

        /* Set the Position control to the current position.  (When called by the other thread, this method must be run via
         * the dispatcher, since the media player and the Position control are dispatcher objects owned by the main UI thread.)
         */
        private void SetPosition() { this.PositionTextBox.Text = this.Player.Position.ToString(@"hh\:mm\:ss"); }

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
