/* MIDIopsyApp.cs - Implementation of MIDIopsyApp class, which encapsulates this application.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* STAThread, Uri, UriKind */
using System;

/* Application */
using System.Windows;

/* BitmapFrame, BitmapImage, BitmapSource */
using System.Windows.Media.Imaging;

/* Settings */
using JeffBourdier.Properties;


namespace JeffBourdier
{
    /// <summary>Encapsulates the MIDIopsy application.</summary>
    public class MIDIopsyApp : Application
    {
        /**********
         * Fields *
         **********/

        #region Public Fields

        public const string Name = "MIDIopsy";
        public const string AuthorName = "Jeff Bourdier";
        public const string FullName = MIDIopsyApp.Name + " by " + MIDIopsyApp.AuthorName;
        public const string Version = "1.2";

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>
        /// Returns the setting (from the config file) that specifies whether or not the text in a given column should wrap.
        /// </summary>
        /// <param name="header">The header content of the given column.</param>
        /// <returns>True if the text in the column should wrap; false if it should not; otherwise, null.</returns>
        public static bool? GetWrapSetting(object header) { return Settings.Default["Wrap" + header] as bool?; }

        /// <summary>Initializes a new instance of the BitmapSource class using a given resource file.</summary>
        /// <param name="fileName">The name of the resource file (representing an icon or other image).</param>
        /// <returns>A BitmapSource instance (either a BitmapFrame or a BitmapImage) created from the resource file.</returns>
        /// <remarks>The "Build Action" property of the resource file must be set to "Resource".</remarks>
        public static BitmapSource CreateBitmapSource(string fileName)
        {
            Uri uri = new Uri("pack://application:,,,/resources/" + fileName, UriKind.RelativeOrAbsolute);
            if (fileName.EndsWith(".ico")) return BitmapFrame.Create(uri);
            return new BitmapImage(uri);
        }

        #endregion

        #region Private Methods

        /// <summary>The main entry point for the application.</summary>
        /// <remarks>
        /// Without the STAThread attribute, an exception is thrown when trying to open a file dialog in debug mode.
        /// </remarks>
        [STAThread]
        private static void Main()
        {
            /* Start the application and open a new window. */
            MIDIopsyApp app = new MIDIopsyApp();
            MIDIopsyWindow window = new MIDIopsyWindow();
            app.Run(window);
        }

        #endregion
    }
}
