/* SettingsDialog.cs - Implementation of SettingsDialog class, which allows the user to view and update configuration settings.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* OpenFileDialog */
using Microsoft.Win32;

/* Exception */
using System;

/* StreamWriter */
using System.IO;

/* ResizeMode, RoutedEventArgs, SizeToContent, Thickness, Window, WindowStyle */
using System.Windows;

/* Button, CheckBox, Dock, DockPanel, GroupBox, StackPanel, TextBox */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that allows the user to view and update configuration settings.</summary>
    public class SettingsDialog : StandardDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the SettingsDialog class.</summary>
        public SettingsDialog()
        {
            int i = 0;

            /* Initialize the "Write messages to log file" check box. */
            this.LogCheckBox = new CheckBox();
            this.LogCheckBox.TabIndex = ++i;
            this.LogCheckBox.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.UnitSpace);
            this.LogCheckBox.Content = Common.Resources.WriteToLog;
            this.LogCheckBox.IsChecked = Common.Settings.Default.Log;

            /* Initialize the "Log file path" label. */
            this.LogPathLabel = new StandardLabel(Common.Resources.LogFilePath, true);
            this.LogPathLabel.TabIndex = ++i;
            this.LogPathLabel.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);

            /* Initialize the log path text box. */
            this.LogPathTextBox = new TextBox();
            this.LogPathTextBox.TabIndex = ++i;
            this.LogPathTextBox.Margin = new Thickness(0, 0, UI.HalfSpace, 0);
            this.LogPathTextBox.Text = Common.Settings.Default.LogPath;
            this.LogPathTextBox.LostFocus += this.LogPathTextBox_LostFocus;
            this.LogPathLabel.Target = this.LogPathTextBox;

            /* Initialize the browse button. */
            Button button = new Button();
            button.TabIndex = ++i;
            button.Margin = new Thickness(UI.HalfSpace, 0, 0, 0);
            button.Content = "...";
            button.Width = 20;
            button.Click += this.BrowseButton_Click;

            /* Build out the browse (dock) panel, which contains the file/
             * path and browse controls, and add it to the stack panel.
             */
            DockPanel.SetDock(button, Dock.Right);
            this.BrowsePanel = new DockPanel();
            this.BrowsePanel.Children.Add(button);
            this.BrowsePanel.Children.Add(this.LogPathTextBox);
            this.BrowsePanel.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);

            /* Initialize the "All" check box. */
            this.AllCheckBox = new CheckBox();
            this.AllCheckBox.TabIndex = ++i;
            this.AllCheckBox.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.UnitSpace);
            this.AllCheckBox.Content = Common.Resources.All;

            /* Initialize the "Timestamps" check box. */
            this.TimestampsCheckBox = new CheckBox();
            this.TimestampsCheckBox.TabIndex = ++i;
            this.TimestampsCheckBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
            this.TimestampsCheckBox.Content = Common.Resources.Timestamps;
            this.TimestampsCheckBox.IsChecked = Common.Settings.Default.LogTimestamp;

            /* Initialize the "Procedure names" check box. */
            this.ProcedureNamesCheckBox = new CheckBox();
            this.ProcedureNamesCheckBox.TabIndex = ++i;
            this.ProcedureNamesCheckBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
            this.ProcedureNamesCheckBox.Content = Common.Resources.ProcedureNames;
            this.ProcedureNamesCheckBox.IsChecked = Common.Settings.Default.LogProcedureName;

            /* Initialize the "Indents" check box. */
            this.IndentsCheckBox = new CheckBox();
            this.IndentsCheckBox.TabIndex = ++i;
            this.IndentsCheckBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
            this.IndentsCheckBox.Content = Common.Resources.Indents;
            this.IndentsCheckBox.IsChecked = Common.Settings.Default.LogIndent;

            /* Initialize the "Exception detail" check box. */
            this.ExceptionDetailCheckBox = new CheckBox();
            this.ExceptionDetailCheckBox.TabIndex = ++i;
            this.ExceptionDetailCheckBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
            this.ExceptionDetailCheckBox.Content = Common.Resources.ExceptionDetail;
            this.ExceptionDetailCheckBox.IsChecked = Common.Settings.Default.LogExceptionDetail;

            /* Build out the options stack panel, which contains the
             * option check boxes and will serve as the group box content.
             */
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(this.AllCheckBox);
            stackPanel.Children.Add(this.TimestampsCheckBox);
            stackPanel.Children.Add(this.ProcedureNamesCheckBox);
            stackPanel.Children.Add(this.IndentsCheckBox);
            stackPanel.Children.Add(this.ExceptionDetailCheckBox);

            /* Build out the group box, which contains the options stack panel. */
            this.OptionsGroupBox = new GroupBox();
            this.OptionsGroupBox.Header = Common.Resources.LoggingOptions;
            this.OptionsGroupBox.Content = stackPanel;
            this.OptionsGroupBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);

            /* Build out the window and its content. */
            this.AddUIElement(this.LogCheckBox);
            this.AddUIElement(this.LogPathLabel);
            this.AddUIElement(this.BrowsePanel);
            this.AddUIElement(this.OptionsGroupBox);
            this.BuildOut(UI.ClientWidth, Common.Resources.Settings);

            /* Since the Checked handlers refer to the other controls, they are not assigned (and should not be
             * called) until all controls have been instantiated (in order to avoid null reference exceptions).
             */
            this.LogCheckBox.Checked += this.LogCheckBox_Checked;
            this.LogCheckBox.Unchecked += this.LogCheckBox_Checked;
            this.AllCheckBox.Checked += this.AllCheckBox_Checked;
            this.AllCheckBox.Unchecked += this.AllCheckBox_Checked;
            this.TimestampsCheckBox.Checked += this.OptionCheckBox_Checked;
            this.TimestampsCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.ProcedureNamesCheckBox.Checked += this.OptionCheckBox_Checked;
            this.ProcedureNamesCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.IndentsCheckBox.Checked += this.OptionCheckBox_Checked;
            this.IndentsCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.ExceptionDetailCheckBox.Checked += this.OptionCheckBox_Checked;
            this.ExceptionDetailCheckBox.Unchecked += this.OptionCheckBox_Checked;

            /* Now that it's safe, call the Checked handler for the "Write
             * messages to log file" and option check boxes to set visual cues.
             */
            this.LogCheckBox_Checked(null, null);
            this.OptionCheckBox_Checked(null, null);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private CheckBox LogCheckBox;
        private StandardLabel LogPathLabel;
        private TextBox LogPathTextBox;
        private DockPanel BrowsePanel;
        private CheckBox AllCheckBox;
        private CheckBox TimestampsCheckBox;
        private CheckBox ProcedureNamesCheckBox;
        private CheckBox IndentsCheckBox;
        private CheckBox ExceptionDetailCheckBox;
        private GroupBox OptionsGroupBox;
        private bool ItemCheckInProgress;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets whether or not to write messages to the log file.</summary>
        public bool Log { get { return (bool)this.LogCheckBox.IsChecked; } }

        /// <summary>Gets a string containing the full path of the log file.</summary>
        public string LogPath { get { return this.LogPathTextBox.Text; } }

        /// <summary>Gets whether or not to write timestamps to the log file.</summary>
        public bool LogTimestamps { get { return (bool)this.TimestampsCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to write procedure names to the log file.</summary>
        public bool LogProcedureNames { get { return (bool)this.ProcedureNamesCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to indent messages (based on call stack) when writing to the log file.</summary>
        public bool LogIndents { get { return (bool)this.IndentsCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to write exception detail to the log file.</summary>
        public bool LogExceptionDetail { get { return (bool)this.ExceptionDetailCheckBox.IsChecked; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        #region Event Handlers

        private void LogCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.ValidateLogFile();

            /* Enable or disable other controls based on whether or not the "Write messages to log file" box is checked. */
            bool b = this.Log;
            this.LogPathLabel.IsEnabled = b;
            this.BrowsePanel.IsEnabled = b;
            this.OptionsGroupBox.IsEnabled = b;
        }

        private void LogPathTextBox_LostFocus(object sender, RoutedEventArgs e) { this.ValidateLogFile(); }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            /* Prompt the user for a file whose path to copy into the log path text box. */
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = Common.Resources.LogFiles;
            bool? result = dialog.ShowDialog(this);
            if (result != true) return;
            this.LogPathTextBox.Text = dialog.FileName;
            this.ValidateLogFile();
        }

        private void AllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            /* This prevents endless loops when boxes are checked programmatically. */
            if (this.ItemCheckInProgress) return;
            this.ItemCheckInProgress = true;

            /* Check or uncheck all options. */
            bool b = (bool)this.AllCheckBox.IsChecked;
            this.TimestampsCheckBox.IsChecked = b;
            this.ProcedureNamesCheckBox.IsChecked = b;
            this.IndentsCheckBox.IsChecked = b;
            this.ExceptionDetailCheckBox.IsChecked = b;
            this.ItemCheckInProgress = false;
        }

        private void OptionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            /* This prevents endless loops when boxes are checked programmatically. */
            if (this.ItemCheckInProgress) return;
            this.ItemCheckInProgress = true;

            /* If all options are checked, check the "All" box; otherwise, uncheck it. */
            this.AllCheckBox.IsChecked = this.AreAllOptionsChecked();
            this.ItemCheckInProgress = false;
        }

        #endregion

        /// <summary>Determines whether or not all logging options are checked.</summary>
        /// <returns>True if all logging options are checked; otherwise, false.</returns>
        private bool AreAllOptionsChecked()
        {
            /* If any one of the options is not checked, return false. */
            if (this.TimestampsCheckBox.IsChecked != true) return false;
            if (this.ProcedureNamesCheckBox.IsChecked != true) return false;
            if (this.IndentsCheckBox.IsChecked != true) return false;
            if (this.ExceptionDetailCheckBox.IsChecked != true) return false;

            /* We made it all the way through, so they must all be checked. */
            return true;
        }

        /* If applicable, test the log file to make sure it can be written to. */
        private void ValidateLogFile()
        {
            if (!this.Log || string.IsNullOrEmpty(this.LogPath)) return;
            try { using (StreamWriter writer = new StreamWriter(this.LogPath, true)) { } }
            catch (Exception ex)
            {
                string s = Text.FormatErrorMessage(Common.Resources.LogNotOpen, ex);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion
    }
}
