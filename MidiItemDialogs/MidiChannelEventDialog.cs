/* MidiChannelEventDialog.cs - Implementation of MidiChannelEventDialog class, used for editing a MIDI channel message/event.
 *
 * Copyright (c) 2020 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* RoutedEventArgs, TextAlignment, Thickness */
using System.Windows;

/* CheckBox, ComboBox, DockPanel, Label, SelectionChangedEventArgs, TextBlock, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit a MIDI channel message/event.</summary>
    public class MidiChannelEventDialog : MidiEventDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiEventDialog class.</summary>
        /// <param name="channelEvent">
        /// MidiChannelEvent object representing the MIDI channel message/event to edit, or null to create a new one.
        /// </param>
        public MidiChannelEventDialog(MidiChannelEvent channelEvent)
            : base(channelEvent)
        {
            int i = this.ControlCount;
            string s;
            DockPanel panel1, panel2;

            /* Initialize the "Running status" check box. */
            this.RunningStatusCheckBox = UI.CreateCheckBox(++i, MarginType.Standard, Properties.Resources.RunningStatus, null);
            this.RunningStatusCheckBox.Checked += this.RunningStatusCheckBox_Checked;
            this.RunningStatusCheckBox.Unchecked += this.RunningStatusCheckBox_Checked;

            /* Initialize the "Message type" label. */
            this.MessageTypeLabel = UI.CreateLabel(MarginType.Standard, Properties.Resources.MessageType, true);

            /* Initialize the "Message type" combo box. */
            this.MessageTypeComboBox = new ComboBox();
            this.MessageTypeComboBox.TabIndex = ++i;
            this.MessageTypeComboBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            foreach (MidiMessageType messageType in MidiChannelEventDialog.MessageTypes)
            {
                s = MidiChannelEvent.GetTypeComment(messageType);
                this.MessageTypeComboBox.Items.Add(s);
            }
            this.MessageTypeComboBox.SelectionChanged += MessageTypeComboBox_SelectionChanged;
            this.MessageTypeLabel.Target = this.MessageTypeComboBox;

            /* Initialize the "Channel" label. */
            this.ChannelLabel = UI.CreateLabel(MarginType.Standard, Properties.Resources.Channel, true);

            /* Initialize the "Channel" text box. */
            this.ChannelTextBox = new TextBox();
            this.ChannelTextBox.TabIndex = ++i;
            this.ChannelTextBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.ChannelTextBox.TextAlignment = TextAlignment.Right;
            this.ChannelTextBox.GotFocus += UI.TextBox_GotFocus;
            this.ChannelTextBox.TextChanged += this.ChannelTextBox_TextChanged;
            this.ChannelLabel.Target = this.ChannelTextBox;

            /* Initialize the "Data 1" controls. */
            this.Data1Label = UI.CreateLabel(MarginType.Standard, Properties.Resources.Data1, true);
            panel1 = MidiEventDialog.CreateDataBytePanel(ref this.Data1TextBox, ref this.Data1TextBlock, ++i);
            this.Data1TextBox.TextChanged += this.Data1TextBox_TextChanged;
            this.Data1Label.Target = this.Data1TextBox;

            /* Initialize the "Data 2" controls. */
            this.Data2Label = UI.CreateLabel(MarginType.Standard, Properties.Resources.Data2, true);
            panel2 = MidiEventDialog.CreateDataBytePanel(ref this.Data2TextBox, ref this.Data2TextBlock, ++i);
            this.Data2TextBox.TextChanged += this.Data2TextBox_TextChanged;
            this.Data2Label.Target = this.Data2TextBox;

            /* Build out the window and its content. */
            this.AddUIElement(this.RunningStatusCheckBox);
            this.AddUIElement(this.MessageTypeLabel);
            this.AddUIElement(this.MessageTypeComboBox);
            this.AddUIElement(this.ChannelLabel);
            this.AddUIElement(this.ChannelTextBox);
            this.AddUIElement(this.Data1Label);
            this.AddUIElement(panel1);
            this.AddUIElement(this.Data2Label);
            this.AddUIElement(panel2);
            s = Text.ParseLabel(Properties.Resources.Channel);
            this.BuildOut(UI.ClientWidth, Properties.Resources.Midi + " " + s + " " + Properties.Resources.Event);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;

            /* If a MidiChannelEvent object was supplied, use it to set initial values. */
            if (this.ForNewItem) return;
            this.DeltaTime = channelEvent.DeltaTime;
            this.RunningStatusCheckBox.IsEnabled = false;
            this.RunningStatusCheckBox.IsChecked = channelEvent.RunningStatus;
            this.MessageTypeComboBox.SelectedItem = MidiChannelEvent.GetTypeComment(channelEvent.MessageType);
            this.ChannelTextBox.Text = channelEvent.Channel.ToString();
            this.Data1TextBox.Text = channelEvent.Data1.ToString();
            if (!MidiChannelEvent.HasData2(channelEvent.MessageType)) return;
            this.Data2TextBox.Text = channelEvent.Data2.ToString();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static readonly MidiMessageType[] MessageTypes = new MidiMessageType[]
        {
            MidiMessageType.NoteOff,
            MidiMessageType.NoteOn,
            MidiMessageType.PolyKeyPressure,
            MidiMessageType.ControlChangeOrChannelMode,
            MidiMessageType.ProgramChange,
            MidiMessageType.ChannelPressure,
            MidiMessageType.PitchBend
        };

        private CheckBox RunningStatusCheckBox;
        private Label MessageTypeLabel;
        private ComboBox MessageTypeComboBox;
        private Label ChannelLabel;
        private TextBox ChannelTextBox;
        private Label Data1Label;
        private TextBox Data1TextBox;
        private TextBlock Data1TextBlock;
        private Label Data2Label;
        private TextBox Data2TextBox;
        private TextBlock Data2TextBlock;
        private int _Channel = -1;
        private int _Data1 = -1;
        private int _Data2 = -1;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Indicates whether or not this event uses running status.</summary>
        public bool RunningStatus { get { return this.RunningStatusCheckBox.IsChecked == true; } }

        /// <summary>Identifies the type of channel message.</summary>
        /// <remarks>This corresponds to the high nibble of the event's status byte.</remarks>
        public MidiMessageType MessageType
        {
            get
            {
                return (this.MessageTypeComboBox.SelectedIndex < 0) ? MidiMessageType.NoteOn :
                    MidiChannelEventDialog.MessageTypes[this.MessageTypeComboBox.SelectedIndex];
            }
        }

        /// <summary>One of the sixteen logical MIDI channels on which this event is transmitted.</summary>
        /// <remarks>This corresponds to the low nibble of the event's status byte.</remarks>
        public int Channel { get { return this._Channel; } }

        /// <summary>The first data byte of the event.</summary>
        public int Data1 { get { return this._Data1; } }

        /// <summary>The second data byte of the event.</summary>
        public int Data2 { get { return this._Data2; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            if (!base.CheckRequiredInput()) return false;

            /* If the event does not use running status, message type and channel are required. */
            if (this.RunningStatusCheckBox.IsChecked == false)
            {
                if (this.MessageTypeComboBox.SelectedIndex < 0) return false;
                if (this.Channel < 0) return false;
            }

            /* Data byte 1 is always required.  Data byte 2 may or may not be, depending on message type. */
            if (this.Data1 < 0) return false;
            if (MidiChannelEvent.HasData2(this.MessageType) && this.Data2 < 0) return false;

            /* All required input has been provided. */
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void RunningStatusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool b = (this.RunningStatusCheckBox.IsChecked == false);
            this.MessageTypeComboBox.IsEnabled = b;
            this.MessageTypeLabel.IsEnabled = b;
            this.ChannelTextBox.IsEnabled = b;
            this.ChannelLabel.IsEnabled = b;
        }

        private void MessageTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Data2TextBox.IsEnabled = MidiChannelEvent.HasData2(this.MessageType);
            this.EnableOkButton();
            this.UpdateData1Text();
            this.UpdateData2Text();
        }

        private void ChannelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.ChannelLabel, ref this._Channel, 0, 15);
            this.UpdateData1Text();
        }

        private void Data1TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.Data1Label, ref this._Data1, 0, sbyte.MaxValue);
            this.UpdateData1Text();
        }

        private void Data2TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.Data2Label, ref this._Data2, 0, sbyte.MaxValue);
            this.UpdateData2Text();
        }

        #endregion

        private void UpdateData1Text()
        {
            this.Data1TextBlock.Text = (this.MessageType == MidiMessageType.NA || this.Data1 < 0) ? null :
                MidiChannelEvent.GetData1Comment(this.MessageType, this.Data1, this.Channel, MidiKeySignature.NA);
        }

        private void UpdateData2Text()
        {
            this.Data2TextBlock.Text = (this.MessageType == MidiMessageType.NA || this.Data2 < 0)
                ? null : MidiChannelEvent.GetData2Comment(this.MessageType, this.Data2);
        }

        #endregion
    }
}
