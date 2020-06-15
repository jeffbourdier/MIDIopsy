/* MidiItemDialog.cs - Implementation of MidiItemDialog class, used for editing a MIDI item (a line of data from a MIDI file).
 *
 * Copyright (c) 2020 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Label, TextBox */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit a MIDI item (a line of data from a MIDI file).</summary>
    public abstract class MidiItemDialog : StandardDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiItemDialog class.</summary>
        /// <param name="item">MIDI item to edit, or null to create a new item.</param>
        protected MidiItemDialog(MidiItem item) { this._ForNewItem = (item == null); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected bool NoValidation = false;

        #endregion

        #region Private Fields

        private bool _ForNewItem;

        #endregion

        /**************
         * Properties *
         **************/

        #region Protected Properties

        /// <summary>
        /// True if the dialog is being used to create a new item;
        /// otherwise (it is being used to edit an existing item), false.
        /// </summary>
        protected bool ForNewItem { get { return this._ForNewItem; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected void ValidateNumericInput(Label label, ref int value, int min, int max)
        {
            /* Prevent unnecessary recursion. */
            if (this.NoValidation) return;
            this.NoValidation = true;

            /* Allow null/empty (revert to initial value). */
            TextBox textBox = label.Target as TextBox;
            if (string.IsNullOrEmpty(textBox.Text)) value = -1;
            else UI.ValidateNumericInput(textBox, ref value, min, max, label.Content as string);

            /* In case of invalid/initial value, text should be null/empty. */
            if (value < 0) textBox.Text = null;
            this.EnableOkButton();
            this.NoValidation = false;
        }

        protected void EnableOkButton() { this.OkButton.IsEnabled = this.CheckRequiredInput(); }

        protected virtual bool CheckRequiredInput() { return true; }

        #endregion
    }
}
