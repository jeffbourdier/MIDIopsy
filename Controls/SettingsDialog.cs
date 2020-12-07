/* SettingsDialog.cs - Implementation of SettingsDialog class, which allows the user to view and update configuration settings.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Thickness, UIElement */
using System.Windows;

/* CheckBox, GroupBox, StackPanel */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that allows the user to view and update configuration settings.</summary>
    public class SettingsDialog : DialogWindow
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the SettingsDialog class.</summary>
        public SettingsDialog()
        {
            int i = 0;

            /* Initialize a check box indicating whether or not the text in the column bound to each non-
             * numeric display field should wrap, along with the stack panel to contain the check boxes.
             */
            this.WrapStackPanel = new StackPanel();
            foreach (MidiItem.DisplayField field in MidiItem.DisplayFields)
            {
                if (field.RightAlign) continue;
                string s = UI.ParseLabel(field.LabelText);
                bool? b = MIDIopsyApp.GetWrapSetting(s);
                CheckBox checkBox = UI.CreateCheckBox(++i, (i > 1) ? MarginType.Standard : MarginType.Top, field.LabelText, b);
                this.WrapStackPanel.Children.Add(checkBox);
            }
            this.InitialElement = this.WrapStackPanel.Children[0];

            /* Build out a group box to contain the stack panel with the text wrapping check boxes. */
            GroupBox groupBox = new GroupBox();
            groupBox.Header = Properties.Resources.TextWrapping;
            groupBox.Content = this.WrapStackPanel;
            groupBox.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.UnitSpace);

            /* Build out the window and its content. */
            this.AddUIElement(groupBox);
            this.BuildOut(SettingsDialog.ClientWidth, Properties.Resources.Settings);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private const int ClientWidth = UI.ClientWidth / 2;

        private StackPanel WrapStackPanel;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Determines whether or not a given text wrapping check box is checked.</summary>
        /// <param name="text">The label content of the check box.</param>
        /// <returns>True if the box is checked; false if the box is unchecked; otherwise, null.</returns>
        public bool? IsWrapBoxChecked(string text)
        {
            foreach (UIElement element in this.WrapStackPanel.Children)
            {
                CheckBox checkBox = element as CheckBox;
                if (checkBox == null || checkBox.Content as string != text) continue;
                return checkBox.IsChecked;
            }
            return null;
        }

        #endregion
    }
}
