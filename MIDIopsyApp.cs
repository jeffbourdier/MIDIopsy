/* MIDIopsyApp.cs - Implementation of MIDIopsyApp class, which encapsulates this application.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* STAThread */
using System;

/* Application */
using System.Windows;


namespace JeffBourdier
{
    /// <summary>Encapsulates the MIDIopsy application. </summary>
    public class MIDIopsyApp : Application
    {
        /***********
         * Methods *
         ***********/

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
