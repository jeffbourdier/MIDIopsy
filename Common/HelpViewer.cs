/* HelpViewer.cs - Implementation of HelpViewer class, which allows the user to view help for an application.
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


/* Exception, Uri */
using System;

/* CancelEventArgs */
using System.ComponentModel;

/* Bitmap */
using System.Drawing;

/* MemoryStream */
using System.IO;

/* Assembly */
using System.Reflection;

/* ResourceManager */
using System.Resources;

/* Application, MessageBox, MessageBoxButton, MessageBoxImage */
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

        #region Private Fields

        /// <summary>
        /// The text (from an HTML file resource of the same name in the entry assembly, presumably the application
        /// .exe file) to show in the help window.  (If there is no such resource, the help window cannot be shown.)
        /// </summary>
        private static string HelpText;

        private static ResourceManager ResourceManager;

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
            HelpViewer.ResourceManager = new ResourceManager("JeffBourdier.Properties.Resources", assembly);
            try { HelpViewer.HelpText = HelpViewer.ResourceManager.GetString("HelpText"); }
            catch (Exception ex)
            {
                Logger.WriteMessage(Common.Resources.NoHelp);
                Logger.WriteException(ex);

                string s = Text.FormatErrorMessage(Common.Resources.NoHelp, ex);
                MessageBox.Show(Application.Current.MainWindow, s,
                    AppHelper.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /* Replace each image source in the help text with its equivalent base-64 encoded string. */
            for (int i = 1; i > 0; i = HelpViewer.EncodeImage(i)) ;

            /* Start building the window. */
            HelpViewer.Window = new StandardWindow();

            /* Set the window icon.  (Note: "Build Action" property must be set to "Resource") */
            Uri uri = AppHelper.CreateResourceUri(true, "Help.ico");
            HelpViewer.Window.Icon = BitmapFrame.Create(uri);
            HelpViewer.Window.Title = AppHelper.Title + " " + Common.Resources.Help;

            /* Set the content of the window to the help text. */
            WebBrowser browser = new WebBrowser();
            browser.NavigateToString(HelpViewer.HelpText);
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

        /* Replace an image source in the help text with its equivalent base-64 encoded string. */
        private static int EncodeImage(int index)
        {
            const string format = "src=\"{0}\"";

            /* Parse out the image source (file name) from the help text. */
            int i = HelpViewer.HelpText.IndexOf("<img ", index);
            if (i < 0) return i;
            i = HelpViewer.HelpText.IndexOf("src=\"", i += 5);
            if (i < 0) return i;
            int j = HelpViewer.HelpText.IndexOf('"', i += 5);
            if (j < 0) return j;
            string name = HelpViewer.HelpText.Substring(i, j - i);

            /* Extract the image resource. */
            string s = Path.GetFileNameWithoutExtension(name);
            Bitmap bitmap = HelpViewer.ResourceManager.GetObject(s) as Bitmap;
            if (bitmap == null) return j;

            /* Convert the image to a byte array. */
            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, bitmap.RawFormat);
                bytes = stream.ToArray();
            }

            /* Convert the byte array to its equivalent base-64 encoded string, and
             * use that to replace the image source (file name) in the help text.
             */
            s = "data:image/png;base64," + Convert.ToBase64String(bytes);
            name = string.Format(format, name);
            s = string.Format(format, s);
            HelpViewer.HelpText = HelpViewer.HelpText.Replace(name, s);
            return HelpViewer.HelpText.IndexOf('"', j);
        }

        #endregion
    }
}
