/* FileAppWindow.cs - Implementation of FileAppWindow class, which extends the class by adds file functionality.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* OpenFileDialog, SaveFileDialog, FileDialog */
using Microsoft.Win32;

/* Math */
using System;

/* List */
using System.Collections.Generic;

/* StringCollection */
using System.Collections.Specialized;

/* CancelEventArgs */
using System.ComponentModel;

/* Path */
using System.IO;

/* RoutedEventArgs, MessageBoxResult, MessageBox, Thickness, HorizontalAlignment */
using System.Windows;

/* StackPanel, Grid, ColumnDefinition, Panel, Button */
using System.Windows.Controls;

/* CommandBinding, RoutedUICommand, KeyGesture, ApplicationCommands, Key, ExecutedRoutedEventArgs, CanExecuteRoutedEventArgs */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>Specifies enumerated constants to define the state of a file.</summary>
    public enum FileState { None, New, Edited, OpenedOrSaved }

    /// <summary>Extends the AppWindow class by adding file functionality.</summary>
    public abstract class FileAppWindow : AppWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a FileAppWindow object.</summary>
        public FileAppWindow()
        {
            int i, n;
            CommandBinding binding;
            List<RoutedUICommand> commands;
            KeyGesture gesture;

            /* Bind the "New" command. */
            binding = new CommandBinding(ApplicationCommands.New, this.NewExecuted, this.OpeningCommandCanExecute);
            this.CommandBindings.Add(binding);

            /* Bind the "Open" command. */
            binding = new CommandBinding(ApplicationCommands.Open, this.OpenExecuted, this.OpeningCommandCanExecute);
            this.CommandBindings.Add(binding);

            /* Build the opening panel (which goes with the MRU panel, to be built shortly). */
            commands = new List<RoutedUICommand>();
            commands.Add(ApplicationCommands.New);
            commands.Add(ApplicationCommands.Open);
            this.OpeningPanel = new CommandPanel(commands, true);

            /* Start building the Most Recently Used (MRU) panel, and initialize the MRU list. */
            this.MruPanel = new StackPanel();
            this.MruArray = new string[FileAppWindow.MruMax];
            this.MruTabIndexOffset = this.OpeningPanel.Children.Count + this.HeaderControlCount;
            if (Common.Settings.Default.Mru != null && Common.Settings.Default.Mru.Count > 0)
            {
                /* Copy the number of MRU file paths into the MRU array, up to the maximum MRU count. */
                n = Math.Min(Common.Settings.Default.Mru.Count, FileAppWindow.MruMax);
                for (i = 0; i < n; ++i) this.MruArray[i] = Common.Settings.Default.Mru[i];

                /* This finishes building the MRU panel. */
                this.RebuildMruButtons();
            }

            /* Bind the "Save" command. */
            binding = new CommandBinding(ApplicationCommands.Save, this.SaveExecuted, this.SaveCanExecute);
            this.CommandBindings.Add(binding);

            /* Bind the "Save As" command. */
            gesture = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);
            ApplicationCommands.SaveAs.InputGestures.Add(gesture);
            binding = new CommandBinding(ApplicationCommands.SaveAs, this.SaveAsExecuted, this.SaveAsCanExecute);
            this.CommandBindings.Add(binding);

            /* Create and bind the "Done" command. */
            RoutedUICommand command = new RoutedUICommand();
            gesture = new KeyGesture(Key.Escape);
            command.InputGestures.Add(gesture);
            command.Text = Common.Resources.Done;
            binding = new CommandBinding(command, this.DoneExecuted, this.DoneCanExecute);
            this.CommandBindings.Add(binding);

            /* Build the saving panel (which goes with the editing panel, to be built by derived classes). */
            commands = new List<RoutedUICommand>();
            commands.Add(ApplicationCommands.Save);
            commands.Add(ApplicationCommands.SaveAs);
            commands.Add(command);
            this.SavingPanel = new CommandPanel(commands, true);

            /* Set the initial file state (which also shows the appopriate panels) and assign the closing event handler. */
            this.FileState = FileState.None;
            this.Closing += this.FileAppWindow_Closing;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected Panel EditingPanel;
        protected string FileDialogFilter;

        #endregion

        #region Private Fields

        private const int MruMax = 5;

        private FileState _FileState;
        private string _FilePath;

        private CommandPanel OpeningPanel;
        private CommandPanel SavingPanel;
        private StackPanel MruPanel;
        private string[] MruArray;
        private int MruTabIndexOffset;

        #endregion

        /**************
         * Properties *
         **************/

        #region Protected Properties

        /// <summary>
        /// Gets or sets the file state, which specifies whether a file has just been opened or saved, edited, created
        /// (new), or none of the above (the application has just been launched, or a file has just been closed).
        /// </summary>
        protected FileState FileState
        {
            get { return this._FileState; }

            set
            {
                /* Set the file state and update the window title. */
                this._FileState = value;
                this.SetTitle();

                /* If the file state is changing to Edited, no further action is necessary. */
                if (value == FileState.Edited) return;

                /* If the application has just been launched or a file closed, show the opening and MRU panels. */
                if (value == FileState.None)
                {
                    this.SetHeaderSubpanel(this.OpeningPanel);
                    this.SetMainSubpanel(this.MruPanel);
                    return;
                }

                /* For all other file states (opened/saved or new), show the saving and editing panels. */
                this.SetHeaderSubpanel(this.SavingPanel);
                this.SetMainSubpanel(this.EditingPanel);
            }
        }

        /// <summary>Gets or sets the path of the currently open file.</summary>
        protected string FilePath
        {
            get { return this._FilePath; }

            set
            {
                /* Set the file path and update the window title. */
                this._FilePath = value;
                this.SetTitle();

                /* If there's no file path, no further action is necessary. */
                if (string.IsNullOrEmpty(value)) return;

                /* Update the Most Recently Used (MRU) list. */
                this.UpdateMruItems();
            }
        }

        /// <summary>Gets the Most Recently Used (MRU) file list.</summary>
        protected StringCollection MruCollection
        {
            get
            {
                /* Add each MRU configuration setting. */
                StringCollection collection = new StringCollection();
                for (int i = 0; i < FileAppWindow.MruMax; ++i)
                {
                    if (string.IsNullOrEmpty(this.MruArray[i])) continue;
                    collection.Add(this.MruArray[i]);
                }
                return collection;
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>
        /// Resets the file path (to null).  When overridden in a derived
        /// class, performs additional actions as needed to create a new file.
        /// </summary>
        /// <returns>True if a new file is to be created; otherwise, false.</returns>
        protected virtual bool NewFile()
        {
            this.FilePath = null;
            return true;
        }

        /// <summary>
        /// Sets the file path or prompts the user for one.  When overridden in a
        /// derived class, performs additional actions as needed to open a file.
        /// </summary>
        /// <param name="filePath">The path of the file being opened, or null/empty to prompt the user.</param>
        /// <returns>True if the file is to be opened; otherwise, false.</returns>
        protected virtual bool OpenFile(string filePath)
        {
            /* If no file path is supplied, prompt the user. */
            if (string.IsNullOrEmpty(filePath))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                return this.OpenOrSaveFile(dialog);
            }

            /* A file path is supplied, so set it. */
            this.FilePath = filePath;
            return true;
        }

        /// <summary>
        /// If necessary, prompts the user for a file path to save.  When overridden
        /// in a derived class, performs additional actions as needed to save a file.
        /// </summary>
        /// <param name="asNew">
        /// True if the file is to be saved "as new" (i.e., it has not
        /// already been saved at the current path); otherwise, false.
        /// </param>
        /// <returns>True if the file is to be saved; otherwise, false.</returns>
        protected virtual bool SaveFile(bool asNew)
        {
            /* If we're not saving as new and we already have a file path, then we're good. */
            if (!asNew && !string.IsNullOrEmpty(this.FilePath)) return true;

            /* We're saving as new and/or we do not have a file path, so prompt the user for a file path. */
            SaveFileDialog dialog = new SaveFileDialog();
            return this.OpenOrSaveFile(dialog);
        }

        /// <summary>
        /// Prompts the user to save changes, resetting the file path if appropriate.  When
        /// overridden in a derived class, performs additional actions as needed to close a file.
        /// </summary>
        /// <returns>True if the file is to be closed; otherwise, false.</returns>
        protected virtual bool CloseFile()
        {
            /* Prompt the user to save changes.  If the user cancels, never mind. */
            if (!this.SaveChanges()) return false;

            /* The user did not cancel (they either saved or discarded their changes), so reset the file path. */
            this.FilePath = null;
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Reset file path and file state accordingly. */
            if (!this.NewFile()) return;
            this.FileState = FileState.New;
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for the file path to open.  If the user does not
             * click OK, no further action is necessary, as no file is to be opened.
             */
            if (!this.OpenFile(null)) return;

            /* The user clicked OK, so a file is to be opened.  Set the file state accordingly. */
            this.FileState = FileState.OpenedOrSaved;
        }

        /* An opening command can execute as long as there is no file open. */
        private void OpeningCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState == FileState.None); }

        private void MruButton_Click(object sender, RoutedEventArgs e)
        {
            /* This event handler was called by an MRU button, so open the file specified by the button's content. */
            string s = (sender as Button).Content.ToString().Substring(1, 1);
            s = this.MruArray[int.Parse(s) - 1];
            if (!this.OpenFile(s)) return;
            this.FileState = FileState.OpenedOrSaved;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Save the file at the current path (if there is one; otherwise, the user will be prompted).
             * If the user does not click OK, no further action is necessary, as the file is not to be saved.
             */
            if (!this.SaveFile(false)) return;

            /* The user clicked OK, so the file is to be saved.  Set the file state accordingly. */
            this.FileState = FileState.OpenedOrSaved;
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState == FileState.Edited); }

        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user for a new path at which to save the file.  If the user does
             * not click OK, no further action is necessary, as the file is not to be saved.
             */
            if (!this.SaveFile(true)) return;

            /* The user clicked OK, so the file is to be saved.  Set the file state accordingly. */
            this.FileState = FileState.OpenedOrSaved;
        }

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState == FileState.OpenedOrSaved) || (this.FileState == FileState.Edited); }

        private void DoneExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, no
             * further action is necessary, as the file is to be closed.
             */
            if (!this.CloseFile()) return;

            /* The user did not cancel (they either saved or discarded their changes),
             * so the file is to be closed.  Reset the file state accordingly.
             */
            this.FileState = FileState.None;
        }

        private void DoneCanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = (this.FileState != FileState.None); }

        private void FileAppWindow_Closing(object sender, CancelEventArgs e)
        {
            /* Prompt the user to save changes.  If the user cancels, do not close the window. */
            if (e.Cancel = !this.CloseFile()) return;

            /* The user did not cancel (they either saved or discarded
             * their changes), so save their settings and close the window.
             */
            Common.Settings.Default.Mru = this.MruCollection;
            Common.Settings.Default.Save();
        }

        #endregion

        /* Set the window title appropriately based on the file path/name and the file state. */
        private void SetTitle()
        {
            this.Title = string.Format("{0}{1}{2}{3}", Path.GetFileName(this.FilePath),
                (this.FileState == FileState.Edited) ? "*" : string.Empty,
                (this.FileState == FileState.None) ? string.Empty : " - ", AppHelper.Title);
        }

        private void UpdateMruItems()
        {
            int i, j = -1;

            /* The idea here is to remove an item from the array, move everything else down, and put the file path
             * on top.  If the file path is already in the array, remove it.  Otherwise, remove whatever's on bottom.
             */
            for (i = 0; i < FileAppWindow.MruMax; ++i)
            {
                if (this.MruArray[i] == this.FilePath)
                {
                    j = i;

                    /* If the file path is already on top, we're done. */
                    if (j == 0) return;
                }

                /* If we've reached a null/empty, there are no more. */
                if (string.IsNullOrEmpty(this.MruArray[i])) break;

                /* Remove the button. */
                this.MruPanel.Children.RemoveAt(1);
            }

            /* If we got all the way through and didn't find the file path, remove whatever's on bottom. */
            if (j < 0) { if (i == FileAppWindow.MruMax) j = i - 1; else j = i; }

            /* Remove the chosen item from the array and move everything else down. */
            for (i = j; i > 0; --i) this.MruArray[i] = this.MruArray[i - 1];

            /* Put the file path on top, rebuild the buttons, and we're done. */
            this.MruArray[0] = this.FilePath;
            this.RebuildMruButtons();
        }

        private void RebuildMruButtons()
        {
            /* If necessary, add "Open Recent" label to the MRU panel. */
            if (this.MruPanel.Children.Count < 1)
            {
                StandardLabel label = new StandardLabel(Common.Resources.OpenRecent, false);
                this.MruPanel.Children.Add(label);
            }

            /* Iterate through each item in the Most Recently Used (MRU) array, up to the max. */
            for (int i = 0; i < FileAppWindow.MruMax; ++i)
            {
                /* If we've reached a null/empty, there are no more. */
                if (string.IsNullOrEmpty(this.MruArray[i])) break;

                /* Build and add a new button for this file path. */
                Button button = new Button();
                button.Content = string.Format("_{0} {1}", i + 1, this.MruArray[i]);
                button.Margin = new Thickness(UI.UnitSpace, 0, UI.UnitSpace, 0);
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Click += this.MruButton_Click;
                button.TabIndex = this.MruTabIndexOffset + i;
                this.MruPanel.Children.Add(button);
            }
        }

        private bool OpenOrSaveFile(FileDialog dialog)
        {
            /* Prompt the user for the file path.  If the user does not click OK, return false. */
            dialog.Filter = this.FileDialogFilter;
            bool? result = dialog.ShowDialog(this);
            if (result != true) return false;

            /* The user clicked OK.  Now that we have a file path, set properties appropriately. */
            this.FilePath = dialog.FileName;
            return true;
        }

        private bool SaveChanges()
        {
            /* The user needs to be prompted to save changes only if the file state is Edited. */
            if (this.FileState != FileState.Edited) return true;

            /* The file state is Edited, so prompt the user to save changes.  If the user clicks
             * Yes, save the file at the current path (if there is one; otherwise, the user will
             * be prompted).  In any event, return true to indicate that the user did not cancel.
             */
            string s = string.Format(Common.Resources.SaveChangesFormat, this.FilePath);
            MessageBoxResult result = MessageBox.Show(this, s, AppHelper.Title, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.No) return true;
            if (result == MessageBoxResult.Yes && this.SaveFile(false)) return true;

            /* The user canceled, so return false. */
            return false;
        }

        #endregion
    }
}
