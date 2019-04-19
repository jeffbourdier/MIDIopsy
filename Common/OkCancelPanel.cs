/* OkCancelPanel.cs - Implementation of OkCancelPanel class, which defines an area containing an OK button and a Cancel button.
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

/* DockPanel, Button, Dock */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Defines an area containing an OK button and a Cancel button.</summary>
    public class OkCancelPanel : DockPanel
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the OkCancelPanel class.</summary>
        public OkCancelPanel()
        {
            this.LastChildFill = false;
            this.InitializeButton(false);
            this.InitializeButton(true);
        }

        #endregion

        #region Private Methods

        /// <summary>Create the OK or Cancel button and add it to the panel.</summary>
        /// <param name="ok">True for the OK button; otherwise (for the Cancel button), false.</param>
        private void InitializeButton(bool ok)
        {
            /* These attributes are common to both buttons (mostly). */
            Button button = new Button();
            button.Width = UI.ButtonWidth;
            button.Height = UI.ButtonHeight;
            double right = ok ? UI.UnitSpace : UI.TripleSpace;
            button.Margin = new Thickness(UI.UnitSpace, UI.DoubleSpace, right, UI.TripleSpace);

            /* These attributes differ depending on the button. */
            if (ok)
            {
                button.Content = Common.Resources.OK;
                button.Click += UI.OkButton_Click;
                button.IsDefault = true;
                button.TabIndex = int.MaxValue - 1;
            }
            else
            {
                button.Content = Common.Resources.Cancel;
                button.IsCancel = true;
                button.TabIndex = int.MaxValue;
            }

            /* The rest applies to both buttons. */
            DockPanel.SetDock(button, Dock.Right);
            this.Children.Add(button);
        }

        #endregion
    }
}
