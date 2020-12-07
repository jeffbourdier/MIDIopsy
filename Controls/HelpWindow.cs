/* HelpWindow.cs - Implementation of HelpWindow class, which displays a browser with help for the application.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* CancelEventArgs */
using System.ComponentModel;

/* RoutedEventArgs */
using System.Windows;

/* WebBrowser */
using System.Windows.Controls;

/* ApplicationCommands, FocusManager */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>Displays a browser with help for the application.</summary>
    public class HelpWindow : CommonWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a HelpWindow object.</summary>
        public HelpWindow()
        {
            /* This browser containing the (HTML) help text will serve as the content of the window. */
            this.Browser = new WebBrowser();
            this.Browser.NavigateToString(Properties.Resources.HelpText);

            /* Build the window. */
            this.Content = this.Browser;
            this.Icon = MIDIopsyApp.CreateBitmapSource("Help.ico");
            this.Title = MIDIopsyApp.Name + " " + ApplicationCommands.Help.Text;
            this.Restore(Properties.Settings.Default.HelpWindowMaximized, Properties.Settings.Default.HelpWindowBounds);
            this.Loaded += this.HelpWindow_Loaded;
            this.Closing += this.HelpWindow_Closing;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private WebBrowser Browser;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        #region Event Handlers

        private void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        { FocusManager.SetFocusedElement(this, this.Browser); }

        private void HelpWindow_Closing(object sender, CancelEventArgs e)
        {
            /* Save help window settings. */
            Properties.Settings.Default.HelpWindowMaximized = this.Maximized;
            Properties.Settings.Default.HelpWindowBounds = this.Bounds;
            Properties.Settings.Default.Save();
        }

        #endregion

        #endregion
    }
}
