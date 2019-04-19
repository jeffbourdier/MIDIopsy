/* MidiHeaderDialog.cs - Implementation of MidiHeaderDialog class, used for editing the header (MThd) chunk of a MIDI file.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Thickness, TextAlignment, RoutedEventArgs, MessageBox, MessageBoxButton, MessageBoxImage */
using System.Windows;

/* ComboBox, TextBox, RadioButton, GroupBox, SelectionChangedEventArgs, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit the header (MThd) chunk of a MIDI file.</summary>
    public class MidiHeaderDialog : StandardDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiHeaderDialog class.</summary>
        public MidiHeaderDialog()
        {
            /* Initialize the "Format" label. */
            this.FormatLabel = new StandardLabel(Properties.Resources.Format, true);
            this.FormatLabel.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.HalfSpace);

            /* Initialize the "Format" combo box. */
            this.FormatComboBox = new ComboBox();
            this.FormatComboBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.FormatComboBox.Items.Add(Properties.Resources.Format0);
            this.FormatComboBox.Items.Add(Properties.Resources.Format1);
            this.FormatComboBox.Items.Add(Properties.Resources.Format2);
            this.FormatComboBox.SelectionChanged += FormatComboBox_SelectionChanged;
            this.FormatLabel.Target = this.FormatComboBox;

            /* Initialize the "Number of tracks" label. */
            this.NumberOfTracksLabel = new StandardLabel(Properties.Resources.NumberOfTracks, true);
            this.NumberOfTracksLabel.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);

            /* Initialize the "Number of tracks" text box. */
            this.NumberOfTracksTextBox = new TextBox();
            this.NumberOfTracksTextBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.NumberOfTracksTextBox.TextAlignment = TextAlignment.Right;
            this.NumberOfTracksTextBox.TextChanged += NumberOfTracksTextBox_TextChanged;
            this.NumberOfTracksLabel.Target = this.NumberOfTracksTextBox;

            /* Initialize the "Metrical time" radio button. */
            this.MetricalTimeRadioButton = new RadioButton();
            this.MetricalTimeRadioButton.Content = Properties.Resources.MetricalTime;
            this.MetricalTimeRadioButton.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.UnitSpace);
            this.MetricalTimeRadioButton.Checked += MetricalTimeRadioButton_Checked;

            /* Initialize the "Ticks per quarter note" label. */
            this.TicksPerQuarterNoteLabel = new StandardLabel(Properties.Resources.TicksPerQuarterNote, true);
            this.TicksPerQuarterNoteLabel.Margin = new Thickness(UI.IndentSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);
            this.TicksPerQuarterNoteLabel.IsEnabled = false;

            /* Initialize the "Ticks per quarter note" text box. */
            this.TicksPerQuarterNoteTextBox = new TextBox();
            this.TicksPerQuarterNoteTextBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.TicksPerQuarterNoteTextBox.TextAlignment = TextAlignment.Right;
            this.TicksPerQuarterNoteTextBox.TextChanged += TicksPerQuarterNoteTextBox_TextChanged;
            this.TicksPerQuarterNoteTextBox.IsEnabled = false;
            this.TicksPerQuarterNoteLabel.Target = this.TicksPerQuarterNoteTextBox;

            /* Initialize the "Time-code-based time" radio button. */
            this.TimeCodeBasedTimeRadioButton = new RadioButton();
            this.TimeCodeBasedTimeRadioButton.Content = Properties.Resources.TimeCodeBasedTime;
            this.TimeCodeBasedTimeRadioButton.Margin =
                new Thickness(UI.TripleSpace, UI.DoubleSpace, UI.TripleSpace, UI.UnitSpace);
            this.TimeCodeBasedTimeRadioButton.Checked += TimeCodeBasedTimeRadioButton_Checked;

            /* Initialize the "Frames per second" label. */
            this.FramesPerSecondLabel = new StandardLabel(Properties.Resources.FramesPerSecond, true);
            this.FramesPerSecondLabel.Margin = new Thickness(UI.IndentSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);
            this.FramesPerSecondLabel.IsEnabled = false;

            /* Initialize the "Frames per second" combo box. */
            this.FramesPerSecondComboBox = new ComboBox();
            this.FramesPerSecondComboBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            for (int i = 0; i < MidiHeaderData.FramesPerSecondCount; ++i)
                this.FramesPerSecondComboBox.Items.Add(MidiHeaderData.FramesPerSecondStrings[i]);
            this.FramesPerSecondComboBox.SelectionChanged += FramesPerSecondComboBox_SelectionChanged;
            this.FramesPerSecondComboBox.IsEnabled = false;
            this.FramesPerSecondLabel.Target = this.FramesPerSecondComboBox;

            /* Initialize the "Ticks per frame" label. */
            this.TicksPerFrameLabel = new StandardLabel(Properties.Resources.TicksPerFrame, true);
            this.TicksPerFrameLabel.Margin = new Thickness(UI.IndentSpace, UI.UnitSpace, UI.TripleSpace, UI.HalfSpace);
            this.TicksPerFrameLabel.IsEnabled = false;

            /* Initialize the "Ticks per frame" text box. */
            this.TicksPerFrameTextBox = new TextBox();
            this.TicksPerFrameTextBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.TripleSpace);
            this.TicksPerFrameTextBox.TextAlignment = TextAlignment.Right;
            this.TicksPerFrameTextBox.TextChanged += TicksPerFrameTextBox_TextChanged;
            this.TicksPerFrameTextBox.IsEnabled = false;
            this.TicksPerFrameLabel.Target = this.TicksPerFrameTextBox;

            /* Build out the stack panel, which will serve as the content for the "Delta-time division" group box. */
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(this.MetricalTimeRadioButton);
            stackPanel.Children.Add(this.TicksPerQuarterNoteLabel);
            stackPanel.Children.Add(this.TicksPerQuarterNoteTextBox);
            stackPanel.Children.Add(this.TimeCodeBasedTimeRadioButton);
            stackPanel.Children.Add(this.FramesPerSecondLabel);
            stackPanel.Children.Add(this.FramesPerSecondComboBox);
            stackPanel.Children.Add(this.TicksPerFrameLabel);
            stackPanel.Children.Add(this.TicksPerFrameTextBox);

            /* Build out the "Delta-time division" group box. */
            this.DeltaTimeDivisionGroupBox = new GroupBox();
            this.DeltaTimeDivisionGroupBox.Header = Properties.Resources.DeltaTimeDivision;
            this.DeltaTimeDivisionGroupBox.Content = stackPanel;
            this.DeltaTimeDivisionGroupBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);

            /* Build out the window and its content. */
            this.AddUIElement(this.FormatLabel);
            this.AddUIElement(this.FormatComboBox);
            this.AddUIElement(this.NumberOfTracksLabel);
            this.AddUIElement(this.NumberOfTracksTextBox);
            this.AddUIElement(this.DeltaTimeDivisionGroupBox);
            this.BuildOut(320, Properties.Resources.MidiHeader);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private StandardLabel FormatLabel;
        private ComboBox FormatComboBox;
        private StandardLabel NumberOfTracksLabel;
        private TextBox NumberOfTracksTextBox;
        private RadioButton MetricalTimeRadioButton;
        private StandardLabel TicksPerQuarterNoteLabel;
        private TextBox TicksPerQuarterNoteTextBox;
        private RadioButton TimeCodeBasedTimeRadioButton;
        private StandardLabel FramesPerSecondLabel;
        private ComboBox FramesPerSecondComboBox;
        private StandardLabel TicksPerFrameLabel;
        private TextBox TicksPerFrameTextBox;
        private GroupBox DeltaTimeDivisionGroupBox;
        private uint NumberOfTracks = uint.MaxValue;
        private uint TicksPerQuarterNote = uint.MaxValue;
        private uint TicksPerFrame = uint.MaxValue;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Creates a new MidiHeaderChunk object based on the user's input.</summary>
        /// <returns>A new MidiHeaderChunk object (based on the user's input).</returns>
        public MidiHeaderChunk CreateChunk()
        {
            if (this.MetricalTimeRadioButton.IsChecked == true)
                return new MidiHeaderChunk((uint)this.FormatComboBox.SelectedIndex, this.NumberOfTracks,
                    this.TicksPerQuarterNote);
            else if (this.TimeCodeBasedTimeRadioButton.IsChecked == true)
                return new MidiHeaderChunk((uint)this.FormatComboBox.SelectedIndex, this.NumberOfTracks,
                    MidiHeaderData.FramesPerSecondValues[this.FramesPerSecondComboBox.SelectedIndex], this.TicksPerFrame);
            return null;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FormatComboBox.SelectedIndex == 0)
            {
                /* Number of tracks will always be 1 for a format 0 file. */
                this.NumberOfTracksTextBox.IsEnabled = false;
                this.NumberOfTracksTextBox.Text = "1";
            }
            else this.NumberOfTracksTextBox.IsEnabled = true;

            this.EnableOkButton();
        }

        private void NumberOfTracksTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.NumberOfTracksLabel, ushort.MaxValue, ref this.NumberOfTracks);
            this.EnableOkButton();
        }

        private void MetricalTimeRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableDivisionControls(); }

        private void TicksPerQuarterNoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.TicksPerQuarterNoteLabel, (uint)short.MaxValue, ref this.TicksPerQuarterNote);
            this.EnableOkButton();
        }

        private void TimeCodeBasedTimeRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableDivisionControls(); }

        private void FramesPerSecondComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { this.EnableOkButton(); }

        private void TicksPerFrameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ValidateNumericInput(this.TicksPerFrameLabel, byte.MaxValue, ref this.TicksPerFrame);
            this.EnableOkButton();
        }

        #endregion

        private void EnableDivisionControls()
        {
            bool b = (this.TimeCodeBasedTimeRadioButton.IsChecked == true);
            this.TicksPerQuarterNoteTextBox.IsEnabled = !b;
            this.TicksPerQuarterNoteLabel.IsEnabled = !b;
            this.FramesPerSecondComboBox.IsEnabled = b;
            this.FramesPerSecondLabel.IsEnabled = b;
            this.TicksPerFrameTextBox.IsEnabled = b;
            this.TicksPerFrameLabel.IsEnabled = b;
            this.EnableOkButton();
        }

        private void ValidateNumericInput(Label label, uint max, ref uint value)
        {
            uint n;

            /* Allow null/empty value. */
            TextBox textBox = label.Target as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                /* Revert to initial value. */
                value = uint.MaxValue;
                textBox.Text = null;
                return;
            }

            /* If the user entered something non-numeric, it's invalid. */
            if (!uint.TryParse(textBox.Text, out n))
            {
                /* Revert text to previous value. */
                textBox.Text = (value < uint.MaxValue) ? value.ToString() : null;
                return;
            }

            /* If the user entered a valid number, assign the new value and we're done. */
            if (n < max) { value = n; return; }

            /* Give the user a message for an invalid number. */
            string s = Text.ParseLabel(label.Content as string);
            s = string.Format(Properties.Resources.ValueRangeFormat, s, 0, max);
            MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            /* Revert text to previous value. */
            textBox.Text = (value < uint.MaxValue) ? value.ToString() : null;
        }

        private void EnableOkButton() { this.OkButton.IsEnabled = this.CheckRequiredInput(); }

        private bool CheckRequiredInput()
        {
            /* Format and Number of tracks are required. */
            if (this.FormatComboBox.SelectedIndex < 0) return false;
            if (this.NumberOfTracks == uint.MaxValue) return false;

            /* Either Metrical time or Time-code-based time must be checked.
             * Whichever one it is, its corresponding input is required.
             */
            if (this.MetricalTimeRadioButton.IsChecked == true)
            {
                if (this.TicksPerQuarterNote == uint.MaxValue) return false;
            }
            else if (this.TimeCodeBasedTimeRadioButton.IsChecked == true)
            {
                if (this.FramesPerSecondComboBox.SelectedIndex < 0) return false;
                if (this.TicksPerFrame == uint.MaxValue) return false;
            }
            else return false;

            /* All required input has been provided. */
            return true;
        }

        #endregion
    }
}
