/* MIDIopsyWindow.cs - Implementation of MIDIopsyWindow class, which makes up this application's user interface.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Uri, Exception, Environment, EventArgs, Action, StringSplitOptions, ApplicationException */
using System;

/* List */
using System.Collections.Generic;

/* CancelEventArgs */
using System.ComponentModel;

/* NumberStyles */
using System.Globalization;

/* EventWaitHandle, EventResetMode, ThreadPool, Thread */
using System.Threading;

/* Thickness, GridLength, HorizontalAlignment, VerticalAlignment, TextAlignment, MessageBox,
 * MessageBoxButton, MessageBoxImage, MessageBoxResult, RoutedEventArgs, RoutedEventHandler
 */
using System.Windows;

/* StackPanel, Orientation, Canvas, TextBox, GridSplitter, Grid, ColumnDefinition, DockPanel,
 * Dock, TextChangedEventArgs, ScrollChangedEventArgs, ScrollBarVisibility, ScrollViewer, Label
 */
using System.Windows.Controls;

/* KeyGesture, MediaCommands, RoutedUICommand, CommandBinding,
 * CommandManager, ExecutedRoutedEventArgs, CanExecuteRoutedEventArgs
 */
using System.Windows.Input;

/* FontFamily, Brush, Brushes, MediaPlayer */
using System.Windows.Media;

/* BitmapFrame */
using System.Windows.Media.Imaging;


namespace JeffBourdier
{
    /// <summary>Specifies enumerated constants to define the state of the UI.</summary>
    internal enum UIState { View, Play, Edit }

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
            RoutedUICommand command;
            CommandBinding binding;
            StandardLabel label;

            /* The duration of a MIDI file cannot be determined until it has been
             * opened for media playback (i.e., the MediaOpened event has been raised).
             */
            this.Player.MediaOpened += this.Player_MediaOpened;
            this.Player.MediaEnded += this.Player_MediaEnded;

            /* Bind the Play/Stop command (for MIDI file playback). */
            gesture = new KeyGesture(Key.F5);
            MediaCommands.Play.InputGestures.Add(gesture);
            binding = new CommandBinding(MediaCommands.Play, this.PlayStopExecuted, this.PlaybackCommandCanExecute);
            this.CommandBindings.Add(binding);
            this.PlayStopButton = new CommandButton(MediaCommands.Play);

            /* Initialize the Starting Position control. */
            this.StartingPositionControl = new PositionControl(Properties.Resources.StartingPosition, false);
            this.StartingPositionControl.Margin = new Thickness(UI.UnitSpace);

            /* Create and bind the Reset command (to reset/zero starting position). */
            command = new RoutedUICommand();
            gesture = new KeyGesture(Key.F6);
            command.InputGestures.Add(gesture);
            command.Text = Properties.Resources.Reset;
            binding = new CommandBinding(command, this.ResetExecuted, this.PlaybackCommandCanExecute);
            this.CommandBindings.Add(binding);
            CommandButton resetButton = new CommandButton(command);

            /* Create and bind the Sync command (to synchronize starting position). */
            command = new RoutedUICommand();
            gesture = new KeyGesture(Key.F7);
            command.InputGestures.Add(gesture);
            command.Text = Properties.Resources.Sync;
            binding = new CommandBinding(command, this.SyncExecuted, this.PlaybackCommandCanExecute);
            this.CommandBindings.Add(binding);
            CommandButton syncButton = new CommandButton(command);

            /* Initialize the grid for the Position & Duration controls (to be added shortly). */
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[1].Width = new GridLength(16 * UI.UnitSpace);
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.Margin = new Thickness(0, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);

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

            /* Build the panel for the playback controls. */
            this.PlaybackPanel = new StackPanel();
            this.PlaybackPanel.Orientation = Orientation.Horizontal;
            this.PlaybackPanel.Children.Add(this.PlayStopButton);
            this.PlaybackPanel.Children.Add(this.StartingPositionControl);
            this.PlaybackPanel.Children.Add(resetButton);
            this.PlaybackPanel.Children.Add(syncButton);
            this.PlaybackPanel.Children.Add(grid);

            /* Create and bind the command to toggle (start/stop) editing. */
            command = new RoutedUICommand();
            gesture = new KeyGesture(Key.F2);
            command.InputGestures.Add(gesture);
            command.Text = Properties.Resources.StartEditing;
            binding = new CommandBinding(command, this.EditExecuted, this.EditCanExecute);
            this.CommandBindings.Add(binding);
            this.EditButton = new CommandButton(command);

            /* Build the dock panel (for all the controls above). */
            DockPanel.SetDock(this.EditButton, Dock.Left);
            DockPanel.SetDock(this.PlaybackPanel, Dock.Right);
            DockPanel panel = new DockPanel();
            panel.Children.Add(this.EditButton);
            panel.Children.Add(this.PlaybackPanel);
            panel.Children.Add(new Canvas());

            /* Initialize the text boxes. */
            this.HexTextBox = this.CreateTextBox(null);
            this.HexTextBox.AcceptsReturn = true;
            this.HexTextBox.Loaded += this.HexTextBox_Loaded;
            this.HexTextBox.TextChanged += HexTextBox_TextChanged;
            this.CommentsTextBox = this.CreateTextBox(null);

            /* Initialize the grid splitter. */
            GridSplitter splitter = new GridSplitter();
            splitter.HorizontalAlignment = HorizontalAlignment.Center;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.Width = UI.UnitSpace;

            /* Build the grid for the text boxes. */
            this.TextGrid = new Grid();
            this.TextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.TextGrid.ColumnDefinitions[0].Width = new GridLength(UI.ClientWidth / 2);
            this.TextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.TextGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            this.TextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(this.HexTextBox, 0);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(this.CommentsTextBox, 2);
            this.TextGrid.Children.Add(this.HexTextBox);
            this.TextGrid.Children.Add(splitter);
            this.TextGrid.Children.Add(this.CommentsTextBox);

            /* Build out the editing panel. */
            DockPanel.SetDock(panel, Dock.Top);
            this.EditingPanel = new DockPanel();
            this.EditingPanel.Children.Add(panel);
            this.EditingPanel.Children.Add(this.TextGrid);

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
        private static Brush HighlightingBrush = Brushes.DarkKhaki;
        private static Brush SelectionBrush = Brushes.DodgerBlue;
        private static int DefaultTimeout = 250;
        private static string[] Separators = new string[] { " ", Environment.NewLine };

        private MediaPlayer Player = new MediaPlayer();
        private CommandButton PlayStopButton;
        private PositionControl StartingPositionControl;
        private TextBox PositionTextBox;
        private TextBox DurationTextBox;
        private StackPanel PlaybackPanel;
        private CommandButton EditButton;
        private TextBox HexTextBox;
        private TextBox CommentsTextBox;
        private Grid TextGrid;
        private EventWaitHandle EventHandle;
        private MidiFile MidiFile;
        private UIState UIState = UIState.View;
        private bool AutoPlay = false;
        private bool NoHighlighting = false;
        private string LastValidHex;

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
            MidiHeaderData data = dialog.CreateData();
            this.MidiFile = new MidiFile(data);

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
                Logger.WriteMessage(Common.Resources.FileNotOpened);
                Logger.WriteException(ex);

                string s = Text.FormatErrorMessage(Common.Resources.FileNotOpened, ex);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                this.FilePath = null;
                return false;
            }

            /* Load the file object into the UI. */
            this.LoadFile();
            this.ShowFileErrors();
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
            MessageBoxResult result;

            switch (this.UIState)
            {
                /* If the file is playing, warn the user that this will stop it. */
                case UIState.Play:
                    result = MessageBox.Show(this, Properties.Resources.WillStopPlayback,
                        Meta.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return false;
                    break;

                /* If editing, validate the changes. */
                case UIState.Edit:
                    bool? valid = this.ApplyHex();

                    /* If the changes are invalid and the user did not revert (so the data remains invalid), don't save. */
                    if (valid == false) return false;

                    /* If the user reverted the invalid changes (so the data is now valid), ask if they still want to save. */
                    if (valid == null)
                    {
                        result = MessageBox.Show(this, Properties.Resources.SaveReverted, Meta.Name, MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.No) return false;
                    }
                    break;
            }

            /* Confirm that the user really wants to save, and establish the file path. */
            if (!base.SaveFile(asNew)) return false;

            /* Save the file and reload it for media playback.  (The sleep is to help ensure
             * that the media player has released the file before attempting to write to it.)
             */
            this.StopPlayback();
            this.Player.Close();
            Thread.Sleep(MIDIopsyWindow.DefaultTimeout);
            try { this.MidiFile.WriteToDisk(this.FilePath); }
            catch (Exception ex)
            {
                Logger.WriteMessage(Common.Resources.FileNotSaved);
                Logger.WriteException(ex);

                string s = Text.FormatErrorMessage(Common.Resources.FileNotSaved, ex);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            this.InitializePlayback(false);
            return true;
        }

        /// <summary>Closes the open MIDI file.</summary>
        /// <returns>True if the file is to be closed; otherwise, false.</returns>
        protected override bool CloseFile()
        {
            /* Confirm that the user is really done. */
            if (!base.CloseFile()) return false;

            /* They're really done, so unload the file. */
            if (!this.StopEditing(false)) this.HexTextBox.IsUndoEnabled = false;
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
            this.DurationTextBox.Text = (this.Player.NaturalDuration.HasTimeSpan ?
                this.Player.NaturalDuration.TimeSpan : TimeSpan.Zero).ToString();

            /* If we're supposed to (e.g., the user saved and reloaded when trying to play), play the file. */
            if (!this.AutoPlay) return;
            this.PlayMedia();
            this.AutoPlay = false;
        }

        /* If the MIDI file finishes playing on its own (without being
         * stopped by the user), playback must still be stopped explicitly.
         */
        private void Player_MediaEnded(object sender, EventArgs e)
        {
            this.StopPlayback();

            /* For whatever reason, the Edit command's CanExecute event handler is not
             * automatically called here, so trigger it by raising the CanExecuteChanged event.
             */
            CommandManager.InvalidateRequerySuggested();
        }

        /* Toggle (play/stop) MIDI file playback. */
        private void PlayStopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* If we're stopping, we can be done right here. */
            if (this.StopPlayback()) return;

            /* If the file is edited, prompt the user to save and reload. */
            if (this.FileState == FileState.Edited)
            {
                MessageBoxResult result = MessageBox.Show(this, Properties.Resources.ReloadForPlayback,
                    Meta.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    /* The user chose to save and reload.  Set the flag to play the file as
                     * soon as it's reloaded.  Executing the Save command (if the file is saved
                     * successfully) will trigger the MediaOpened event, which will play the file.
                     */
                    this.AutoPlay = true;
                    ApplicationCommands.Save.Execute(null, null);

                    /* If the file was not saved, don't play. */
                    if (this.FileState == FileState.Edited) this.AutoPlay = false;

                    /* Regardless, we're done here. */
                    return;
                }
            }

            /* Either the file is not edited, or the user chose not to save and reload.  Either way, play the file. */
            this.PlayMedia();
        }

        /* Reset the starting position to zero. */
        private void ResetExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.StartingPositionControl.Position = TimeSpan.Zero;
            ThreadPool.QueueUserWorkItem(this.FlashStartingPosition, Brushes.Orange);
        }

        /* Set the starting position equal to the current position. */
        private void SyncExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.StartingPositionControl.Position = TimeSpan.Parse(this.PositionTextBox.Text);
            ThreadPool.QueueUserWorkItem(this.FlashStartingPosition, Brushes.Yellow);
        }

        /* A playback command can execute as long as there is a file open and the panel is enabled. */
        private void PlaybackCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState != FileState.None) && this.PlaybackPanel.IsEnabled; }

        /* Toggle (start/stop) MIDI file editing. */
        private void EditExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* If we're stopping, we can be done right here. */
            if (this.StopEditing(true)) return;

            /* Warn the user about the risk of editing the file and confirm that they really want to. */
            MessageBoxResult result = MessageBox.Show(this, Properties.Resources.EditWarning,
                Meta.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            /* They really do, so start editing. */

            /* Save the last valid version of the hex data to restore in
             * case the user's changes are invalid and they choose to revert.
             */
            this.LastValidHex = this.HexTextBox.Text;

            /* Disallow playback while editing. */
            this.UIState = UIState.Edit;
            this.PlaybackPanel.IsEnabled = false;

            /* Instead of highlighting, allow normal selection. */
            this.NoHighlighting = true;
            this.CommentsTextBox.SelectionLength = 0;
            this.HexTextBox.SelectionLength = 0;
            this.HexTextBox.SelectionBrush = MIDIopsyWindow.SelectionBrush;

            /* Don't show comments while editing. */
            this.EditingPanel.Children.Remove(this.TextGrid);
            this.TextGrid.Children.Remove(this.HexTextBox);
            this.EditingPanel.Children.Add(this.HexTextBox);

            /* Make the hex editable. */
            this.HexTextBox.IsUndoEnabled = true;
            this.HexTextBox.IsReadOnly = false;

            /* Finally, change the text of the edit button. */
            this.EditButton.Text = Properties.Resources.StopEditing;

            /* To be safe/lazy, go ahead and mark the file as edited now (even
             * though it's not really edited yet--but we assume it soon will be).
             */
            this.FileState = FileState.Edited;
        }

        /* The editing command can execute as long as there is a file open and MIDI file playback is stopped.
         * (Note:  When a command cannot execute, its button is automatically disabled.  Therefore,
         * EditButton.IsEnabled cannot be part of the condition here, because then the command could never execute!)
         */
        protected void EditCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState != FileState.None) && (this.UIState != UIState.Play); }

        /* Give the hex text box initial focus. */
        private void HexTextBox_Loaded(object sender, RoutedEventArgs e) { this.HexTextBox.Focus(); }

        /* When the hex changes while editing, make sure the file is marked as edited. */
        private void HexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.UIState != UIState.Edit || this.FileState == FileState.Edited) return;
            this.FileState = FileState.Edited;
        }

        /* When the caret (or current selection) of a text box changes, highlight accordingly. */
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e) { this.Highlight(sender as TextBox); }

        /* Synchronize the scroll position of the other text box. */
        private void TextBox_ScrollChanged(object sender, RoutedEventArgs e)
        { this.GetOtherTextBox(sender).ScrollToVerticalOffset((e as ScrollChangedEventArgs).VerticalOffset); }

        /* If we're really closing, signal the other thread so that it will exit. */
        private void MIDIopsyWindow_Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel) return;
            this.EventHandle.Set();
        }

        #endregion

        private TextBox CreateTextBox(Label label)
        {
            TextBox textBox = new TextBox();
            textBox.IsUndoEnabled = false;
            textBox.IsReadOnly = true;

            if (label != null)
            {
                label.Target = textBox;
                textBox.TextAlignment = TextAlignment.Right;
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
            textBox.SelectionBrush = MIDIopsyWindow.HighlightingBrush;
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

                /* If MIDI file playback is stopped, exit. */
                if (this.UIState != UIState.Play)
                {
                    Logger.WriteMessage("thread exiting");
                    return;
                }

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
            this.InitializePlayback(true);
            this.PopulateTextBoxes();
        }

        /* Initialize playback controls for the MIDI file. */
        private void InitializePlayback(bool reset)
        {
            /* If the file has no path (i.e., it is new), reset its position and duration. */
            if (string.IsNullOrEmpty(this.FilePath))
                this.PositionTextBox.Text = this.DurationTextBox.Text = TimeSpan.Zero.ToString();

            /* Otherwise, open it for media playback.  (This should trigger the MediaOpened
             * event, which will set its initial position and determine its duration.)
             */
            else this.Player.Open(new Uri(this.FilePath));

            /* If specified, reset the starting position. */
            if (!reset) return;
            this.StartingPositionControl.Position = TimeSpan.Zero;
        }

        /* If applicable, stop MIDI file playback. */
        private bool StopPlayback()
        {
            /* If MIDI file playback is already stopped, not applicable. */
            if (this.UIState != UIState.Play) return false;

            /* Block the other thread so that it does not continue to update the Position control after playback is stopped. */
            this.EventHandle.Reset();

            /* The Position control should be accurate (or pretty close) since the other thread has been updating it, but it
             * could be a little off due to minor discrepancies in thread synchronization, so make sure it is accurate now.
             */
            this.SetPosition();

            /* The MIDI file is playing, so stop it. */
            this.Player.Stop();
            this.PlayStopButton.Text = MediaCommands.Play.Text;
            this.UIState = UIState.View;
            return true;
        }

        /* Play the open MIDI file. */
        private void PlayMedia()
        {
            /* If no duration, we can't play. */
            if (!this.Player.NaturalDuration.HasTimeSpan)
            {
                this.ShowFileErrors();
                return;
            }

            /* Validate the starting position. */
            if (this.StartingPositionControl.Position > this.Player.NaturalDuration.TimeSpan)
            {
                string s = Text.ParseLabel(Properties.Resources.StartingPosition);
                s = string.Format(Common.Resources.ValueRangeFormat, s, TimeSpan.Zero, this.Player.NaturalDuration.TimeSpan);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            /* Start playing at the starting position. */
            this.UIState = UIState.Play;
            this.Player.Position = this.StartingPositionControl.Position;
            this.PlayStopButton.Text = MediaCommands.Stop.Text;
            this.Player.Play();

            /* Signal the other thread so that it can proceed with updating the Position control. */
            this.EventHandle.Set();
        }

        /* Show any errors associated with the open MIDI file. */
        private void ShowFileErrors()
        {
            if (string.IsNullOrEmpty(this.MidiFile.ErrorText)) return;

            MessageBox.Show(this, Properties.Resources.FileHasErrors + Environment.NewLine + Environment.NewLine +
                this.MidiFile.ErrorText, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /* Set the Position control to the current position.  (When called by the other thread, this method must be run via
         * the dispatcher, since the media player and the Position control are dispatcher objects owned by the main UI thread.)
         */
        private void SetPosition() { this.PositionTextBox.Text = this.Player.Position.ToString(@"hh\:mm\:ss"); }

        /* "Flash" (temporarily highlight) the starting position (to indicate that it changed).
         * This method should be run in a background thread.
         * Because the Starting Position control is a dispatcher object owned by the main
         * UI thread, a background thread cannot access it, so the starting position must
         * be highlighted (and unhighlighted) on the main UI thread via the dispatcher.
         */
        private void FlashStartingPosition(object state)
        {
            this.Dispatcher.Invoke(new Action<Brush>(this.StartingPositionControl.Highlight), state as Brush);
            Thread.Sleep(MIDIopsyWindow.DefaultTimeout);
            this.Dispatcher.Invoke(this.StartingPositionControl.Unhighlight);
        }

        /* If applicable, stop editing the MIDI file. */
        private bool StopEditing(bool validate)
        {
            /* If we're not editing to begin with, not applicable. */
            if (this.UIState != UIState.Edit) return false;

            /* Before we stop editing, if specified, validate the changes. */
            if (validate && this.ApplyHex() != true) return true;

            /* Changes are valid (or we're not supposed to validate), so stop editing. */
            this.HexTextBox.IsReadOnly = true;
            this.HexTextBox.IsUndoEnabled = false;

            /* Reinstate the comments.  (Repopulating the text boxes has the
             * effect of formatting the hex while replacing the comments.)
             */
            this.PopulateTextBoxes();
            this.EditingPanel.Children.Remove(this.HexTextBox);
            this.TextGrid.Children.Insert(0, this.HexTextBox);
            this.EditingPanel.Children.Add(this.TextGrid);

            /* Back to highlighting (instead of normal selection). */
            this.HexTextBox.SelectionLength = 0;
            this.HexTextBox.SelectionBrush = MIDIopsyWindow.HighlightingBrush;
            this.NoHighlighting = false;

            /* Re-allow playback. */
            this.PlaybackPanel.IsEnabled = true;
            this.UIState = UIState.View;

            /* Finally, change the text of the edit button. */
            this.EditButton.Text = Properties.Resources.StartEditing;
            return true;
        }

        private bool? ApplyHex()
        {
            /* Convert the hex to bytes. */
            List<byte> bytes = new List<byte>(this.HexTextBox.Text.Length / 3);
            string[] strings = this.HexTextBox.Text.Split(MIDIopsyWindow.Separators, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach (string s in strings) bytes.Add(byte.Parse(s, NumberStyles.HexNumber));

                /* Replace the contents of the MIDI file with these bytes.  (This is where errors are most likely.) */
                this.MidiFile.Replace(bytes.ToArray());
                if (!string.IsNullOrEmpty(this.MidiFile.ErrorText)) throw new ApplicationException(this.MidiFile.ErrorText);
            }
            catch (Exception ex)
            {
                Logger.WriteMessage(Properties.Resources.DataInvalid);
                Logger.WriteException(ex);

                /* Inform the user of the error and ask if they want to revert their changes. */
                string s = Text.FormatErrorMessage(Properties.Resources.DataInvalid, ex) +
                    Environment.NewLine + Environment.NewLine + Properties.Resources.RevertToValid;
                MessageBoxResult result = MessageBox.Show(this, s, Meta.Name,
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                /* If we don't revert, the data remains invalid.  Otherwise, replace the hex with the last valid hex. */
                if (result == MessageBoxResult.No) return false;
                this.HexTextBox.Text = this.LastValidHex;
                return null;
            }

            /* It's all good. */
            return true;
        }

        /* Populate the text boxes with the appropriate contents from the MIDI file. */
        private void PopulateTextBoxes()
        {
            this.HexTextBox.Text = this.MidiFile.Hex;
            this.CommentsTextBox.Text = this.MidiFile.Comments;
        }

        /* Highlight a line in a text box. */
        private void Highlight(TextBox textBox)
        {
            /* Highlight only if we're supposed to. */
            if (this.NoHighlighting) return;

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
            this.NoHighlighting = true;
            textBox.Select(i, j);
            this.NoHighlighting = false;
        }

        private TextBox GetOtherTextBox(object textBox)
        { return (textBox == this.HexTextBox) ? this.CommentsTextBox : this.HexTextBox; }

        #endregion
    }
}
