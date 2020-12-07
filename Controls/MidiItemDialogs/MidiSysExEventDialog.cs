/* MidiSysExEventDialog.cs - Implementation of MidiSysExEventDialog
 * class, used for editing a MIDI system exclusive (SysEx) message/event.
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

/* CheckBox, DockPanel, Label, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit a MIDI system exclusive (SysEx) message/event.</summary>
    public class MidiSysExEventDialog : MidiEventDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSysExEventDialog class.</summary>
        /// <param name="sysExEvent">
        /// MidiSysExEvent object representing the MIDI system exclusive
        /// (SysEx) message/event to edit, or null to create a new one.
        /// </param>
        public MidiSysExEventDialog(MidiSysExEvent sysExEvent)
            : base(sysExEvent)
        {
            int i = this.ControlCount;

            /* Initialize the "Escape" check box. */
            this.EscapeCheckBox = UI.CreateCheckBox(++i, MarginType.Standard, Properties.Resources.Escape, null);

            /* Initialize the "Data" controls. */
            this.DataLabel = UI.CreateLabel(MarginType.Standard, Properties.Resources.Data, true);
            DockPanel panel = MidiEventDialog.CreateDataPanel(ref this.DataHexTextBox, ref this.DataCommentTextBox, ++i);
            this.DataHexTextBox.TextChanged += this.DataHexTextBox_TextChanged;
            this.DataCommentTextBox.IsReadOnly = true;
            this.DataLabel.Target = this.DataHexTextBox;

            /* Build out the window and its content. */
            this.AddUIElement(this.EscapeCheckBox);
            this.AddUIElement(this.DataLabel);
            this.AddUIElement(panel);
            this.BuildOut(UI.ClientWidth, MidiSysExEventDialog.TitleString);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;

            /* If a MidiSysExEvent object was supplied, use it to set initial values. */
            if (this.ForNewItem) return;
            this.DeltaTime = sysExEvent.DeltaTime;
            this.EscapeCheckBox.IsChecked = sysExEvent.Escape;
            this.DataHexTextBox.Text = this.Hex = Midi.FormatHex(sysExEvent.Data, 0, sysExEvent.Data.Length);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static readonly string TitleString = Properties.Resources.Midi +
            " " + Properties.Resources.SysEx + " " + Properties.Resources.Event;

        private CheckBox EscapeCheckBox;
        private Label DataLabel;
        private TextBox DataHexTextBox;
        private TextBox DataCommentTextBox;
        private string Hex;
        private byte[] _Data;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </summary>
        public bool Escape { get { return this.EscapeCheckBox.IsChecked == true; } }

        /// <summary>The data bytes of the event.</summary>
        public byte[] Data { get { return this._Data; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        #region Event Handlers

        private void DataHexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            byte[] bytes = Midi.ParseHex(this.DataHexTextBox.Text);
            if (bytes == null) { this.DataHexTextBox.Text = this.Hex; return; }
            this.Hex = this.DataHexTextBox.Text;
            this._Data = bytes;
            this.DataCommentTextBox.Text = Midi.ReadText(this.Data, 0, this.Data.Length);
        }

        #endregion

        #endregion
    }
}
