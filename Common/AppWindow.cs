/* AppWindow.cs - Implementation of AppWindow class, which makes up an application's user interface.
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


/* EventArgs */
using System;

/* List */
using System.Collections.Generic;

/* CancelEventArgs */
using System.ComponentModel;

/* Button, Canvas, Dock, DockPanel, Panel */
using System.Windows.Controls;

/* ApplicationCommands, CanExecuteRoutedEventArgs, CanExecuteRoutedEventHandler, CommandBinding,
 * ExecutedRoutedEventArgs, ExecutedRoutedEventHandler, Key, KeyGesture, ModifierKeys, RoutedUICommand
 */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>
    /// Represents a window that makes up an application's user interface with a resizable border and Help button.
    /// </summary>
    public class AppWindow : StandardWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes an AppWindow object.</summary>
        public AppWindow()
        {
            /* Create/bind the commands for the header panel. */
            List<RoutedUICommand> commands = new List<RoutedUICommand>();
            this.CreateCommand("_" + Common.Resources.Settings, Key.None,
                ModifierKeys.None, this.SettingsExecuted, this.CanAlwaysExecute, commands);
            this.BindCommand(ApplicationCommands.Help, Key.None, ModifierKeys.None,
                this.HelpExecuted, this.CanAlwaysExecute, commands);
            this.CreateCommand("_" + Common.Resources.About, Key.None,
                ModifierKeys.None, this.AboutExecuted, this.CanAlwaysExecute, commands);
            this.HeaderCommandPanel = new CommandPanel(commands, true);

            /* Build the header panel. */
            DockPanel.SetDock(this.HeaderCommandPanel, Dock.Right);
            this.HeaderPanel = new DockPanel();
            this.HeaderPanel.Children.Add(this.HeaderCommandPanel);
            this.HeaderPanel.Children.Add(new Canvas());
            this._HeaderControlCount = this.HeaderCommandPanel.Children.Count;

            /* Build the main panel. */
            DockPanel.SetDock(this.HeaderPanel, Dock.Top);
            this.MainPanel = new DockPanel();
            this.MainPanel.Children.Add(this.HeaderPanel);
            this.MainPanel.Children.Add(new Canvas());

            /* Set title, content, startup location/state, and Closing event handler. */
            this.Title = AppHelper.Title;
            this.Content = this.MainPanel;
            this.RestorePreferences(Common.Settings.Default.AppWindowMaximized, Common.Settings.Default.AppWindowBounds);
            this.Closing += this.AppWindow_Closing;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private DockPanel MainPanel;
        private DockPanel HeaderPanel;
        private CommandPanel HeaderCommandPanel;
        private int _HeaderControlCount;

        #endregion

        /**************
         * Properties *
         **************/

        #region Protected Properties

        /// <summary>
        /// Gets the number of controls belonging to the header panel, including
        /// buttons on the command panel and children of the header subpanel.
        /// </summary>
        protected int HeaderControlCount { get { return this._HeaderControlCount; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>
        /// Creates a new routed UI command with an optional keyboard shortcut (i.e., a key gesture with
        /// the specified key and modifier keys), associates (binds) the command to the specified event
        /// handlers, adds the command binding to the UI, and optionally adds the command to a list.
        /// </summary>
        /// <param name="text">The text that describes the command.</param>
        /// <param name="key">The key associated with the gesture.</param>
        /// <param name="modifiers">The modifier keys associated with the gesture.</param>
        /// <param name="executed">The handler for the Executed event on the command.</param>
        /// <param name="canExecute">The handler for the CanExecute event on the command.</param>
        /// <param name="list">If non-null, a list to which the command is added.</param>
        /// <returns>The RoutedUICommand object.</returns>
        protected RoutedUICommand CreateCommand(string text, Key key, ModifierKeys modifiers,
            ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, List<RoutedUICommand> list)
        {
            /* Create the command and set its text property. */
            RoutedUICommand command = new RoutedUICommand();
            command.Text = text;

            /* Bind and return the command. */
            this.BindCommand(command, key, modifiers, executed, canExecute, list);
            return command;
        }

        /// <summary>
        /// Associates a routed UI command with an optional keyboard shortcut (i.e., a key
        /// gesture with the specified key and modifier keys) to the specified event handlers,
        /// adds the command binding to the UI, and optionally adds the command to a list.
        /// </summary>
        /// <param name="command">A RoutedUICommand object.</param>
        /// <param name="key">The key associated with the gesture.</param>
        /// <param name="modifiers">The modifier keys associated with the gesture.</param>
        /// <param name="executed">The handler for the Executed event on the command.</param>
        /// <param name="canExecute">The handler for the CanExecute event on the command.</param>
        /// <param name="list">If non-null, a list to which the command is added.</param>
        protected void BindCommand(RoutedUICommand command, Key key, ModifierKeys modifiers,
            ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, List<RoutedUICommand> list)
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

            /* If specified, add the command to a list. */
            if (list != null) list.Add(command);
        }

        /// <summary>Replaces the main subpanel with a specified panel.</summary>
        /// <param name="panel">The panel with which to replace the main subpanel.</param>
        protected void SetMainSubpanel(Panel panel)
        {
            this.MainPanel.Children.RemoveAt(1);
            this.MainPanel.Children.Add(panel);
            this.AdjustTabIndexes();
        }

        /// <summary>Removes the header subpanel or replaces it with a specified panel.</summary>
        /// <param name="panel">The panel with which to replace the header subpanel, or null to remove it.</param>
        protected void SetHeaderSubpanel(Panel panel)
        {
            /* Remove any existing header subpanel. */
            if (this.HeaderPanel.Children.Count > 2) this.HeaderPanel.Children.RemoveAt(1);

            /* Set the tab index offset of the header command panel.  This keeps all controls on the header
             * panel in the right order.  If a replacement panel was passed, insert it into the header panel.
             */
            if (panel == null) this.HeaderCommandPanel.TabIndexOffset = 0;
            else
            {
                DockPanel.SetDock(panel, Dock.Left);
                this.HeaderPanel.Children.Insert(1, panel);
                this.HeaderCommandPanel.TabIndexOffset = panel.Children.Count;
            }

            /* Update the header control count.  Derived classes will need
             * this in order to keep their tab indexes in the right order.
             */
            this._HeaderControlCount = this.HeaderCommandPanel.TabIndexOffset + this.HeaderCommandPanel.Children.Count;
        }

        /// <summary>
        /// When overridden in a derived class, adjusts the tab indexes of its controls as necessary to account for the header.
        /// </summary>
        protected virtual void AdjustTabIndexes() { }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void SettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            /* Show the settings dialog.  If the user cancels, take no further action. */
            SettingsDialog dialog = new SettingsDialog();
            bool? result = dialog.ShowDialog(this);
            if (result == false) return;

            /* The user did not cancel.  Save settings. */
            Common.Settings.Default.Log = dialog.Log;
            Common.Settings.Default.LogPath = dialog.LogPath;
            Common.Settings.Default.LogTimestamp = dialog.LogTimestamps;
            Common.Settings.Default.LogProcedureName = dialog.LogProcedureNames;
            Common.Settings.Default.LogIndent = dialog.LogIndents;
            Common.Settings.Default.LogExceptionDetail = dialog.LogExceptionDetail;
            Common.Settings.Default.Save();
            Logger.ReadSettings();
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e) { HelpViewer.Show(); }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e) { AboutBox.Show(this); }

        private void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }

        private void AppWindow_Closing(object sender, CancelEventArgs e)
        {
            HelpViewer.Close();

            /* Save window settings. */
            Common.Settings.Default.AppWindowMaximized = this.Maximized;
            Common.Settings.Default.AppWindowBounds = this.Bounds;
            Common.Settings.Default.Save();
        }

        #endregion

        #endregion
    }
}
