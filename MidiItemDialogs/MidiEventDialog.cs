/* MidiEventDialog.cs - Implementation of MidiEventDialog class, used for editing a MIDI (MTrk) event.
 *
 * Copyright (c) 2020 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* TextAlignment, TextWrapping, Thickness */
using System.Windows;

/* Dock, DockPanel, Label, ScrollBarVisibility, TextBlock, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit a MIDI (MTrk) event.</summary>
    public abstract class MidiEventDialog : MidiItemDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiEventDialog class.</summary>
        /// <param name="midiEvent">
        /// MidiEvent object representing the MIDI (MTrk) event to edit, or null to create a new one.
        /// </param>
        public MidiEventDialog(MidiEvent midiEvent)
            : base(midiEvent)
        {
            int i = 0;

            /* Initialize the "Delta-time" label. */
            this.DeltaTimeLabel = UI.CreateLabel(MarginType.Top, Properties.Resources.DeltaTime, true);

            /* Initialize the "Delta-time" text box. */
            this.DeltaTimeTextBox = new TextBox();
            this.DeltaTimeTextBox.TabIndex = ++i;
            this.DeltaTimeTextBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.DeltaTimeTextBox.TextAlignment = TextAlignment.Right;
            this.DeltaTimeTextBox.GotFocus += UI.TextBox_GotFocus;
            this.DeltaTimeTextBox.TextChanged += this.DeltaTimeTextBox_TextChanged;
            this.DeltaTimeLabel.Target = this.DeltaTimeTextBox;
            this.InitialElement = this.DeltaTimeTextBox;

            /* Build out the window and its content. */
            this.AddUIElement(this.DeltaTimeLabel);
            this.AddUIElement(this.DeltaTimeTextBox);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private Label DeltaTimeLabel;
        private TextBox DeltaTimeTextBox;
        private int _DeltaTime = -1;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>The amount of time (in ticks) between the previous event in the track and this one.</summary>
        public int DeltaTime
        {
            get { return this._DeltaTime; }
            set { this.DeltaTimeTextBox.Text = value.ToString(); }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            /* Delta-time is required for all events. */
            if (this.DeltaTime < 0) return false;

            /* All required input has been provided. */
            return true;
        }

        /* Return a dock panel with a byte-size text box and a text block for a corresponding comment. */
        protected static DockPanel CreateDataBytePanel(ref TextBox textBox, ref TextBlock textBlock, int tabIndex)
        {
            /* Initialize the text box. */
            textBox = new TextBox();
            textBox.TabIndex = tabIndex;
            textBox.Margin = new Thickness(0, 0, UI.UnitSpace, 0);
            textBox.TextAlignment = TextAlignment.Right;
            textBox.Width = 64;
            textBox.GotFocus += UI.TextBox_GotFocus;

            /* Initialize the text block for the comment. */
            textBlock = new TextBlock();
            textBlock.Margin = new Thickness(UI.UnitSpace, UI.HalfSpace, 0, 0);

            /* Build a dock panel for the text box and comment block. */
            DockPanel.SetDock(textBox, Dock.Left);
            DockPanel panel = new DockPanel();
            panel.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            panel.Children.Add(textBox);
            panel.Children.Add(textBlock);
            return panel;
        }

        /* Return a dock panel with two text boxes: one for hex, the other for a corresponding comment. */
        protected static DockPanel CreateDataPanel(ref TextBox hexTextBox, ref TextBox commentTextBox, int tabIndex)
        {
            /* Initialize the hex text box. */
            hexTextBox = new TextBox();
            hexTextBox.TabIndex = tabIndex;
            hexTextBox.Margin = new Thickness(0, 0, UI.HalfSpace, 0);
            hexTextBox.Width = 196;
            hexTextBox.Height = 64;
            hexTextBox.TextWrapping = TextWrapping.Wrap;
            hexTextBox.FontFamily = UI.MonospaceFont;
            hexTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            /* Initialize the comment text box. */
            commentTextBox = new TextBox();
            commentTextBox.TabIndex = tabIndex + 1;
            commentTextBox.Margin = new Thickness(UI.HalfSpace, 0, 0, 0);
            commentTextBox.Height = 64;
            commentTextBox.TextWrapping = TextWrapping.Wrap;
            commentTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            /* Build a dock panel for the text boxes. */
            DockPanel.SetDock(hexTextBox, Dock.Left);
            DockPanel panel = new DockPanel();
            panel.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            panel.Children.Add(hexTextBox);
            panel.Children.Add(commentTextBox);
            return panel;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void DeltaTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        { this.ValidateNumericInput(this.DeltaTimeLabel, ref this._DeltaTime, 0, Midi.MaxVLQ); }

        #endregion

        #endregion
    }
}
