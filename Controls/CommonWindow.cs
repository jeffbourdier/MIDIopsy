/* CommonWindow.cs - Implementation of CommonWindow class, which encapsulates common (non-dialog) window procedures.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Rect, Window, WindowStartupLocation, WindowState */
using System.Windows;


namespace JeffBourdier
{
    /// <summary>Extends the Window class by encapsulating common (non-dialog) window procedures.</summary>
    public class CommonWindow : Window
    {
        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets whether or not the window is maximized.</summary>
        public bool Maximized { get { return this.WindowState == WindowState.Maximized; } }

        /// <summary>Gets the location and size of the window when it is not maximized.</summary>
        public Rect Bounds
        {
            get
            {
                return (this.WindowState == WindowState.Normal) ?
                    new Rect(this.Left, this.Top, this.Width, this.Height) : this.RestoreBounds;
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Restores the preferences of this window object from user configuration settings.</summary>
        /// <param name="maximized">Indicates whether or not the window is maximized.</param>
        /// <param name="bounds">The location and size of the window when it is not maximized.</param>
        public void Restore(bool maximized, Rect bounds)
        {
            if (maximized) this.WindowState = WindowState.Maximized;

            if (bounds.Width < 1 || bounds.Height < 1) return;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = bounds.Top;
            this.Left = bounds.Left;
            this.Width = bounds.Width;
            this.Height = bounds.Height;
        }

        #endregion
    }
}
