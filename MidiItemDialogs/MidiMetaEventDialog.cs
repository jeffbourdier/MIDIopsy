/* MidiMetaEventDialog.cs - Implementation of MidiMetaEventDialog class, used for editing a MIDI meta-event.
 *
 * Copyright (c) 2020 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Thickness */
using System.Windows;

/* DockPanel, TextBlock, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit a MIDI meta-event.</summary>
    public class MidiMetaEventDialog : MidiEventDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiMetaEventDialog class.</summary>
        /// <param name="metaEvent">
        /// MidiMetaEvent object representing the MIDI meta-event to edit, or null to create a new one.
        /// </param>
        public MidiMetaEventDialog(MidiMetaEvent metaEvent)
            : base(metaEvent)
        {
            int i = this.ControlCount;
            DockPanel typePanel, dataPanel;

            /* Initialize the "Type" controls. */
            this.TypeLabel = new StandardLabel(Properties.Resources.Type, true);
            this.TypeLabel.TabIndex = ++i;
            this.TypeLabel.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);
            typePanel = MidiEventDialog.CreateDataBytePanel(ref this.TypeTextBox, ref this.TypeTextBlock, ++i);
            this.TypeTextBox.TextChanged += this.TypeTextBox_TextChanged;
            this.TypeLabel.Target = this.TypeTextBox;

            /* Initialize the "Data" controls. */
            this.DataLabel = new StandardLabel(Properties.Resources.Data, true);
            this.DataLabel.TabIndex = ++i;
            this.DataLabel.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);
            dataPanel = MidiEventDialog.CreateDataPanel(ref this.DataHexTextBox, ref this.DataCommentTextBox, ++i);
            this.DataHexTextBox.TextChanged += this.DataHexTextBox_TextChanged;
            this.DataCommentTextBox.TextChanged += this.DataCommentTextBox_TextChanged;
            this.DataLabel.Target = this.DataHexTextBox;

            /* Build out the window and its content. */
            this.AddUIElement(this.TypeLabel);
            this.AddUIElement(typePanel);
            this.AddUIElement(this.DataLabel);
            this.AddUIElement(dataPanel);
            this.BuildOut(UI.ClientWidth, Properties.Resources.Midi + " " + Properties.Resources.MetaEvent);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;

            /* If a MidiMetaEvent object was supplied, use it to set initial values. */
            if (this.ForNewItem) return;
            this.DeltaTime = metaEvent.DeltaTime;
            this.TypeTextBox.Text = metaEvent.Type.ToString();
            this.NoValidation = true;
            this.DataHexTextBox.Text = this.Hex = Midi.FormatHex(metaEvent.Data, 0, metaEvent.Data.Length);
            this.NoValidation = false;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private StandardLabel TypeLabel;
        private TextBox TypeTextBox;
        private TextBlock TypeTextBlock;
        private StandardLabel DataLabel;
        private TextBox DataHexTextBox;
        private TextBox DataCommentTextBox;
        private int DataLength;
        private bool IsDataLengthValid;
        private string Hex;
        private int _Type = -1;
        private byte[] _Data;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Meta-event type (always less than 128).</summary>
        public int Type { get { return this._Type; } }

        /// <summary>The data bytes of the event.</summary>
        public byte[] Data { get { return this._Data; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            if (!base.CheckRequiredInput()) return false;

            /* Meta-event type is required. */
            if (this.Type < 0) return false;

            /* Data length must be valid for the meta-event type. */
            if (!this.IsDataLengthValid) return false;

            /* All required input has been provided. */
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void TypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.TypeLabel, ref this._Type, 0, sbyte.MaxValue);
            this.TypeTextBlock.Text = (this.Type < 0) ? null : MidiMetaEvent.GetTypeComment(this.Type);
            this.DataLength = MidiMetaEvent.GetDataLength(this.Type);
            this.DataCommentTextBox.IsReadOnly = (this.DataLength >= 0);
            this.UpdateDataComment();
        }

        private void DataHexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /* If the hex input is not valid, revert to previous hex. */
            byte[] bytes = Midi.ParseHex(this.DataHexTextBox.Text);
            if (bytes == null)
            {
                this.NoValidation = true;
                this.DataHexTextBox.Text = this.Hex;
                this.NoValidation = false;
                return;
            }

            /* The hex input is valid. */
            this.Hex = this.DataHexTextBox.Text;
            this._Data = bytes;
            this.UpdateDataComment();
        }

        private void DataCommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /* Prevent unnecessary recursion. */
            if (this.NoValidation) return;
            this.NoValidation = true;

            /* Populate hex box based on text/character/string input. */
            int i, n = this.DataCommentTextBox.Text.Length;
            byte[] bytes = new byte[n];
            for (i = 0; i < n; ++i) bytes[i] = (byte)this.DataCommentTextBox.Text[i];
            this.DataHexTextBox.Text = Midi.FormatHex(bytes, 0, n);
            this.NoValidation = false;
        }

        #endregion

        private void UpdateDataComment()
        {
            this.IsDataLengthValid = (this.DataLength < 0) ? true :
                (((this.Data == null) ? 0 : this.Data.Length) == this.DataLength);
            this.NoValidation = true;
            this.DataCommentTextBox.Text = (this.Type < 0 || this.Data == null || !this.IsDataLengthValid)
                ? null : MidiMetaEvent.GetDataComment(this.Type, this.Data);
            this.NoValidation = false;
            this.EnableOkButton();
        }

        #endregion
    }
}
