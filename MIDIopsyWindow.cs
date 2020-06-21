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


/* Action, Delegate, Environment, EventArgs, Exception, Uri */
using System;

/* List */
using System.Collections.Generic;

/* CancelEventArgs */
using System.ComponentModel;

/* EventResetMode, EventWaitHandle, Thread, ThreadPool */
using System.Threading;

/* DataTemplate, FrameworkElementFactory, GridLength, HorizontalAlignment, MessageBox, MessageBoxButton, MessageBoxImage,
 * MessageBoxResult, Setter, Style, SystemColors, SystemParameters, TextAlignment, TextWrapping, Thickness
 */
using System.Windows;

/* Border, ColumnDefinition, Dock, DockPanel, Grid, GridView, GridViewColumn, GroupBox,
 * Label, ListView, ListViewItem, Orientation, TextBlock, TextBox, StackPanel, WrapPanel
 */
using System.Windows.Controls;

/* GeneratorStatus */
using System.Windows.Controls.Primitives;

/* Binding */
using System.Windows.Data;

/* ApplicationCommands, CanExecuteRoutedEventArgs, CommandBinding, CommandManager,
 * ExecutedRoutedEventArgs, KeyGesture, MediaCommands, RoutedUICommand
 */
using System.Windows.Input;

/* Brush, Brushes, MediaPlayer */
using System.Windows.Media;

/* BitmapFrame */
using System.Windows.Media.Imaging;


namespace JeffBourdier
{
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
            CommandBinding binding;
            List<RoutedUICommand> commands;
            KeyGesture gesture;
            RoutedUICommand command;

            /* Create and bind the command to create a new track. */
            RoutedUICommand newTrackCommand = new RoutedUICommand();
            newTrackCommand.Text = Properties.Resources.Track;
            binding = new CommandBinding(newTrackCommand, this.NewTrackExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);

            /* Create and bind the command to create a new channel message/event. */
            RoutedUICommand newChannelEventCommand = new RoutedUICommand();
            newChannelEventCommand.Text =
                Properties.Resources.Channel + Environment.NewLine + "  " + Properties.Resources.Event;
            binding = new CommandBinding(newChannelEventCommand, this.NewChannelEventExecuted, this.NewEventCanExecute);
            this.CommandBindings.Add(binding);

            /* Create and bind the command to create a new system exclusive (SysEx) message/event. */
            RoutedUICommand newSysExEventCommand = new RoutedUICommand();
            string s = Text.ParseLabel(Properties.Resources.SysEx);
            newSysExEventCommand.Text = s.Insert(s.Length - 1, "_") + " " + Properties.Resources.Event;
            binding = new CommandBinding(newSysExEventCommand, this.NewSysExEventExecuted, this.NewEventCanExecute);
            this.CommandBindings.Add(binding);

            /* Create and bind the command to create a new meta-event. */
            RoutedUICommand newMetaEventCommand = new RoutedUICommand();
            newMetaEventCommand.Text = "_" + Properties.Resources.MetaEvent;
            binding = new CommandBinding(newMetaEventCommand, this.NewMetaEventExecuted, this.NewEventCanExecute);
            this.CommandBindings.Add(binding);

            /* Build the command list and panel for the "New Items" group box. */
            commands = new List<RoutedUICommand>();
            commands.Add(newTrackCommand);
            commands.Add(newChannelEventCommand);
            commands.Add(newSysExEventCommand);
            commands.Add(newMetaEventCommand);
            this.NewItemsPanel = new CommandPanel(commands, true);

            /* Build out the "New Items" group box. */
            GroupBox newItemsGroupBox = new GroupBox();
            newItemsGroupBox.Header = Properties.Resources.NewItems;
            newItemsGroupBox.Content = this.NewItemsPanel;
            newItemsGroupBox.Margin = new Thickness(UI.UnitSpace);

            /* Bind the command to edit the properties of the selected item. */
            binding = new CommandBinding(ApplicationCommands.Properties, this.EditItemExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);

            /* Bind the command to delete the selected item. */
            binding = new CommandBinding(ApplicationCommands.Delete, this.DeleteItemExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);

            /* Build the command list and panel for the "Edit Items" group box. */
            commands = new List<RoutedUICommand>();
            commands.Add(ApplicationCommands.Properties);
            commands.Add(ApplicationCommands.Delete);
            this.EditItemsPanel = new CommandPanel(commands, true);

            /* Build out the "Edit Items" group box. */
            GroupBox editItemsGroupBox = new GroupBox();
            editItemsGroupBox.Header = Properties.Resources.EditItems;
            editItemsGroupBox.Content = this.EditItemsPanel;
            editItemsGroupBox.Margin = new Thickness(UI.UnitSpace);

            /* The duration of a MIDI file cannot be determined until it has been
             * opened for media playback (i.e., the MediaOpened event has been raised).
             */
            this.Player = new MediaPlayer();
            this.Player.MediaOpened += this.Player_MediaOpened;
            this.Player.MediaEnded += this.Player_MediaEnded;

            /* Bind the Play/Stop command (for MIDI file playback). */
            gesture = new KeyGesture(Key.F5);
            MediaCommands.Play.InputGestures.Add(gesture);
            binding = new CommandBinding(MediaCommands.Play, this.PlayStopExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);
            this.PlayStopButton = new CommandButton(MediaCommands.Play);

            /* Initialize the Starting Position control. */
            this.StartingPositionControl = new PositionControl(Properties.Resources.StartingPosition, false);
            this.StartingPositionControl.Margin = new Thickness(UI.UnitSpace);

            /* Because the Starting Position control is a dispatcher object owned by the main UI thread,
             * a background thread cannot access it.  These delegates allow a background thread to
             * highlight (and unhighlight) the starting position on the main UI thread via the dispatcher.
             */
            this.HighlightStartingPosition = new Action<Brush>(this.StartingPositionControl.Highlight);
            this.UnhighlightStartingPosition = new Action<object>(this.StartingPositionControl.Unhighlight);

            /* Create and bind the Reset command (to reset/zero starting position). */
            command = new RoutedUICommand();
            gesture = new KeyGesture(Key.F6);
            command.InputGestures.Add(gesture);
            command.Text = Properties.Resources.Reset;
            binding = new CommandBinding(command, this.ResetExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);
            CommandButton resetButton = new CommandButton(command);

            /* Create and bind the Sync command (to synchronize starting position). */
            command = new RoutedUICommand();
            gesture = new KeyGesture(Key.F7);
            command.InputGestures.Add(gesture);
            command.Text = Properties.Resources.Sync;
            binding = new CommandBinding(command, this.SyncExecuted, this.CommandCanExecute);
            this.CommandBindings.Add(binding);
            CommandButton syncButton = new CommandButton(command);

            /* Initialize the grid for the Position and Duration controls. */
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[1].Width = new GridLength(64);
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            this.PositionTextBox = MIDIopsyWindow.AddTextBoxWithLabel(grid, 0, Properties.Resources.Position);
            this.DurationTextBox = MIDIopsyWindow.AddTextBoxWithLabel(grid, 1, Properties.Resources.Duration);

            /* Build the panel for the playback controls (which will serve as the content for the "Playback" group box). */
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(this.PlayStopButton);
            stackPanel.Children.Add(this.StartingPositionControl);
            stackPanel.Children.Add(resetButton);
            stackPanel.Children.Add(syncButton);
            stackPanel.Children.Add(grid);

            /* Build out the "Playback" group box. */
            GroupBox playbackGroupBox = new GroupBox();
            playbackGroupBox.Header = Properties.Resources.Playback;
            playbackGroupBox.Content = stackPanel;
            playbackGroupBox.Margin = new Thickness(UI.UnitSpace);

            /* Build a wrap panel (for all the controls above). */
            WrapPanel wrapPanel = new WrapPanel();
            wrapPanel.Children.Add(newItemsGroupBox);
            wrapPanel.Children.Add(editItemsGroupBox);
            wrapPanel.Children.Add(playbackGroupBox);

            /* Initialize the grid/list view. */
            this.ListView = new ListView();
            this.ListView.ItemContainerStyle = new Style(typeof(ListViewItem));
            Setter setter = new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            this.ListView.ItemContainerStyle.Setters.Add(setter);
            this.GridView = new GridView();
            foreach (MidiItem.DisplayField field in MidiItem.DisplayFields) this.AddViewColumn(field);
            this.ListView.View = this.GridView;
            this.ListView.Margin = new Thickness(0, UI.UnitSpace, 0, 0);
            this.ListView.ItemContainerGenerator.StatusChanged += this.ItemContainerGenerator_StatusChanged;

            /* Build out the editing panel. */
            DockPanel.SetDock(wrapPanel, Dock.Top);
            this.EditingPanel = new DockPanel();
            this.EditingPanel.Children.Add(wrapPanel);
            this.EditingPanel.Children.Add(this.ListView);

            /* Initialize the window object. */
            Uri uri = AppHelper.CreateResourceUri(false, "MIDIopsy.ico");
            this.Icon = BitmapFrame.Create(uri);
            this.FileDialogFilter = Properties.Resources.MidiFiles;
            this.Closing += this.MIDIopsyWindow_Closing;
            this.MinWidth = 640;
            this.MinHeight = 320;

            /* Start another thread (initally blocked) to update the Position control during MIDI file playback. */
            this.EventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            ThreadPool.QueueUserWorkItem(this.UpdatePosition);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private const int DefaultTimeout = 250;
        private static readonly Thickness CellMargin = new Thickness(-6, -3, -6, -3);
        private static readonly Thickness CellPadding = new Thickness(6, 3, 6, 3);

        private CommandPanel NewItemsPanel;
        private CommandPanel EditItemsPanel;
        private MediaPlayer Player;
        private CommandButton PlayStopButton;
        private PositionControl StartingPositionControl;
        private Delegate HighlightStartingPosition;
        private Delegate UnhighlightStartingPosition;
        private TextBox PositionTextBox;
        private TextBox DurationTextBox;
        private ListView ListView;
        private GridView GridView;
        private EventWaitHandle EventHandle;
        private MidiFile MidiFile = new MidiFile();
        private bool Playing = false;
        private bool AutoPlay = false;

        #endregion

        /**************
         * Properties *
         **************/

        #region Private Properties

        /* Determine whether or not the selected MIDI item can (or should) be deleted. */
        private bool? DeletionStatus
        {
            get
            {
                /* No part of a header chunk can be deleted. */
                if (this.ListView.SelectedItem is MidiHeader) return false;

                /* If the selected MIDI item is a channel message/event that does not use
                 * running status, check for dependent events (which do use running status).
                 */
                MidiChannelEvent channelEvent = this.ListView.SelectedItem as MidiChannelEvent;
                if (channelEvent != null && !channelEvent.RunningStatus)
                {
                    /* Find the next channel event in the chunk.  If it uses
                     * running status, the selected MIDI item cannot be deleted.
                     */
                    for (int i = this.ListView.SelectedIndex + 1; i < this.ListView.Items.Count; ++i)
                    {
                        if (this.ListView.Items[i] is MidiItem.Separator) return true;
                        channelEvent = this.ListView.Items[i] as MidiChannelEvent;
                        if (channelEvent != null) return !channelEvent.RunningStatus;
                    }
                    return true;
                }

                /* Otherwise, as long as the selected MIDI item is not chunk info, it can be deleted. */
                MidiChunkInfo chunkInfo = this.ListView.SelectedItem as MidiChunkInfo;
                if (chunkInfo == null) return true;

                /* No part of a header chunk can be deleted. */
                if (chunkInfo.Type == MidiChunkInfo.HeaderType) return false;

                /* If there's only 1 track, it cannot be deleted. */
                if (chunkInfo.Type == MidiChunkInfo.TrackType && (this.ListView.Items[1] as MidiHeader).NumberOfTracks == 1)
                    return false;

                /* Otherwise, the user should be prompted/warned that the entire chunk will be deleted. */
                return null;
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

        #region Protected Methods

        /// <summary>Adjusts tab indexes as necessary to account for the header.</summary>
        protected override void AdjustTabIndexes()
        {
            if (this.FileState == FileState.None) return;
            this.NewItemsPanel.TabIndexOffset = this.HeaderControlCount;
            this.EditItemsPanel.TabIndexOffset = this.HeaderControlCount + this.NewItemsPanel.Children.Count;
        }

        /// <summary>Prompts the user for information needed to create a new MIDI file.</summary>
        /// <returns>True if a new file is to be created; otherwise, false.</returns>
        protected override bool NewFile()
        {
            /* Prompt the user for header data.  If the user cancels, take no further action. */
            MidiHeaderDialog dialog = new MidiHeaderDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return false;

            /* The user did not cancel.  Reset the file path. */
            base.NewFile();

            /* Create a new file object using the header data supplied by the user. */
            if (dialog.TicksPerQuarterNote < 0)
                this.MidiFile.Create(dialog.Format, dialog.NumberOfTracks, dialog.FramesPerSecond, dialog.TicksPerFrame);
            else this.MidiFile.Create(dialog.Format, dialog.NumberOfTracks, dialog.TicksPerQuarterNote);

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
            try { this.MidiFile.Load(this.FilePath); }
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
            /* If the file is playing, warn the user that this will stop it. */
            if (this.Playing)
            {
                MessageBoxResult result = MessageBox.Show(this, Properties.Resources.WillStopPlayback,
                    Meta.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return false;
            }

            /* Confirm that the user really wants to save, and establish the file path. */
            if (!base.SaveFile(asNew)) return false;

            /* Save the file and reload it for media playback.  (The sleep is to help ensure
             * that the media player has released the file before attempting to write to it.)
             */
            this.StopPlayback();
            this.Player.Close();
            Thread.Sleep(MIDIopsyWindow.DefaultTimeout);
            try { this.MidiFile.Save(this.FilePath); }
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
            this.StopPlayback();
            this.Player.Close();
            this.ListView.Items.Clear();
            this.MidiFile.Clear();
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        /* Create a new track (MTrk) chunk. */
        private void NewTrackExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Tracks cannot be added to a format 0 file. */
            if ((this.ListView.Items[1] as MidiHeader).Format == 0)
            {
                MessageBox.Show(this, Properties.Resources.OneTrack, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            MidiItem.Separator separator;
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
                metaEvent = this.MidiFile.InsertMetaEvent(++j, 0, MidiMetaEvent.EndOfTrackType, null);

                /* Correspondingly, insert into the list view a separator, the new track chunk, and the End of Track event. */
                separator = new MidiItem.Separator();
                this.ListView.Items.Insert(--i, separator);
                this.ListView.Items.Insert(++i, chunkInfo);
                this.ListView.Items.Insert(++i, metaEvent);

                /* Refresh the view, and mark the file as edited. */
                this.FinalizeChanges();
                return;
            }

            /* If there is no next chunk in the list view, add the new
             * track chunk and End of Track event to the end of the file.
             */
            chunkInfo = this.MidiFile.AddTrack();
            metaEvent = this.MidiFile.AddMetaEvent(0, MidiMetaEvent.EndOfTrackType, null);

            /* Correspondingly, add to the list view a separator, new track chunk, and End of Track event. */
            separator = new MidiItem.Separator();
            this.ListView.Items.Add(separator);
            this.ListView.Items.Add(chunkInfo);
            this.ListView.Items.Add(metaEvent);

            /* Refresh the view, and mark the file as edited. */
            this.FinalizeChanges();
        }

        /* Create a new MIDI channel message/event. */
        private void NewChannelEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiChannelEventDialog dialog = new MidiChannelEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            MidiChannelEvent channelEvent;
            int i = this.GetInsertionIndex();

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                channelEvent = dialog.RunningStatus ?
                    this.MidiFile.AddChannelEvent(dialog.DeltaTime, dialog.Data1, dialog.Data2) :
                    this.MidiFile.AddChannelEvent(dialog.DeltaTime, dialog.MessageType,
                        dialog.Channel, dialog.Data1, dialog.Data2);
                this.ListView.Items.Add(channelEvent);
            }
            /* Otherwise, insert the new event right after the selected item. */
            else
            {
                channelEvent = dialog.RunningStatus ?
                    this.MidiFile.InsertChannelEvent(i, dialog.DeltaTime, dialog.Data1, dialog.Data2) :
                    this.MidiFile.InsertChannelEvent(i, dialog.DeltaTime, dialog.MessageType,
                        dialog.Channel, dialog.Data1, dialog.Data2);
                this.ListView.Items.Insert(this.ListView.SelectedIndex + 1, channelEvent);
            }
            this.FinalizeChanges();
        }

        /* Create a new MIDI system exclusive (SysEx) message/event. */
        private void NewSysExEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiSysExEventDialog dialog = new MidiSysExEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            MidiSysExEvent sysExEvent;
            int i = this.GetInsertionIndex();

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                sysExEvent = this.MidiFile.AddSysExEvent(dialog.DeltaTime, dialog.Escape, dialog.Data);
                this.ListView.Items.Add(sysExEvent);
            }
            /* Otherwise, insert the new event right after the selected item. */
            else
            {
                sysExEvent = this.MidiFile.InsertSysExEvent(i, dialog.DeltaTime, dialog.Escape, dialog.Data);
                this.ListView.Items.Insert(this.ListView.SelectedIndex + 1, sysExEvent);
            }
            this.FinalizeChanges();
        }

        /* Create a new MIDI meta-event. */
        private void NewMetaEventExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for event data.  If the user cancels, take no further action. */
            MidiMetaEventDialog dialog = new MidiMetaEventDialog(null);
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Create the event based on user input. */
            MidiMetaEvent metaEvent;
            int i = this.GetInsertionIndex();

            /* If the selected item is last in the list view, add the new event to the end. */
            if (i < 0)
            {
                metaEvent = this.MidiFile.AddMetaEvent(dialog.DeltaTime, dialog.Type, dialog.Data);
                this.ListView.Items.Add(metaEvent);
            }
            /* Otherwise, insert the new event right after the selected item. */
            else
            {
                metaEvent = this.MidiFile.InsertMetaEvent(i, dialog.DeltaTime, dialog.Type, dialog.Data);
                this.ListView.Items.Insert(this.ListView.SelectedIndex + 1, metaEvent);
            }
            this.FinalizeChanges();
        }

        /* Edit [the properties of] the selected MIDI item. */
        private void EditItemExecuted(object sender, ExecutedRoutedEventArgs e)
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

            /* If the dialog has not been initialized by now, it means the selected MIDI item is not editable. */
            if (dialog == null)
            {
                MessageBox.Show(this, Properties.Resources.CannotEdit, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

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
                    {
                        channelEvent.MessageType = channelEventDialog.MessageType;
                        channelEvent.Channel = channelEventDialog.Channel;
                    }
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
            this.FinalizeChanges();
        }

        /* Delete the selected MIDI item. */
        private void DeleteItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Determine if the selected MIDI item can/should be deleted. */
            bool? b = this.DeletionStatus;
            if (b == false)
            {
                MessageBox.Show(this, Properties.Resources.CannotDelete, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (b == null)
            {
                MessageBoxResult result = MessageBox.Show(this, Properties.Resources.WillDeleteChunk,
                    Meta.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            /* We have clearance.  Delete the selected item. */
            int i = this.ListView.SelectedIndex;
            this.MidiFile.DeleteItem(i - this.SeparatorCount);
            this.ListView.Items.RemoveAt(i);

            /* If appropriate, remove (from the list view) all items in the chunk. */
            if (b == null)
            {
                while (i < this.ListView.Items.Count && !(this.ListView.Items[i] is MidiItem.Separator))
                    this.ListView.Items.RemoveAt(i);
                this.ListView.Items.RemoveAt(i - 1);
            }

            /* With the selected item deleted, there is now no selection.
             * For convenience, select the nearest non-separator item.
             */
            while (i < this.ListView.Items.Count && this.ListView.Items[i++] is MidiItem.Separator) ;
            if (i > this.ListView.Items.Count) i = this.ListView.Items.Count;
            while (this.ListView.Items[--i] is MidiItem.Separator) ;
            this.ListView.SelectedIndex = i;
            this.FinalizeChanges();
        }

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

        /* Most commands can execute as long as there is a file open. */
        protected void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState != FileState.None); }

        /* Commands that create new events can execute as long as there is a file open and
         * the selected item is neither of the first two (presumably, the header chunk).
         */
        protected void NewEventCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState != FileState.None && this.ListView.SelectedIndex > 1); }

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

        /* If we're really closing, signal the other thread so that it will exit. */
        private void MIDIopsyWindow_Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel) return;
            this.EventHandle.Set();
        }

        #endregion

        private static TextBox AddTextBoxWithLabel(Grid grid, int row, object content)
        {
            /* Create the text box. */
            TextBox textBox = new TextBox();
            textBox.IsUndoEnabled = false;
            textBox.IsReadOnly = true;
            textBox.TextAlignment = TextAlignment.Right;
            textBox.GotFocus += UI.TextBox_GotFocus;
            textBox.Margin = new Thickness(0, UI.HalfSpace, 0, UI.HalfSpace);
            Grid.SetRow(textBox, row);
            Grid.SetColumn(textBox, 1);

            /* Create the label. */
            StandardLabel label = new StandardLabel(content, false);
            label.Target = textBox;
            Grid.SetRow(label, row);

            /* Add the new controls. */
            grid.Children.Add(label);
            grid.Children.Add(textBox);
            return textBox;
        }

        private void AddViewColumn(MidiItem.DisplayField field)
        {
            /* Create the column and set its header content. */
            GridViewColumn column = new GridViewColumn();
            column.Header = Text.ParseLabel(field.LabelText);

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
            else factory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            column.CellTemplate.VisualTree.AppendChild(factory);

            /* Add the column to the grid/list view. */
            this.GridView.Columns.Add(column);
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
                if (!this.Playing) { Logger.WriteMessage("thread exiting"); return; }

                /* Because the media player and the Position control are dispatcher objects owned by the main UI thread,
                 * this thread cannot access them, so the position must be set on the main UI thread via the dispatcher.
                 */
                this.Dispatcher.Invoke(setPosition, null as object);

                /* Wait a second, then repeat. */
                Thread.Sleep(1000);
            }
        }

        /* Perform all actions necessary to load the MIDI file object into the UI. */
        private void LoadFile()
        {
            int i;
            MidiItem item;
            MidiItem.Separator separator;

            this.InitializePlayback(true);

            /* Populate the list view with the contents of the MIDI file. */
            for (i = 0; i < this.MidiFile.ItemCount; ++i)
            {
                item = this.MidiFile.GetItem(i);

                /* Add a separator before each chunk (after the first). */
                if (i > 0 && item is MidiChunkInfo)
                {
                    separator = new MidiItem.Separator();
                    this.ListView.Items.Add(separator);
                }

                this.ListView.Items.Add(item);
            }
            this.ListView.SelectedIndex = 0;
            this.ListView.ScrollIntoView(this.ListView.SelectedItem);

            /* Reset the width of each column of the grid/list view. */
            double d = this.ActualWidth - 50;
            for (i = 0; i < MidiItem.DisplayFields.Length; ++i)
                this.GridView.Columns[i].Width = MidiItem.DisplayFields[i].WidthFactor * d;
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

        /* Return the index in the MIDI file's list at which a new event should be inserted (based
         * on the selected item), or -1 if the event should be added to the end of the file.
         */
        private int GetInsertionIndex()
        {
            /* If the selected item is last in the list view, the new event should be added to the end of the file. */
            if (this.ListView.SelectedIndex == this.ListView.Items.Count - 1) return -1;

            /* Otherwise, the new event should be inserted into the file (right after the
             * selected item).  The index in the MIDI file's list should be the same as
             * the index in the list view, minus the number of separators that precede it.
             */
            return this.ListView.SelectedIndex - this.SeparatorCount + 1;
        }

        /* This method should be called after any changes to the MIDI file. */
        private void FinalizeChanges()
        {
            /* Refresh the view so that the changes show up. */
            this.ListView.Items.Refresh();

            /* Mark the file as edited. */
            this.FileState = FileState.Edited;
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
            this.Playing = true;
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
