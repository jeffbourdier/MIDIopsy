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


/* FileDialog, OpenFileDialog, SaveFileDialog */
using Microsoft.Win32;

/* Action, Delegate, Environment, EventArgs, Exception, Math, Uri */
using System;

/* StringCollection */
using System.Collections.Specialized;

/* CancelEventArgs */
using System.ComponentModel;

/* Path */
using System.IO;

/* EventResetMode, EventWaitHandle, Thread, ThreadPool */
using System.Threading;

/* DataTemplate, FrameworkElementFactory, GridLength, HorizontalAlignment, MessageBox,
 * MessageBoxButton, MessageBoxImage, MessageBoxResult, RoutedEventArgs, Setter, SizeChangedEventArgs,
 * Style, SystemColors, SystemParameters, TextAlignment, TextWrapping, Thickness, UIElement
 */
using System.Windows;

/* Border, Button, Canvas, ColumnDefinition, Dock, DockPanel, Grid, GridView, GridViewColumn, GroupBox, Label,
 * ListView, ListViewItem, Orientation, RowDefinition, SelectionMode, StackPanel, TextBlock, TextBox, WrapPanel
 */
using System.Windows.Controls;

/* GeneratorStatus */
using System.Windows.Controls.Primitives;

/* Binding */
using System.Windows.Data;

/* ApplicationCommands, CanExecuteRoutedEventArgs, CanExecuteRoutedEventHandler, CommandBinding, CommandManager,
 * ExecutedRoutedEventArgs, ExecutedRoutedEventHandler, Key, KeyGesture, MediaCommands, ModifierKeys, RoutedUICommand
 */
using System.Windows.Input;

/* Brush, Brushes, MediaPlayer */
using System.Windows.Media;


namespace JeffBourdier
{
    /// <summary>Represents a window that makes up the user interface for the MIDIopsy application.</summary>
    public sealed class MIDIopsyWindow : CommonWindow
    {
        /*********
         * Types *
         *********/

        #region Private Types

        /// <summary>Specifies enumerated constants to define the state of a file.</summary>
        private enum FileState { Closed, Saved, Edited }

        /// <summary>Temporarily stores information on a MIDI channel message/event (for cut/copy/paste operations).</summary>
        private class ChannelEvent
        {
            public ChannelEvent(int deltaTime, int status, int data1, int data2)
            {
                this._DeltaTime = deltaTime;
                this._Status = status;
                this._Data1 = data1;
                this._Data2 = data2;
            }

            private int _DeltaTime;
            private int _Status;
            private int _Data1;
            private int _Data2;

            public int DeltaTime { get { return this._DeltaTime; } }
            public int Status { get { return this._Status; } }
            public int Data1 { get { return this._Data1; } }
            public int Data2 { get { return this._Data2; } }
        }

        /// <summary>
        /// Temporarily stores information on a system exclusive (SysEx) message/event (for cut/copy/paste operations).
        /// </summary>
        private class SysExEvent
        {
            public SysExEvent(int deltaTime, bool escape, byte[] bytes)
            {
                this._DeltaTime = deltaTime;
                this._Escape = escape;
                this._Data = bytes;
            }

            private int _DeltaTime;
            private bool _Escape;
            private byte[] _Data;

            public int DeltaTime { get { return this._DeltaTime; } }
            public bool Escape { get { return this._Escape; } }
            public byte[] Data { get { return this._Data; } }
        }

        /// <summary>Temporarily stores information on a meta-event (for cut/copy/paste operations).</summary>
        private class MetaEvent
        {
            public MetaEvent(int deltaTime, int type, byte[] bytes)
            {
                this._DeltaTime = deltaTime;
                this._Type = type;
                this._Data = bytes;
            }

            private int _DeltaTime;
            private int _Type;
            private byte[] _Data;

            public int DeltaTime { get { return this._DeltaTime; } }
            public int Type { get { return this._Type; } }
            public byte[] Data { get { return this._Data; } }
        }

        #endregion

        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a MIDIopsyWindow object.</summary>
        public MIDIopsyWindow()
        {
            StackPanel stackPanel;
            WrapPanel wrapPanel = new WrapPanel();
            DockPanel.SetDock(wrapPanel, Dock.Top);

            /* Build the "File" group box. */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.BindCommand(ApplicationCommands.New, Key.None, ModifierKeys.None,
                this.NewExecuted, this.CanAlwaysExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Open, Key.None, ModifierKeys.None,
                this.OpenExecuted, this.CanAlwaysExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Save, Key.None, ModifierKeys.None,
                this.SaveExecuted, this.SaveCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift,
                this.SaveAsExecuted, this.FileCommandCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Close, Key.F4, ModifierKeys.Control,
                this.CloseExecuted, this.FileCommandCanExecute, stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.File, stackPanel);

            /* Build the "Application" group box. */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.CreateCommand("_" + Properties.Resources.Settings, Key.None,
                ModifierKeys.None, this.SettingsExecuted, this.CanAlwaysExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Help, Key.None, ModifierKeys.None,
                this.HelpExecuted, this.CanAlwaysExecute, stackPanel);
            this.CreateCommand("_" + Properties.Resources.About, Key.None,
                ModifierKeys.None, this.AboutExecuted, this.CanAlwaysExecute, stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.Application, stackPanel);

            /* Build the "New Item" group box. */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.CreateCommand(Properties.Resources.Track, Key.None, ModifierKeys.None,
                this.NewTrackExecuted, this.FileCommandCanExecute, stackPanel);
            this.CreateCommand(Properties.Resources.Channel + Environment.NewLine + "  " + Properties.Resources.Event,
                Key.None, ModifierKeys.None, this.NewChannelEventExecuted, this.BodyCommandCanExecute, stackPanel);
            string s = UI.ParseLabel(Properties.Resources.SysEx);
            this.CreateCommand(s.Insert(s.Length - 1, "_") + " " + Properties.Resources.Event, Key.None,
                ModifierKeys.None, this.NewSysExEventExecuted, this.BodyCommandCanExecute, stackPanel);
            this.CreateCommand("_" + Properties.Resources.MetaEvent, Key.None, ModifierKeys.None,
                this.NewMetaEventExecuted, this.BodyCommandCanExecute, stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.NewItem, stackPanel);

            /* Build the "Edit Item" group box. */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.BindCommand(ApplicationCommands.Properties, Key.None, ModifierKeys.None,
                this.PropertiesExecuted, this.PropertiesCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Cut, Key.None, ModifierKeys.None,
                this.CutExecuted, this.EventCommandCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Copy, Key.None, ModifierKeys.None,
                this.CopyExecuted, this.EventCommandCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Paste, Key.None, ModifierKeys.None,
                this.PasteExecuted, this.PasteCanExecute, stackPanel);
            this.BindCommand(ApplicationCommands.Delete, Key.None, ModifierKeys.None,
                this.DeleteExecuted, this.BodyCommandCanExecute, stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.EditItem, stackPanel);

            /* Build the "Navigation" group box. */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.CreateCommand(Properties.Resources.GoTo, Key.G, ModifierKeys.Control,
                this.GoToExecuted, this.FileCommandCanExecute, stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.Navigation, stackPanel);

            /* Build the "Playback" group box (whose content will be a panel with controls for MIDI file playback). */
            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            this.PlayStopButton = this.CreateCommand(MediaCommands.Play.Text, Key.F5,
                ModifierKeys.None, this.PlayStopExecuted, this.FileCommandCanExecute, stackPanel);
            this.InitPlaybackPosition(stackPanel);
            this.CreateCommand(Properties.Resources.Reset, Key.F6, ModifierKeys.None,
                this.ResetExecuted, this.FileCommandCanExecute, stackPanel);
            this.CreateCommand(Properties.Resources.Sync, Key.F7, ModifierKeys.None,
                this.SyncExecuted, this.FileCommandCanExecute, stackPanel);
            this.InitPlaybackGrid(stackPanel);
            MIDIopsyWindow.AddGroupBox(wrapPanel, Properties.Resources.Playback, stackPanel);

            /* Initialize the media player (used for MIDI file playback).
             * Note that the duration of a MIDI file cannot be determined until it has been
             * opened for media playback (i.e., the MediaOpened event has been raised).
             */
            this.Player = new MediaPlayer();
            this.Player.MediaOpened += this.Player_MediaOpened;
            this.Player.MediaEnded += this.Player_MediaEnded;

            /* Initialize the panel that shows recently opened files. */
            this.RecentPanel = new StackPanel();
            this.RecentPanel.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.TripleSpace);
            this.RecentFilePaths = new string[MIDIopsyWindow.RecentMax];
            if (Properties.Settings.Default.Recent != null && Properties.Settings.Default.Recent.Count > 0)
            {
                /* Copy the number of recent file paths into the array, up to the maximum. */
                int n = Math.Min(Properties.Settings.Default.Recent.Count, MIDIopsyWindow.RecentMax);
                for (int i = 0; i < n; ++i) this.RecentFilePaths[i] = Properties.Settings.Default.Recent[i];
                this.BuildRecentButtons();
            }

            /* Initialize the grid/list view. */
            this.ListView = new ListView();
            this.ListView.ItemContainerStyle = new Style(typeof(ListViewItem));
            Setter setter = new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            this.ListView.ItemContainerStyle.Setters.Add(setter);
            this.GridView = new GridView();
            foreach (MidiItem.DisplayField field in MidiItem.DisplayFields)
            {
                GridViewColumn column = this.BuildColumn(field);
                this.GridView.Columns.Add(column);
            }
            this.ListView.View = this.GridView;
            this.ListView.Margin = new Thickness(0, UI.DoubleSpace, 0, 0);
            this.ListView.SelectionMode = SelectionMode.Single;
            this.ListView.ItemContainerGenerator.StatusChanged += this.ItemContainerGenerator_StatusChanged;

            /* Build the main content panel. */
            this.MainPanel = new DockPanel();
            this.MainPanel.Children.Add(wrapPanel);
            this.MainPanel.Children.Add(new Canvas());
            this.Content = this.MainPanel;

            /* Initialize the window object. */
            this.Icon = MIDIopsyApp.CreateBitmapSource("MIDIopsy.ico");
            this.Title = MIDIopsyApp.Name;
            this.Restore(Properties.Settings.Default.MainWindowMaximized, Properties.Settings.Default.MainWindowBounds);
            this.MinWidth = 960;
            this.MinHeight = 480;
            this.SizeChanged += this.MIDIopsyWindow_SizeChanged;
            this.Closing += this.MIDIopsyWindow_Closing;

            /* Set the initial file state (which also sets the appopriate content). */
            this.State = FileState.Closed;

            /* Start another thread (initally blocked) to update the Position control during MIDI file playback. */
            this.EventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            ThreadPool.QueueUserWorkItem(this.UpdatePosition);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private const int RecentMax = 9;
        private const int DefaultTimeout = 250;
        private static readonly Thickness CellMargin = new Thickness(-6, -3, -6, -3);
        private static readonly Thickness CellPadding = new Thickness(6, 3, 6, 3);

        private CommandButton PlayStopButton;
        private Label StartingPositionLabel;
        private PositionControl StartingPositionControl;
        private Delegate HighlightStartingPosition;
        private Delegate UnhighlightStartingPosition;
        private Grid PlaybackGrid;
        private TextBox PositionTextBox;
        private TextBox DurationTextBox;
        private MediaPlayer Player;
        private StackPanel RecentPanel;
        private string[] RecentFilePaths;
        private ListView ListView;
        private GridView GridView;
        private DockPanel MainPanel;
        private FileState _State;
        private EventWaitHandle EventHandle;
        private HelpWindow HelpWindow = null;
        private MidiFile MidiFile = new MidiFile();
        private string _FilePath = null;
        private object Clipboard = null;
        private bool Playing = false;
        private bool AutoPlay = false;

        #endregion

        /**************
         * Properties *
         **************/

        #region Private Properties

        /// <summary>
        /// Gets or sets the file state, which specifies whether a file is open (or
        /// closed), and if open, whether it is saved (not edited) or edited (not saved).
        /// </summary>
        private FileState State
        {
            get { return this._State; }

            set
            {
                /* If the file state is changing (from anything) to Closed,
                 * disable/reset playback controls and show the Recent panel.
                 */
                if (value == FileState.Closed)
                {
                    this.EnablePlaybackControls(false);
                    this.StartingPositionControl.Position = TimeSpan.Zero;
                    this.PositionTextBox.Text = this.DurationTextBox.Text = TimeSpan.Zero.ToString();
                    this.SetContent(this.RecentPanel);
                }

                /* Otherwise, if the file state is changing from Closed (to
                 * anything), show the list view and enable playback controls.
                 */
                else if (this.State == FileState.Closed)
                {
                    this.SetContent(this.ListView);
                    this.EnablePlaybackControls(true);
                }

                /* If the value is actually changing, set the file state and update the window title. */
                if (this.State == value) return;
                this._State = value;
                this.SetTitle();
            }
        }

        /// <summary>Gets or sets the path of the currently open file.</summary>
        private string FilePath
        {
            get { return this._FilePath; }

            set
            {
                /* If the value is actually changing, set the file path and update the window title. */
                if (this.FilePath == value) return;
                this._FilePath = value;
                this.SetTitle();

                /* If there's no file path, no further action is necessary. */
                if (string.IsNullOrEmpty(value)) return;

                /* Update the recent file list. */
                int i, j = -1;

                /* The idea here is to remove an item from the array, move everything else down, and put the file path
                 * on top.  If the file path is already in the array, remove it.  Otherwise, remove whatever's on bottom.
                 */
                for (i = 0; i < MIDIopsyWindow.RecentMax; ++i)
                {
                    if (this.RecentFilePaths[i] == this.FilePath)
                    {
                        j = i;

                        /* If the file path is already on top, we're done. */
                        if (j == 0) return;
                    }

                    /* If we've reached a null/empty, there are no more. */
                    if (string.IsNullOrEmpty(this.RecentFilePaths[i])) break;

                    /* Remove the button. */
                    this.RecentPanel.Children.RemoveAt(1);
                }

                /* If we got all the way through and didn't find the file path, remove whatever's on bottom. */
                if (j < 0) { if (i == MIDIopsyWindow.RecentMax) j = i - 1; else j = i; }

                /* Remove the chosen item from the array and move everything else down. */
                for (i = j; i > 0; --i) this.RecentFilePaths[i] = this.RecentFilePaths[i - 1];

                /* Put the file path on top, rebuild the buttons, and we're done. */
                this.RecentFilePaths[0] = this.FilePath;
                this.BuildRecentButtons();
            }
        }

        /* Return the index in the MIDI file's list at which a new event should be inserted (based
         * on the selected item), or -1 if the event should be added to the end of the file.
         */
        private int InsertionIndex
        {
            get
            {
                /* If the selected item is last in the list view, the new event should be added to the end of the file. */
                if (this.ListView.SelectedIndex == this.ListView.Items.Count - 1) return -1;

                /* Otherwise, the new event should be inserted into the file (right after the
                 * selected item).  The index in the MIDI file's list should be the same as
                 * the index in the list view, minus the number of separators that precede it.
                 */
                return this.ListView.SelectedIndex - this.SeparatorCount + 1;
            }
        }

        /* Count the number of separators that precede the selected item in the list view. */
        private int SeparatorCount
        {
            get
            {
                int i = this.ListView.SelectedIndex, n = 0;
                while (i > 0) if (this.ListView.Items[--i] is MidiItem.Separator) ++n;
                return n;
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        #region Event Handlers

        #region "Executed" Handlers

        /* Prompt the user for information needed to create a new MIDI file. */
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, no
             * further action is necessary, as the file is not to be closed.
             */
            if (!this.CloseFile()) return;

            /* Prompt the user for header data.  If the user cancels, set file state to Closed and take no further action. */
            MidiHeaderDialog dialog = new MidiHeaderDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) { this.State = FileState.Closed; return; }

            /* The user did not cancel.  Create a new file object using the header data supplied by the user. */
            if (dialog.TicksPerQuarterNote < 0)
                this.MidiFile.Create(dialog.Format, dialog.NumberOfTracks, dialog.FramesPerSecond, dialog.TicksPerFrame);
            else this.MidiFile.Create(dialog.Format, dialog.NumberOfTracks, dialog.TicksPerQuarterNote);

            /* Load the file object into the UI. */
            this.LoadFile();
            this.FilePath = dialog.FileName;

            /* Reset file state accordingly. */
            this.State = FileState.Edited;
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, no
             * further action is necessary, as the file is not to be closed.
             */
            if (!this.CloseFile()) return;

            /* Prompt the user for the file path to open.  If the user clicks
             * OK, a file is to be opened.  Set the file state accordingly.
             */
            this.State = this.OpenFile(null) ? FileState.Saved : FileState.Closed;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Save the file at the current path (if there is one; otherwise, the user will be prompted).
             * If the user does not click OK, no further action is necessary, as the file is not to be saved.
             */
            if (!this.SaveFile(false)) return;

            /* The user clicked OK, so the file is to be saved.  Set the file state accordingly. */
            this.State = FileState.Saved;
        }

        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for a new path at which to save the file.  If the user does
             * not click OK, no further action is necessary, as the file is not to be saved.
             */
            if (!this.SaveFile(true)) return;

            /* The user clicked OK, so the file is to be saved.  Set the file state accordingly. */
            this.State = FileState.Saved;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, no
             * further action is necessary, as the file is not to be closed.
             */
            if (!this.CloseFile()) return;

            /* The user did not cancel (they either saved or discarded their changes),
             * so the file is to be closed.  Reset the file state accordingly.
             */
            this.State = FileState.Closed;
        }

        private void SettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Show the settings dialog.  If the user cancels, take no further action. */
            SettingsDialog dialog = new SettingsDialog();
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Save settings. */
            for (int i = 0; i < MidiItem.DisplayFields.Length; ++i)
            {
                if (MidiItem.DisplayFields[i].RightAlign) continue;
                bool? b = dialog.IsWrapBoxChecked(MidiItem.DisplayFields[i].LabelText);
                object header = this.GridView.Columns[i].Header;
                if (b == MIDIopsyApp.GetWrapSetting(header)) continue;
                Properties.Settings.Default["Wrap" + header] = b;
                this.GridView.Columns.RemoveAt(i);
                GridViewColumn column = this.BuildColumn(MidiItem.DisplayFields[i]);
                if (i < MidiItem.DisplayFields.Length - 1) this.GridView.Columns.Insert(i, column);
                else this.GridView.Columns.Add(column);
            }
            Properties.Settings.Default.Save();
            this.SetColumnWidths();
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* If the help window is already shown, activate it. */
            if (this.HelpWindow != null && this.HelpWindow.IsVisible) { this.HelpWindow.Activate(); return; }

            /* Build and show the help window. */
            this.HelpWindow = new HelpWindow();
            this.HelpWindow.Show();
        }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e) { AboutBox.Show(this); }

        /* Create a new track (MTrk) chunk. */
        private void NewTrackExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            int i = this.CreateTrack(true);
            if (i < 0) return;
            this.FinalizeChanges(i);
        }

        /* Create a new MIDI channel message/event. */
        private void NewChannelEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiChannelEventDialog dialog = new MidiChannelEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            int b = dialog.RunningStatus ? -1 : Midi.MakeByte((int)dialog.MessageType, dialog.Channel),
                i = this.CreateChannelEvent(dialog.DeltaTime, b, dialog.Data1, dialog.Data2);
            this.FinalizeChanges(i);
        }

        /* Create a new MIDI system exclusive (SysEx) message/event. */
        private void NewSysExEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiSysExEventDialog dialog = new MidiSysExEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            int i = this.CreateSysExEvent(dialog.DeltaTime, dialog.Escape, dialog.Data);
            this.FinalizeChanges(i);
        }

        /* Create a new MIDI meta-event. */
        private void NewMetaEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiMetaEventDialog dialog = new MidiMetaEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            int i = this.CreateMetaEvent(dialog.DeltaTime, dialog.Type, dialog.Data);
            this.FinalizeChanges(i);
        }

        /* Edit the properties of the selected MIDI item. */
        private void PropertiesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MidiItemDialog dialog = null;

            /* If the selected MIDI item is header data, use the header dialog. */
            MidiHeader header = this.ListView.SelectedItem as MidiHeader;
            if (header != null) dialog = new MidiHeaderDialog(header);

            /* If the selected MIDI item is a channel message/event, use the channel event dialog. */
            MidiChannelEvent channelEvent = this.ListView.SelectedItem as MidiChannelEvent;
            if (channelEvent != null) dialog = new MidiChannelEventDialog(channelEvent);

            /* If the selected MIDI item is a system exclusive (SysEx) message/event, use the SysEx event dialog. */
            MidiSysExEvent sysExEvent = this.ListView.SelectedItem as MidiSysExEvent;
            if (sysExEvent != null) dialog = new MidiSysExEventDialog(sysExEvent);

            /* If the selected MIDI item is a meta-event, use the meta-event dialog. */
            MidiMetaEvent metaEvent = this.ListView.SelectedItem as MidiMetaEvent;
            if (metaEvent != null) dialog = new MidiMetaEventDialog(metaEvent);

            /* If the dialog has not been initialized by now, the selected
             * MIDI item is not editable (although this should never happen).
             */
            if (dialog == null) return;

            /* Prompt the user to edit the item.  If the user cancels, take no further action. */
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Change the MIDI item based on user input. */
            if (header != null)
            {
                MidiHeaderDialog headerDialog = dialog as MidiHeaderDialog;
                header.Format = headerDialog.Format;
                if (headerDialog.TicksPerQuarterNote < 0)
                    header.SetTimeCodeBasedDivision(headerDialog.FramesPerSecond, headerDialog.TicksPerFrame);
                else header.SetMetricalDivision(headerDialog.TicksPerQuarterNote);
            }
            else
            {
                (this.ListView.SelectedItem as MidiEvent).DeltaTime = (dialog as MidiEventDialog).DeltaTime;

                if (channelEvent != null)
                {
                    MidiChannelEventDialog channelEventDialog = dialog as MidiChannelEventDialog;
                    if (!channelEventDialog.RunningStatus)
                        channelEvent.Status = Midi.MakeByte((int)channelEventDialog.MessageType, channelEventDialog.Channel);
                    channelEvent.Data1 = channelEventDialog.Data1;
                    channelEvent.Data2 = channelEventDialog.Data2;
                }
                else if (sysExEvent != null)
                {
                    MidiSysExEventDialog sysExEventDialog = dialog as MidiSysExEventDialog;
                    sysExEvent.Escape = sysExEventDialog.Escape;
                    sysExEvent.Data = sysExEventDialog.Data;
                }
                else if (metaEvent != null)
                {
                    MidiMetaEventDialog metaEventDialog = dialog as MidiMetaEventDialog;
                    metaEvent.Type = metaEventDialog.Type;
                    metaEvent.Data = metaEventDialog.Data;
                }
            }
            this.FinalizeChanges(this.ListView.SelectedIndex);
        }

        /* Cut (to the clipboard) the selected MIDI item. */
        private void CutExecuted(object sender, ExecutedRoutedEventArgs e) { this.CopyItem(); this.RemoveItem(); }

        /* Copy (to the clipboard) the selected MIDI item. */
        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e) { this.CopyItem(); }

        /* Copy clipboard items (as new items) into the MIDI file. */
        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            int i = -1;

            ChannelEvent channelEvent = this.Clipboard as ChannelEvent;
            if (channelEvent != null) i = this.CreateChannelEvent(channelEvent.DeltaTime,
                channelEvent.Status, channelEvent.Data1, channelEvent.Data2);

            SysExEvent sysExEvent = this.Clipboard as SysExEvent;
            if (sysExEvent != null) i = this.CreateSysExEvent(sysExEvent.DeltaTime, sysExEvent.Escape, sysExEvent.Data);

            MetaEvent metaEvent = this.Clipboard as MetaEvent;
            if (metaEvent != null) i = this.CreateMetaEvent(metaEvent.DeltaTime, metaEvent.Type, metaEvent.Data);

            if (i < 0) return;
            this.FinalizeChanges(i);
        }

        /* Delete the selected MIDI item. */
        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e) { this.RemoveItem(); }

        /* Go to a given place in the file. */
        private void GoToExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Build array of track names. */
            int i, j = 2, n = this.MidiFile.Header.NumberOfTracks;
            string[] trackNames = new string[n];
            for (i = 0; i < n; ++i)
                for (trackNames[i] = null; j < this.ListView.Items.Count; ++j)
                {
                    if (!string.IsNullOrEmpty(trackNames[i])) break;
                    MidiChunkInfo chunkInfo = this.ListView.Items[j] as MidiChunkInfo;
                    if (chunkInfo != null)
                    {
                        if (trackNames[i] != null) break;
                        if (chunkInfo.Type == MidiChunkInfo.TrackType) trackNames[i] = string.Empty;
                        continue;
                    }
                    MidiMetaEvent metaEvent = this.ListView.Items[j] as MidiMetaEvent;
                    if (metaEvent == null) continue;
                    if (metaEvent.Type != MidiMetaEvent.SequenceTrackNameType) continue;
                    trackNames[i] = metaEvent.DataField;
                }

            /* Determine the maximum total time in the MIDI file. */
            for (n = j = 0, i = this.ListView.Items.Count - 1; i > 2; --i)
            {
                if (j == 0)
                {
                    MidiEvent mtrkEvent = this.ListView.Items[i] as MidiEvent;
                    if (mtrkEvent == null) continue;
                    j = 1;
                    if (mtrkEvent.TotalTime > n) n = mtrkEvent.TotalTime;
                }
                else if (!(this.ListView.Items[i] is MidiEvent)) j = 0;
            }

            /* Prompt the user for where to go.  If the user cancels, take no further action. */
            GoToDialog dialog = new GoToDialog(trackNames, n,
                this.Player.NaturalDuration.HasTimeSpan ? this.Player.NaturalDuration.TimeSpan : TimeSpan.MinValue);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Determine which item in the list view to select, based on the
             * track number and total time (in ticks) entered by the user.  If the user entered a position
             * (hh:mm:ss), convert it to a total time, assuming a uniform (average) number of ticks per second.
             */
            n = (dialog.TotalTime < 0) ?
                (int)(n * dialog.Position.TotalSeconds / this.Player.NaturalDuration.TimeSpan.TotalSeconds) : dialog.TotalTime;
            for (j = 0, i = 2; i < this.ListView.Items.Count; ++i)
            {
                /* First, find the desired track. */
                if (j < dialog.TrackNumber)
                {
                    MidiChunkInfo chunkInfo = this.ListView.Items[i] as MidiChunkInfo;
                    if (chunkInfo != null && chunkInfo.Type == MidiChunkInfo.TrackType) ++j;
                    continue;
                }

                /* We have found the desired track.  Now find (within this track) the first event whose total time
                 * is equal to (or the last event whose total time is less than) the total time entered by the user.
                 */
                MidiEvent mtrkEvent = this.ListView.Items[i] as MidiEvent;
                if (mtrkEvent == null || mtrkEvent.TotalTime > n) break;
                if (mtrkEvent.TotalTime == n) { ++i; break; }
            }
            while (this.ListView.Items[--i] is MidiItem.Separator) ;
            this.ListView.SelectedIndex = i;
            this.ListView.ScrollIntoView(this.ListView.SelectedItem);
        }

        /* Execute the Play/Stop command (toggle MIDI file playback). */
        private void PlayStopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* If we're stopping, we can be done right here. */
            if (this.StopPlayback()) return;

            /* If the file is edited, prompt the user to save and reload. */
            if (this.State == FileState.Edited)
                if (PromptUser(Properties.Resources.Reload, true))
                {
                    /* The user chose to save and reload.  Set the flag to play the file as
                     * soon as it's reloaded.  Executing the Save command (if the file is saved
                     * successfully) will trigger the MediaOpened event, which will play the file.
                     */
                    this.AutoPlay = true;
                    ApplicationCommands.Save.Execute(null, null);

                    /* If the file was not saved, don't play. */
                    if (this.State != FileState.Saved) this.AutoPlay = false;

                    /* Regardless, we're done here. */
                    return;
                }

            /* Either the file is not edited, or the user chose not to save and reload.  Either way, play the file. */
            this.PlayMedia();
        }

        /* Execute the Reset command (reset the starting position to zero. */
        private void ResetExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.StartingPositionControl.Position = TimeSpan.Zero;
            ThreadPool.QueueUserWorkItem(this.FlashStartingPosition, Brushes.Orange);
        }

        /* Execute the Sync command (set the starting position equal to the current position. */
        private void SyncExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.StartingPositionControl.Position = TimeSpan.Parse(this.PositionTextBox.Text);
            ThreadPool.QueueUserWorkItem(this.FlashStartingPosition, Brushes.Yellow);
        }

        #endregion

        #region "CanExecute" Handlers

        /* Some commands can always execute. */
        private void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }

        /* The Save command can execute only if the file is marked as edited. */
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State == FileState.Edited); }

        /* Some commands can execute as long as there is a file open. */
        private void FileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.Closed); }

        /* Commands that apply only to the "body" of the MIDI file can execute as long as there is a
         * file open and the selected item is neither of the first two (presumably, the header chunk).
         */
        private void BodyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.Closed && this.ListView.SelectedIndex > 1); }

        /* The command that edits the properties of the selected MIDI item can execute
         * as long as there is a file open and the selected item is not chunk info.
         */
        private void PropertiesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.Closed && !(this.ListView.SelectedItem is MidiChunkInfo)); }

        /* Commands that apply only to MIDI (MTrk) events can execute as long as
         * there is a file open and the selected item is in fact a MIDI event.
         */
        private void EventCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.Closed && this.ListView.SelectedItem is MidiEvent); }

        /* The Paste command can execute as long as there is a file open, the clipboard is not
         * empty, and the selected item is neither of the first two (presumably, the header chunk).
         */
        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.State != FileState.Closed && this.Clipboard != null && this.ListView.SelectedIndex > 1); }

        #endregion

        /* Once a MIDI file has been opened for media playback, set its initial position and determine its duration. */
        private void Player_MediaOpened(object sender, EventArgs e)
        {
            this.SetPosition(null);
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

            /* For whatever reason, the CanExecute event handler is not automatically
             * called here, so trigger it by raising the CanExecuteChanged event.
             */
            CommandManager.InvalidateRequerySuggested();
        }

        /* This event handler is called by the buttons representing recently opened files. */
        private void RecentButton_Click(object sender, RoutedEventArgs e)
        {
            /* Open the file specified by the button's content. */
            string s = (sender as Button).Content.ToString().Substring(1, 1);
            s = this.RecentFilePaths[int.Parse(s) - 1];
            if (!this.OpenFile(s)) return;
            this.State = FileState.Saved;
        }

        /* The item container generator acts on behalf of its host (the list view) to generate the UI (including
         * ListViewItem objects, which contain the data items in the list view).  When the generator has finished
         * generating the containers (i.e., the ListViewItem objects), give the selected item (if there is one) the focus.
         */
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            int i = this.ListView.SelectedIndex;
            if (i < 0) return;
            ListViewItem item = this.ListView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
            if (item == null) return;
            item.Focus();
        }

        private void MIDIopsyWindow_SizeChanged(object sender, SizeChangedEventArgs e) { this.SetColumnWidths(); }

        private void MIDIopsyWindow_Closing(object sender, CancelEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, do not close the window. */
            if (e.Cancel = !this.CloseFile()) return;

            /* The user did not cancel (they either saved or discarded their changes), so this
             * window will close.  Start cleaning up by closing the help window (if it's open).
             */
            if (this.HelpWindow != null) this.HelpWindow.Close();

            /* Save window and recent file settings. */
            Properties.Settings.Default.MainWindowMaximized = this.Maximized;
            Properties.Settings.Default.MainWindowBounds = this.Bounds;
            Properties.Settings.Default.Recent = new StringCollection();
            for (int i = 0; i < MIDIopsyWindow.RecentMax; ++i)
            {
                if (string.IsNullOrEmpty(this.RecentFilePaths[i])) continue;
                Properties.Settings.Default.Recent.Add(this.RecentFilePaths[i]);
            }
            Properties.Settings.Default.Save();

            /* Signal the other thread so that it will exit. */
            this.EventHandle.Set();
        }

        #endregion

        /// <summary>
        /// Creates a new routed UI command with an optional keyboard shortcut (i.e., a key gesture with the specified
        /// key and modifier keys), associates (binds) the command to the specified event handlers, adds the command
        /// binding to the UI, creates a button for the command, and adds the command button to a stack panel.
        /// </summary>
        /// <param name="text">The text that describes the command.</param>
        /// <param name="key">The key associated with the gesture.</param>
        /// <param name="modifiers">The modifier keys associated with the gesture.</param>
        /// <param name="executed">The handler for the Executed event on the command.</param>
        /// <param name="canExecute">The handler for the CanExecute event on the command.</param>
        /// <param name="panel">A stack panel to which the command button is added.</param>
        /// <returns>A new instance of the CommandButton class.</returns>
        private CommandButton CreateCommand(string text, Key key, ModifierKeys modifiers,
            ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, StackPanel panel)
        {
            /* Create the command and set its text property. */
            RoutedUICommand command = new RoutedUICommand();
            command.Text = text;

            /* Bind the command and return its button. */
            return this.BindCommand(command, key, modifiers, executed, canExecute, panel);
        }

        /// <summary>
        /// Associates a routed UI command with an optional keyboard shortcut (i.e., a key gesture with
        /// the specified key and modifier keys) to the specified event handlers, adds the command binding
        /// to the UI, creates a button for the command, and adds the command button to a stack panel.
        /// </summary>
        /// <param name="command">A RoutedUICommand object.</param>
        /// <param name="key">The key associated with the gesture.</param>
        /// <param name="modifiers">The modifier keys associated with the gesture.</param>
        /// <param name="executed">The handler for the Executed event on the command.</param>
        /// <param name="canExecute">The handler for the CanExecute event on the command.</param>
        /// <param name="panel">A stack panel to which the command button is added.</param>
        /// <returns>A new instance of the CommandButton class.</returns>
        private CommandButton BindCommand(RoutedUICommand command, Key key, ModifierKeys modifiers,
            ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, StackPanel panel)
        {
            /* If specified, define a key gesture for the command. */
            if (key != Key.None)
            {
                KeyGesture gesture = new KeyGesture(key, modifiers);
                command.InputGestures.Add(gesture);
            }

            /* Bind the command and add the binding to the UI. */
            CommandBinding binding = new CommandBinding(command, executed, canExecute);
            this.CommandBindings.Add(binding);

            /* Create a command button and add it to the panel. */
            CommandButton button = new CommandButton(command);
            panel.Children.Add(button);
            return button;
        }

        /* Initialize the Starting Position control for the "Playback" group box. */
        private void InitPlaybackPosition(StackPanel panel)
        {
            /* This grid will contain the position control and label. */
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions[0].Height = GridLength.Auto;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions[1].Height = GridLength.Auto;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.Margin = new Thickness(UI.UnitSpace);
            panel.Children.Add(grid);

            /* Add the label to the top row. */
            this.StartingPositionLabel = UI.CreateLabel(MarginType.None, Properties.Resources.StartingPosition, true);
            this.StartingPositionLabel.Margin = new Thickness(0, 0, 0, UI.UnitSpace);
            this.StartingPositionLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            grid.Children.Add(this.StartingPositionLabel);

            /* Add the position control to the bottom row. */
            this.StartingPositionControl = new PositionControl();
            Grid.SetRow(this.StartingPositionControl, 1);
            grid.Children.Add(this.StartingPositionControl);
            this.StartingPositionLabel.Target = this.StartingPositionControl.InitialElement;

            /* Because the Starting Position control is a dispatcher object owned by the main UI thread,
             * a background thread cannot access it.  These delegates allow a background thread to
             * highlight (and unhighlight) the starting position on the main UI thread via the dispatcher.
             */
            this.HighlightStartingPosition = new Action<Brush>(this.StartingPositionControl.Highlight);
            this.UnhighlightStartingPosition = new Action<object>(this.StartingPositionControl.Unhighlight);
        }

        /* Initialize the grid containing the Position and Duration controls for the "Playback" group box. */
        private void InitPlaybackGrid(StackPanel panel)
        {
            this.PlaybackGrid = new Grid();
            this.PlaybackGrid.RowDefinitions.Add(new RowDefinition());
            this.PlaybackGrid.RowDefinitions.Add(new RowDefinition());
            this.PlaybackGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.PlaybackGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.PlaybackGrid.ColumnDefinitions[1].Width = new GridLength(64);
            this.PlaybackGrid.Margin = new Thickness(0, 0, UI.UnitSpace, 0);
            this.PositionTextBox = this.AddTextBoxWithLabel(0, Properties.Resources.Position);
            this.DurationTextBox = this.AddTextBoxWithLabel(1, Properties.Resources.Duration);
            panel.Children.Add(this.PlaybackGrid);
        }

        private TextBox AddTextBoxWithLabel(int rowValue, object labelContent)
        {
            /* Create the text box. */
            TextBox textBox = new TextBox();
            textBox.IsUndoEnabled = false;
            textBox.IsReadOnly = true;
            textBox.TextAlignment = TextAlignment.Right;
            textBox.GotFocus += UI.TextBox_GotFocus;
            textBox.Margin = new Thickness(0, UI.HalfSpace, 0, UI.HalfSpace);
            Grid.SetRow(textBox, rowValue);
            Grid.SetColumn(textBox, 1);

            /* Create the label. */
            Label label = UI.CreateLabel(MarginType.None, labelContent, false);
            label.Target = textBox;
            Grid.SetRow(label, rowValue);

            /* Add the new controls. */
            this.PlaybackGrid.Children.Add(label);
            this.PlaybackGrid.Children.Add(textBox);
            return textBox;
        }

        private void EnablePlaybackControls(bool enable)
        {
            this.StartingPositionLabel.IsEnabled = enable;
            this.StartingPositionControl.IsEnabled = enable;
            this.PlaybackGrid.IsEnabled = enable;
        }

        private static void AddGroupBox(WrapPanel panel, object header, object content)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Header = header;
            groupBox.Content = content;
            groupBox.Margin = new Thickness(UI.DoubleSpace, UI.UnitSpace, UI.DoubleSpace, UI.UnitSpace);
            panel.Children.Add(groupBox);
        }

        /* Build (or rebuild) the buttons for the panel that shows recently opened files. */
        private void BuildRecentButtons()
        {
            /* If necessary, add "Open Recent" label to the panel. */
            if (this.RecentPanel.Children.Count < 1)
            {
                Label label = UI.CreateLabel(MarginType.None, Properties.Resources.OpenRecent, false);
                this.RecentPanel.Children.Add(label);
            }

            /* Iterate through each item in the array, up to the max. */
            for (int i = 0; i < MIDIopsyWindow.RecentMax; ++i)
            {
                /* If we've reached a null/empty, there are no more. */
                if (string.IsNullOrEmpty(this.RecentFilePaths[i])) break;

                /* Build and add a new button for this file path. */
                Button button = new Button();
                button.Content = string.Format("_{0} {1}", i + 1, this.RecentFilePaths[i]);
                button.Padding = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Click += this.RecentButton_Click;
                this.RecentPanel.Children.Add(button);
            }
        }

        /* Replace the lower child of the main content panel with the specified UI element. */
        private void SetContent(UIElement element)
        {
            this.MainPanel.Children.RemoveAt(1);
            this.MainPanel.Children.Add(element);
        }

        /* Set the window title appropriately based on the file path/name and the file state. */
        private void SetTitle()
        {
            this.Title = string.Format("{0}{1}{2}{3}", Path.GetFileName(this.FilePath),
                (this.State == FileState.Edited) ? "*" : string.Empty,
                (this.State == FileState.Closed) ? string.Empty : " - ", MIDIopsyApp.Name);
        }

        private GridViewColumn BuildColumn(MidiItem.DisplayField field)
        {
            /* Create the column and set its header content. */
            GridViewColumn column = new GridViewColumn();
            column.Header = UI.ParseLabel(field.LabelText);

            /* By default, there is no template used to display cell contents.  Setting this template allows us to improve
             * readability by adding grid lines (i.e., cell borders) and aligning text (e.g., right-justifying numeric fields).
             */
            column.CellTemplate = new DataTemplate();
            column.CellTemplate.VisualTree = new FrameworkElementFactory(typeof(Border));
            column.CellTemplate.VisualTree.SetValue(Border.MarginProperty, MIDIopsyWindow.CellMargin);
            column.CellTemplate.VisualTree.SetValue(Border.BorderBrushProperty, SystemColors.ControlLightBrush);
            column.CellTemplate.VisualTree.SetValue(Border.BorderThicknessProperty, field.BorderThickness);

            /* Because each column is bound to a different data item, each column needs
             * its own tree node, including a child factory with a unique data binding.
             */
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            Binding binding = new Binding(field.BindingPath);
            factory.SetBinding(TextBlock.TextProperty, binding);
            factory.SetValue(TextBlock.PaddingProperty, MIDIopsyWindow.CellPadding);
            if (field.Monospace) factory.SetValue(TextBlock.FontFamilyProperty, UI.MonospaceFont);
            if (field.RightAlign) factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            else
            {
                /* Determine whether or not the text in this column should wrap (based on settings in the config file). */
                bool? b = MIDIopsyApp.GetWrapSetting(column.Header);
                if (b != null)
                    factory.SetValue(TextBlock.TextWrappingProperty, (b == true) ? TextWrapping.Wrap : TextWrapping.NoWrap);
            }
            column.CellTemplate.VisualTree.AppendChild(factory);
            return column;
        }

        /// <summary>Opens an existing MIDI file from disk.</summary>
        /// <param name="filePath">The path of the file being opened, or null/empty to prompt the user.</param>
        /// <returns>True if the file is to be opened; otherwise, false.</returns>
        private bool OpenFile(string filePath)
        {
            /* If no file path is supplied, prompt the user. */
            if (string.IsNullOrEmpty(filePath))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (!this.OpenOrSaveFile(dialog)) return false;
            }
            else this.FilePath = filePath;

            /* Create a new file object using the file path. */
            try { this.MidiFile.Load(this.FilePath); }
            catch (Exception ex)
            {
                this.ShowErrorMessage(Properties.Resources.FileNotOpened, ex);
                this.FilePath = null;
                return false;
            }

            /* Load the file object into the UI. */
            this.LoadFile();
            this.ShowFileErrors();
            return true;
        }

        /* Perform all actions necessary to load the MIDI file object into the UI. */
        private void LoadFile()
        {
            this.InitializePlayback(true);

            /* Populate the list view with the contents of the MIDI file. */
            for (int i = 0; i < this.MidiFile.ItemCount; ++i)
            {
                MidiItem item = this.MidiFile.GetItem(i);

                /* Add a separator before each chunk (after the first). */
                if (i > 0 && item is MidiChunkInfo) this.ListView.Items.Add(new MidiItem.Separator());

                this.ListView.Items.Add(item);
            }
            this.ListView.SelectedIndex = 0;
            this.ListView.ScrollIntoView(this.ListView.SelectedItem);
            this.SetColumnWidths();
        }

        /* Reset the width of each column of the grid/list view. */
        private void SetColumnWidths()
        {
            double d = this.ActualWidth - 50;
            for (int i = 0; i < MidiItem.DisplayFields.Length; ++i)
                this.GridView.Columns[i].Width = MidiItem.DisplayFields[i].WidthFactor * d;
        }

        /* Create a new track (MTrk) chunk. */
        private int CreateTrack(bool end)
        {
            /* Tracks cannot be added to a format 0 file. */
            if (this.MidiFile.Header.Format == 0)
            {
                MessageBox.Show(this, Properties.Resources.OneTrack,
                    MIDIopsyApp.Name, MessageBoxButton.OK, MessageBoxImage.Hand);
                return -1;
            }

            MidiItem.Separator separator = new MidiItem.Separator();
            MidiChunkInfo chunkInfo;
            MidiMetaEvent metaEvent;

            /* Starting from the selected item, find the next chunk in the list view, and insert the new track there. */
            for (int i = this.ListView.SelectedIndex + 1; i < this.ListView.Items.Count; ++i)
            {
                chunkInfo = this.ListView.Items[i] as MidiChunkInfo;
                if (chunkInfo == null) continue;

                /* We have found the next chunk.  Insert into the file a new track chunk, followed by an End of Track event. */
                int j = i - this.SeparatorCount;
                chunkInfo = this.MidiFile.InsertTrack(--j);
                metaEvent = end ? this.MidiFile.InsertMetaEvent(++j, 0, MidiMetaEvent.EndOfTrackType, null) : null;

                /* Correspondingly, insert into the list view a separator, the new track chunk, and the End of Track event. */
                this.ListView.Items.Insert(--i, separator);
                this.ListView.Items.Insert(++i, chunkInfo);
                if (end) this.ListView.Items.Insert(++i, metaEvent);
                return --i;
            }

            /* If there is no next chunk in the list view, add the new
             * track chunk and End of Track event to the end of the file.
             */
            chunkInfo = this.MidiFile.AddTrack();
            metaEvent = end ? this.MidiFile.AddMetaEvent(0, MidiMetaEvent.EndOfTrackType, null) : null;

            /* Correspondingly, add to the list view a separator, new track chunk, and End of Track event. */
            this.ListView.Items.Add(separator);
            this.ListView.Items.Add(chunkInfo);
            if (end) this.ListView.Items.Add(metaEvent);
            return this.ListView.Items.Count - 2;
        }

        /* Create a new MIDI channel message/event. */
        private int CreateChannelEvent(int deltaTime, int status, int data1, int data2)
        {
            int i = this.InsertionIndex, j = this.ListView.SelectedIndex + 1;
            MidiChannelEvent channelEvent;

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                channelEvent = this.MidiFile.AddChannelEvent(deltaTime, status, data1, data2);
                this.ListView.Items.Add(channelEvent);
                return j;
            }

            /* Otherwise, insert the new event right after the selected item. */
            channelEvent = this.MidiFile.InsertChannelEvent(i, deltaTime, status, data1, data2);
            this.ListView.Items.Insert(j, channelEvent);
            return j;
        }

        /* Create a new MIDI system exclusive (SysEx) message/event. */
        private int CreateSysExEvent(int deltaTime, bool escape, byte[] bytes)
        {
            int i = this.InsertionIndex, j = this.ListView.SelectedIndex + 1;
            MidiSysExEvent sysExEvent;

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                sysExEvent = this.MidiFile.AddSysExEvent(deltaTime, escape, bytes);
                this.ListView.Items.Add(sysExEvent);
                return j;
            }

            /* Otherwise, insert the new event right after the selected item. */
            sysExEvent = this.MidiFile.InsertSysExEvent(i, deltaTime, escape, bytes);
            this.ListView.Items.Insert(j, sysExEvent);
            return j;
        }

        /* Create a new MIDI meta-event. */
        private int CreateMetaEvent(int deltaTime, int type, byte[] bytes)
        {
            int i = this.InsertionIndex, j = this.ListView.SelectedIndex + 1;
            MidiMetaEvent metaEvent;

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                metaEvent = this.MidiFile.AddMetaEvent(deltaTime, type, bytes);
                this.ListView.Items.Add(metaEvent);
                return j;
            }

            /* Otherwise, insert the new event right after the selected item. */
            metaEvent = this.MidiFile.InsertMetaEvent(i, deltaTime, type, bytes);
            this.ListView.Items.Insert(j, metaEvent);
            return j;
        }

        /* Copy the selected MIDI item to the clipboard. */
        private void CopyItem()
        {
            MidiChannelEvent channelEvent = this.ListView.SelectedItem as MidiChannelEvent;
            if (channelEvent != null) this.Clipboard = new ChannelEvent(channelEvent.DeltaTime,
                (channelEvent.RunningStatus ? -1 : channelEvent.Status), channelEvent.Data1, channelEvent.Data2);
            MidiSysExEvent sysExEvent = this.ListView.SelectedItem as MidiSysExEvent;
            if (sysExEvent != null) this.Clipboard = new SysExEvent(sysExEvent.DeltaTime, sysExEvent.Escape, sysExEvent.Data);
            MidiMetaEvent metaEvent = this.ListView.SelectedItem as MidiMetaEvent;
            if (metaEvent != null) this.Clipboard = new MetaEvent(metaEvent.DeltaTime, metaEvent.Type, metaEvent.Data);
        }

        /* Remove the selected MIDI item. */
        private void RemoveItem()
        {
            /* Determine if the selected MIDI item can/should be removed (and whether or not it is chunk info). */
            bool? b = this.CheckRemoval();
            if (b == false) return;

            /* We have clearance.  Remove the selected item from the MIDI file. */
            int i = this.ListView.SelectedIndex;
            this.MidiFile.RemoveItem(i - this.SeparatorCount);

            /* Now for the list view.  Remove at least the selected item.  If appropriate, do the
             * same for all items in the chunk, as well as the separator right before the chunk.
             */
            do this.ListView.Items.RemoveAt(i);
            while (b == null && i < this.ListView.Items.Count && !(this.ListView.Items[i] is MidiItem.Separator));
            if (b == null) this.ListView.Items.RemoveAt(i - 1);

            /* With the selected item removed, there is now no selection.
             * For convenience, select the nearest non-separator item.
             */
            while (i < this.ListView.Items.Count && this.ListView.Items[i++] is MidiItem.Separator) ;
            if (i > this.ListView.Items.Count) i = this.ListView.Items.Count;
            while (this.ListView.Items[--i] is MidiItem.Separator) ;
            this.FinalizeChanges(i);
        }

        /* Determine whether or not the selected MIDI item can (or should) be removed. */
        private bool? CheckRemoval()
        {
            /* If the selected MIDI item is a channel message/event that does not use
             * running status, check for dependent events (which do use running status).
             */
            MidiChannelEvent channelEvent = this.ListView.SelectedItem as MidiChannelEvent;
            if (channelEvent != null && !channelEvent.RunningStatus)
            {
                /* Find the next channel event in the chunk.  If it uses running status, warn the user. */
                for (int i = this.ListView.SelectedIndex + 1; i < this.ListView.Items.Count; ++i)
                {
                    if (this.ListView.Items[i] is MidiItem.Separator) return true;
                    channelEvent = this.ListView.Items[i] as MidiChannelEvent;
                    if (channelEvent == null) continue;
                    if (!channelEvent.RunningStatus) return true;
                    return this.PromptUser(Properties.Resources.ImpactRunningStatus, false);
                }
                return true;
            }

            /* Otherwise, as long as the selected MIDI item is not chunk info, it can be removed. */
            MidiChunkInfo chunkInfo = this.ListView.SelectedItem as MidiChunkInfo;
            if (chunkInfo == null) return true;

            /* If the selected MIDI item represents the only track, warn the user. */
            if (chunkInfo.Type == MidiChunkInfo.TrackType && this.MidiFile.Header.NumberOfTracks == 1)
                return (this.PromptUser(Properties.Resources.OnlyTrack, false)) ? null : (bool?)false;

            /* Otherwise, simply warn the user that the entire chunk will be removed. */
            return (this.PromptUser(Properties.Resources.EntireChunk, true)) ? null : (bool?)false;
        }

        /* Ask the user a Yes/No question. */
        private bool PromptUser(string text, bool defaultYes)
        {
            MessageBoxResult result = MessageBox.Show(this, text, MIDIopsyApp.Name, MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation, defaultYes ? MessageBoxResult.Yes : MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }

        /* This method should be called after any changes to the MIDI file. */
        private void FinalizeChanges(int index)
        {
            /* Refresh the view so that the changes show up. */
            this.ListView.Items.Refresh();
            this.ListView.SelectedIndex = index;

            /* Mark the file as edited. */
            this.State = FileState.Edited;
        }

        /// <summary>Closes the open MIDI file.</summary>
        /// <returns>True if the file is to be closed; otherwise, false.</returns>
        private bool CloseFile()
        {
            /* If the file state is already Closed, we're done. */
            if (this.State == FileState.Closed) return true;

            /* If the file state is Edited, prompt the user to save changes. */
            if (this.State == FileState.Edited)
            {
                string s = string.Format(Properties.Resources.SaveChangesFormat, this.FilePath);
                MessageBoxResult result = MessageBox.Show(this, s, MIDIopsyApp.Name, MessageBoxButton.YesNoCancel);

                /* If the user canceled, never mind. */
                if (result == MessageBoxResult.Cancel) return false;

                /* If the user clicked Yes, save the file at the current path
                 * (if there is one; otherwise, the user will be prompted).
                 */
                if (result == MessageBoxResult.Yes && !this.SaveFile(false)) return false;
            }

            /* The user did not cancel (they either saved or discarded their changes), so unload the file. */
            this.FilePath = null;
            this.StopPlayback();
            this.Player.Close();
            this.ListView.Items.Clear();
            this.MidiFile.Clear();
            return true;
        }

        /// <summary>Saves the open MIDI file to disk.</summary>
        /// <param name="asNew">
        /// True if the file is to be saved "as new" (i.e., it has not
        /// already been saved at the current path); otherwise, false.
        /// </param>
        /// <returns>True if the file is to be saved; otherwise, false.</returns>
        private bool SaveFile(bool asNew)
        {
            /* If the file is playing, warn the user that this will stop it. */
            if (this.Playing) if (!this.PromptUser(Properties.Resources.StopPlayback, true)) return false;

            /* If necessary, prompt the user for a file path. */
            if (asNew || string.IsNullOrEmpty(this.FilePath))
            {
                SaveFileDialog dialog = new SaveFileDialog();
                if (!this.OpenOrSaveFile(dialog)) return false;
            }

            /* Save the file and reload it for media playback.  (The sleep is to help ensure
             * that the media player has released the file before attempting to write to it.)
             */
            this.StopPlayback();
            this.Player.Close();
            Thread.Sleep(MIDIopsyWindow.DefaultTimeout);
            try { this.MidiFile.Save(this.FilePath); }
            catch (Exception ex) { this.ShowErrorMessage(Properties.Resources.FileNotSaved, ex); return false; }
            this.InitializePlayback(false);
            return true;
        }

        private bool OpenOrSaveFile(FileDialog dialog)
        {
            /* Prompt the user for the file path.  If the user does not click OK, return false. */
            dialog.Filter = Properties.Resources.MidiFiles;
            bool? result = dialog.ShowDialog(this);
            if (result != true) return false;

            /* The user clicked OK.  Now that we have a file path, set properties appropriately. */
            this.FilePath = dialog.FileName;
            return true;
        }

        /* Play the open MIDI file. */
        private void PlayMedia()
        {
            /* If no duration, we can't play. */
            if (!this.Player.NaturalDuration.HasTimeSpan) { this.ShowFileErrors(); return; }

            /* Validate the starting position. */
            if (this.StartingPositionControl.Position > this.Player.NaturalDuration.TimeSpan)
            {
                string s = UI.ParseLabel(Properties.Resources.StartingPosition);
                s = string.Format(Properties.Resources.ValueRangeFormat, s,
                    TimeSpan.Zero, this.Player.NaturalDuration.TimeSpan);
                MessageBox.Show(this, s, MIDIopsyApp.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            /* Start playing at the starting position. */
            this.Playing = true;
            this.Player.Position = this.StartingPositionControl.Position;
            this.PlayStopButton.Text = MediaCommands.Stop.Text;
            this.Player.Play();

            /* Signal the other thread so that it can proceed with updating the Position control. */
            this.EventHandle.Set();
        }

        /* If applicable, stop MIDI file playback. */
        private bool StopPlayback()
        {
            /* If MIDI file playback is already stopped, not applicable. */
            if (!this.Playing) return false;

            /* Block the other thread so that it does not continue to update the Position control after playback is stopped. */
            this.EventHandle.Reset();

            /* The Position control should be accurate (or pretty close) since the other thread has been updating it, but it
             * could be a little off due to minor discrepancies in thread synchronization, so make sure it is accurate now.
             */
            this.SetPosition(null);

            /* The MIDI file is playing, so stop it. */
            this.Player.Stop();
            this.PlayStopButton.Text = MediaCommands.Play.Text;
            this.Playing = false;
            return true;
        }

        /* Show any errors associated with the open MIDI file. */
        private void ShowFileErrors()
        {
            if (string.IsNullOrEmpty(this.MidiFile.ErrorText)) return;
            MessageBox.Show(this, Properties.Resources.FileHasErrors + Environment.NewLine + Environment.NewLine +
                this.MidiFile.ErrorText, MIDIopsyApp.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /* Show an error message using the standard format. */
        private void ShowErrorMessage(string text, Exception ex)
        {
            string s = string.Format(Properties.Resources.ErrorMessageFormat, text, Environment.NewLine, ex.Message);
            MessageBox.Show(this, s, MIDIopsyApp.Name, MessageBoxButton.OK, MessageBoxImage.Error);
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

        /* This method runs in its own thread to update the Position control during MIDI file playback. */
        private void UpdatePosition(object state)
        {
            Delegate setPosition = new Action<object>(this.SetPosition);

            while (true)
            {
                /* Block until signaled. */
                this.EventHandle.WaitOne();

                /* If MIDI file playback is stopped, exit. */
                if (!this.Playing) return;

                /* Because the media player and the Position control are dispatcher objects owned by the main UI thread,
                 * this thread cannot access them, so the position must be set on the main UI thread via the dispatcher.
                 */
                this.Dispatcher.Invoke(setPosition, null as object);

                /* Wait a second, then repeat. */
                Thread.Sleep(1000);
            }
        }

        /* Set the Position control to the current position.  (When called by the other thread, this method must be run via
         * the dispatcher, since the media player and the Position control are dispatcher objects owned by the main UI thread.)
         */
        private void SetPosition(object o) { this.PositionTextBox.Text = this.Player.Position.ToString().Substring(0, 8); }

        /* "Flash" (temporarily highlight) the starting position (to indicate that it changed).
         * This method should be run in a background thread.
         * Because the Starting Position control is a dispatcher object owned by the main
         * UI thread, a background thread cannot access it, so the starting position must
         * be highlighted (and unhighlighted) on the main UI thread via the dispatcher.
         */
        private void FlashStartingPosition(object state)
        {
            this.Dispatcher.Invoke(this.HighlightStartingPosition, state as Brush);
            Thread.Sleep(MIDIopsyWindow.DefaultTimeout);
            this.Dispatcher.Invoke(this.UnhighlightStartingPosition, null as object);
        }

        #endregion
    }
}
