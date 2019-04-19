/* StandardLabel.cs - Implementation of StandardLabel class, which standardizes the way labels are created.
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


/* Thickness */
using System.Windows;

/* Label */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Extends the Label class by standardizing the way labels are created.</summary>
    public class StandardLabel : Label
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the StandardLabel class.</summary>
        /// <param name="content">The content of the label.</param>
        /// <param name="fit">True if the label is to be fit to its content; otherwise, false.</param>
        public StandardLabel(object content, bool fit)
        {
            /* Set the content of the label. */
            this.Content = content;

            /* If the label is not to be fit to its content, we're done. */
            if (!fit) return;

            /* Fit the label to its content by removing its padding and setting its height based on its font size. */
            this.Padding = new Thickness();
            this.Height = this.FontSize * 3 / 2;
        }

        #endregion
    }
}
