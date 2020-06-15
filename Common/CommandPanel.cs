/* CommandPanel.cs - Implementation of CommandPanel class, which arranges command buttons into a single line.
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


/* List */
using System.Collections.Generic;

/* Orientation, StackPanel */
using System.Windows.Controls;

/* RoutedUICommand */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>Arranges command buttons into a single line that can be oriented horizontally or vertically.</summary>
    public class CommandPanel : StackPanel
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the CommandPanel class.</summary>
        public CommandPanel(List<RoutedUICommand> commands, bool horizontal)
        {
            /* Orient the button panel. */
            if (horizontal) this.Orientation = Orientation.Horizontal;

            /* Create and add each command button to the panel. */
            for (int i = 0; i < commands.Count; )
            {
                CommandButton button = new CommandButton(commands[i]);
                button.TabIndex = ++i;
                this.Children.Add(button);
            }
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private int _TabIndexOffset;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// Gets or sets the current value that is added to the tab index of each child (command button) of this panel.
        /// </summary>
        public int TabIndexOffset
        {
            get { return this._TabIndexOffset; }

            set
            {
                /* Save ourselves a little time if the offset is not changing. */
                if (value == this.TabIndexOffset) return;

                /* Change the offset of the tab index of each child (command button) of this panel to the passed value. */
                UI.OffsetTabIndexes(this, 0, value - this.TabIndexOffset);

                /* Now that we no longer need the old value, we can overwrite it. */
                this._TabIndexOffset = value;
            }
        }

        #endregion
    }
}
