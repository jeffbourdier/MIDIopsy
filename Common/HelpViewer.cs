/* HelpViewer.cs - Implementation of HelpViewer class, which allows the user to view help for an application.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Uri, Exception */
using System;

/* CancelEventArgs */
using System.ComponentModel;

/* Assembly */
using System.Reflection;

/* ResourceManager */
using System.Resources;

/* MessageBox, Application, MessageBoxButton, MessageBoxImage */
using System.Windows;

/* WebBrowser */
using System.Windows.Controls;

/* Keyboard */
using System.Windows.Input;

/* BitmapFrame */
using System.Windows.Media.Imaging;


namespace JeffBourdier
{
    /// <summary>Displays a browser window that allows the user to view help for the application.</summary>
    public static class HelpViewer
    {
        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>
        /// The name of the resource in the entry assembly (presumably the application .exe file) containing
        /// the text to show in the help window.  If there is no such resource, the help window cannot be shown.
        /// </summary>
        public const string HelpTextResourceName = "HelpText";

        #endregion

        #region Private Fields

        /// <summary>The help window.  There is one per application instance.</summary>
        private static StandardWindow Window;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Displays a help viewer for the application.</summary>
        public static void Show()
        {
            string s;

            /* If the help window is already shown, activate it. */
            if (HelpViewer.Window != null)
            {
                HelpViewer.Window.Activate();
                return;
            }

            /* The help window is not already shown, so build and show it.  First though, try to
             * get the help text, because if it doesn't exist, the help window cannot be shown.
             */
            Assembly assembly = Assembly.GetEntryAssembly();
            ResourceManager manager = new ResourceManager("JeffBourdier.Properties.Resources", assembly);
            try { s = manager.GetString(HelpViewer.HelpTextResourceName); }
            catch (Exception ex)
            {
                s = Text.FormatErrorMessage(Common.Resources.NoHelp, ex);
                MessageBox.Show(Application.Current.MainWindow, s,
                    AppHelper.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /* Start building the window. */
            HelpViewer.Window = new StandardWindow();

            /* Set the window icon.  (Note: "Build Action" property must be set to "Resource") */
            Uri uri = AppHelper.CreateResourceUri(true, "Help.ico");
            HelpViewer.Window.Icon = BitmapFrame.Create(uri);
            HelpViewer.Window.Title = AppHelper.Title + " " + Common.Resources.Help;

            /* Set the content of the window to the help text. */
            WebBrowser browser = new WebBrowser();
            browser.NavigateToString(s);
            HelpViewer.Window.Content = browser;

            /* Finish building the window. */
            HelpViewer.Window.RestorePreferences(Common.Settings.Default.HelpWindowMaximized,
                Common.Settings.Default.HelpWindowBounds);
            HelpViewer.Window.Closing += HelpViewer.Window_Closing;
            HelpViewer.Window.Show();
            Keyboard.Focus(browser);
        }

        /// <summary>Closes the help viewer.</summary>
        public static void Close()
        {
            if (HelpViewer.Window == null) return;
            HelpViewer.Window.Close();
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private static void Window_Closing(object sender, CancelEventArgs e)
        {
            /* Save help window settings. */
            Common.Settings.Default.HelpWindowMaximized = HelpViewer.Window.Maximized;
            Common.Settings.Default.HelpWindowBounds = HelpViewer.Window.Bounds;
            Common.Settings.Default.Save();
            HelpViewer.Window = null;
        }

        #endregion

        #endregion
    }
}
