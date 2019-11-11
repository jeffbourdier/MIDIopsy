/* CommandButton.cs - Implementation of CommandButton class (a button that invokes a command).
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


/* Environment */
using System;

/* Thickness */
using System.Windows;

/* Button */
using System.Windows.Controls;

/* RoutedUICommand, InputGesture, KeyGesture */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>Represents a Windows button control that invokes a command when pressed.</summary>
    public class CommandButton : Button
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the Button class that invokes a command when pressed.</summary>
        /// <param name="command">A command to invoke when the button is pressed.</param>
        public CommandButton(RoutedUICommand command)
        {
            /* Initialize the button. */
            this.Command = command;
            this.Height = UI.ButtonHeight;
            this.Width = UI.ButtonWidth;
            this.Margin = new Thickness(UI.UnitSpace);

            /* Find the first key gesture in the input gesture collection. */
            foreach (InputGesture gesture in command.InputGestures)
            {
                if (!(gesture is KeyGesture)) continue;

                /* We've found a key gesture.  Get its display string. */
                this.Shortcut = (gesture as KeyGesture).GetDisplayStringForCulture(null);
                break;
            }

            /* Set the content and tool-tip based on the command text and gesture display string. */
            this.Text = command.Text;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string Shortcut = null;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Sets the content and tool-tip for this button.</summary>
        public string Text
        {
            set
            {
                this.Content = value;

                /* If applicable, set the tool-tip to include the keyboard shortcut. */
                if (string.IsNullOrEmpty(this.Shortcut)) return;
                this.ToolTip = string.Format("{0} ({1})", value, this.Shortcut);
            }
        }

        #endregion
    }
}
