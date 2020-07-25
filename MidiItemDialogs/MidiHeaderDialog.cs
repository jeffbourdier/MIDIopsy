/* MidiHeaderDialog.cs - Implementation of MidiHeaderDialog class, used for editing the header (MThd) chunk of a MIDI file.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* MessageBox, MessageBoxButton, MessageBoxImage, RoutedEventArgs, TextAlignment, Thickness */
using System.Windows;

/* ComboBox, GroupBox, Label, RadioButton, SelectionChangedEventArgs, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to edit the header (MThd) chunk of a MIDI file.</summary>
    public class MidiHeaderDialog : MidiItemDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static MidiHeaderDialog()
        {
            /* Initialize array containing valid string values for frames per second. */
            MidiHeaderDialog.FramesPerSecondStrings = new string[MidiHeaderDialog.FramesPerSecondCount] {
                Properties.Resources.FramesPerSecond24, Properties.Resources.FramesPerSecond25,
                Properties.Resources.FramesPerSecond29, Properties.Resources.FramesPerSecond30 };

            /* Initialize array containing valid numeric values for frames per second. */
            MidiHeaderDialog.FramesPerSecondValues = new int[MidiHeaderDialog.FramesPerSecondCount];
            for (int i = 0; i < MidiHeaderDialog.FramesPerSecondCount; ++i)
                if (int.TryParse(MidiHeaderDialog.FramesPerSecondStrings[i], out MidiHeaderDialog.FramesPerSecondValues[i]))
                    continue;
                else MidiHeaderDialog.FramesPerSecondValues[i] = 29;
        }

        #endregion

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiHeaderDialog class.</summary>
        /// <param name="header">
        /// MidiHeader object representing the MIDI header (MThd) chunk to edit, or null to create a new one.
        /// </param>
        public MidiHeaderDialog(MidiHeader header)
            : base(header)
        {
            int i, j = 0;

            /* Initialize the "Format" label. */
            Label label = UI.CreateLabel(MarginType.Top, Properties.Resources.Format, true);

            /* Initialize the "Format" combo box. */
            this.FormatComboBox = new ComboBox();
            this.FormatComboBox.TabIndex = ++j;
            this.FormatComboBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.FormatComboBox.Items.Add(Properties.Resources.Format0);
            this.FormatComboBox.Items.Add(Properties.Resources.Format1);
            this.FormatComboBox.Items.Add(Properties.Resources.Format2);
            this.FormatComboBox.SelectionChanged += this.FormatComboBox_SelectionChanged;
            label.Target = this.FormatComboBox;
            this.InitialElement = this.FormatComboBox;

            /* Initialize the "Number of tracks" label. */
            this.NumberOfTracksLabel = UI.CreateLabel(MarginType.Standard, Properties.Resources.NumberOfTracks, true);

            /* Initialize the "Number of tracks" text box. */
            this.NumberOfTracksTextBox = new TextBox();
            this.NumberOfTracksTextBox.TabIndex = ++j;
            this.NumberOfTracksTextBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.NumberOfTracksTextBox.TextAlignment = TextAlignment.Right;
            this.NumberOfTracksTextBox.GotFocus += UI.TextBox_GotFocus;
            this.NumberOfTracksTextBox.TextChanged += this.NumberOfTracksTextBox_TextChanged;
            this.NumberOfTracksLabel.Target = this.NumberOfTracksTextBox;

            /* Initialize the "Metrical time" radio button. */
            this.MetricalTimeRadioButton = new RadioButton();
            this.MetricalTimeRadioButton.TabIndex = ++j;
            this.MetricalTimeRadioButton.Margin = new Thickness(UI.TripleSpace, UI.TripleSpace, UI.TripleSpace, UI.UnitSpace);
            this.MetricalTimeRadioButton.Content = Properties.Resources.MetricalTime;
            this.MetricalTimeRadioButton.Checked += this.MetricalTimeRadioButton_Checked;

            /* Initialize the "Ticks per quarter note" label. */
            this.TicksPerQuarterNoteLabel = UI.CreateLabel(MarginType.Indent, Properties.Resources.TicksPerQuarterNote, true);
            this.TicksPerQuarterNoteLabel.IsEnabled = false;

            /* Initialize the "Ticks per quarter note" text box. */
            this.TicksPerQuarterNoteTextBox = new TextBox();
            this.TicksPerQuarterNoteTextBox.TabIndex = ++j;
            this.TicksPerQuarterNoteTextBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            this.TicksPerQuarterNoteTextBox.TextAlignment = TextAlignment.Right;
            this.TicksPerQuarterNoteTextBox.IsEnabled = false;
            this.TicksPerQuarterNoteTextBox.GotFocus += UI.TextBox_GotFocus;
            this.TicksPerQuarterNoteTextBox.TextChanged += this.TicksPerQuarterNoteTextBox_TextChanged;
            this.TicksPerQuarterNoteLabel.Target = this.TicksPerQuarterNoteTextBox;

            /* Initialize the "Time-code-based time" radio button. */
            this.TimeCodeBasedTimeRadioButton = new RadioButton();
            this.TimeCodeBasedTimeRadioButton.TabIndex = ++j;
            this.TimeCodeBasedTimeRadioButton.Margin =
                new Thickness(UI.TripleSpace, UI.DoubleSpace, UI.TripleSpace, UI.UnitSpace);
            this.TimeCodeBasedTimeRadioButton.Content = Properties.Resources.TimeCodeBasedTime;
            this.TimeCodeBasedTimeRadioButton.Checked += this.TimeCodeBasedTimeRadioButton_Checked;

            /* Initialize the "Frames per second" label. */
            this.FramesPerSecondLabel = UI.CreateLabel(MarginType.Indent, Properties.Resources.FramesPerSecond, true);
            this.FramesPerSecondLabel.IsEnabled = false;

            /* Initialize the "Frames per second" combo box. */
            this.FramesPerSecondComboBox = new ComboBox();
            this.FramesPerSecondComboBox.TabIndex = ++j;
            this.FramesPerSecondComboBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            for (i = 0; i < MidiHeaderDialog.FramesPerSecondCount; ++i)
                this.FramesPerSecondComboBox.Items.Add(MidiHeaderDialog.FramesPerSecondStrings[i]);
            this.FramesPerSecondComboBox.IsEnabled = false;
            this.FramesPerSecondComboBox.SelectionChanged += this.FramesPerSecondComboBox_SelectionChanged;
            this.FramesPerSecondLabel.Target = this.FramesPerSecondComboBox;

            /* Initialize the "Ticks per frame" label. */
            this.TicksPerFrameLabel = UI.CreateLabel(MarginType.Indent, Properties.Resources.TicksPerFrame, true);
            this.TicksPerFrameLabel.IsEnabled = false;

            /* Initialize the "Ticks per frame" text box. */
            this.TicksPerFrameTextBox = new TextBox();
            this.TicksPerFrameTextBox.TabIndex = ++j;
            this.TicksPerFrameTextBox.Margin = new Thickness(UI.IndentSpace, UI.HalfSpace, UI.TripleSpace, UI.TripleSpace);
            this.TicksPerFrameTextBox.TextAlignment = TextAlignment.Right;
            this.TicksPerFrameTextBox.IsEnabled = false;
            this.TicksPerFrameTextBox.GotFocus += UI.TextBox_GotFocus;
            this.TicksPerFrameTextBox.TextChanged += this.TicksPerFrameTextBox_TextChanged;
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
            GroupBox groupBox = new GroupBox();
            groupBox.Header = Properties.Resources.DeltaTimeDivision;
            groupBox.Content = stackPanel;
            groupBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);

            /* Build out the window and its content. */
            this.AddUIElement(label);
            this.AddUIElement(this.FormatComboBox);
            this.AddUIElement(this.NumberOfTracksLabel);
            this.AddUIElement(this.NumberOfTracksTextBox);
            this.AddUIElement(groupBox);
            this.BuildOut(320, Properties.Resources.Midi + " " + Properties.Resources.Header);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;

            /* If a MidiHeader object was supplied, use it to set initial values. */
            if (this.ForNewItem) return;
            this.NumberOfTracksTextBox.IsEnabled = false;
            this.NumberOfTracksLabel.IsEnabled = false;
            this.NumberOfTracksTextBox.Text = header.NumberOfTracks.ToString();
            this.FormatComboBox.SelectedIndex = header.Format;
            if (header.TicksPerQuarterNote < 0)
            {
                this.TimeCodeBasedTimeRadioButton.IsChecked = true;
                for (i = 0; i < MidiHeaderDialog.FramesPerSecondCount; ++i)
                    if (MidiHeaderDialog.FramesPerSecondValues[i] == header.FramesPerSecond)
                    { this.FramesPerSecondComboBox.SelectedIndex = i; break; }
                this.TicksPerFrameTextBox.Text = header.TicksPerFrame.ToString();
            }
            else
            {
                this.MetricalTimeRadioButton.IsChecked = true;
                this.TicksPerQuarterNoteTextBox.Text = header.TicksPerQuarterNote.ToString();
            }
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        /// <summary>
        /// The number of standard SMPTE and MIDI time code formats representing the number of frames per second.
        /// </summary>
        private const int FramesPerSecondCount = 4;

        /// <summary>Array containing valid string values for frames per second.</summary>
        private static readonly string[] FramesPerSecondStrings;

        /// <summary>Array containing valid numeric values for frames per second.</summary>
        private static readonly int[] FramesPerSecondValues;

        private ComboBox FormatComboBox;
        private Label NumberOfTracksLabel;
        private TextBox NumberOfTracksTextBox;
        private RadioButton MetricalTimeRadioButton;
        private Label TicksPerQuarterNoteLabel;
        private TextBox TicksPerQuarterNoteTextBox;
        private RadioButton TimeCodeBasedTimeRadioButton;
        private Label FramesPerSecondLabel;
        private ComboBox FramesPerSecondComboBox;
        private Label TicksPerFrameLabel;
        private TextBox TicksPerFrameTextBox;
        private int _NumberOfTracks = -1;
        private int _TicksPerQuarterNote = -1;
        private int _TicksPerFrame = -1;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Specifies the overall organization of the MIDI file.</summary>
        public int Format { get { return this.FormatComboBox.SelectedIndex; } }

        /// <summary>Number of track chunks in the MIDI file.</summary>
        public int NumberOfTracks { get { return this._NumberOfTracks; } }

        /// <summary>Number of delta-time "ticks" which make up a quarter-note.</summary>
        /// <remarks>
        /// This applies only for metrical time.  For time-code-based time, FramesPerSecond and
        /// TicksPerFrame apply, and TicksPerQuarterNote is -1 (to indicate that it does not apply).
        /// </remarks>
        public int TicksPerQuarterNote
        { get { return (this.MetricalTimeRadioButton.IsChecked == true) ? this._TicksPerQuarterNote : -1; } }

        /// <summary>Standard SMPTE and MIDI time code format representing the number of frames per second.</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are -1 (to indicate that they do not apply).
        /// </remarks>
        public int FramesPerSecond
        {
            get
            {
                return (this.TimeCodeBasedTimeRadioButton.IsChecked == true) ?
                    MidiHeaderDialog.FramesPerSecondValues[this.FramesPerSecondComboBox.SelectedIndex] : -1;
            }
        }

        /// <summary>Resolution within a frame (ticks per frame).</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are -1 (to indicate that they do not apply).
        /// </remarks>
        public int TicksPerFrame
        { get { return (this.TimeCodeBasedTimeRadioButton.IsChecked == true) ? this._TicksPerFrame : -1; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            /* Format and Number of tracks are required. */
            if (this.FormatComboBox.SelectedIndex < 0) return false;
            if (this.NumberOfTracks < 0) return false;

            /* Either Metrical time or Time-code-based time must be checked.
             * Whichever one it is, its corresponding input is required.
             */
            if (this.MetricalTimeRadioButton.IsChecked == true)
            {
                if (this.TicksPerQuarterNote < 0) return false;
            }
            else if (this.TimeCodeBasedTimeRadioButton.IsChecked == true)
            {
                if (this.FramesPerSecondComboBox.SelectedIndex < 0) return false;
                if (this.TicksPerFrame < 0) return false;
            }
            else return false;

            /* All required input has been provided. */
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FormatComboBox.SelectedIndex == 0)
            {
                /* If we're editing an existing header and there's already
                 * more than 1 track, tell the user only 1 track is allowed.
                 */
                if (!this.ForNewItem && this.NumberOfTracks > 1)
                {
                    MessageBox.Show(this, Properties.Resources.OneTrack, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Hand);
                    this.FormatComboBox.SelectedIndex = (int)this.Format;
                    return;
                }

                /* Number of tracks will always be 1 for a format 0 file. */
                this.NumberOfTracksTextBox.IsEnabled = false;
                this.NumberOfTracksLabel.IsEnabled = false;
                this.NumberOfTracksTextBox.Text = "1";
            }
            else if (this.ForNewItem) this.NumberOfTracksLabel.IsEnabled = this.NumberOfTracksTextBox.IsEnabled = true;
            this.EnableOkButton();
        }

        private void NumberOfTracksTextBox_TextChanged(object sender, TextChangedEventArgs e)
        { this.ValidateNumericInput(this.NumberOfTracksLabel, ref this._NumberOfTracks, 1, ushort.MaxValue); }

        private void MetricalTimeRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableDivisionControls(); }

        private void TicksPerQuarterNoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        { this.ValidateNumericInput(this.TicksPerQuarterNoteLabel, ref this._TicksPerQuarterNote, 0, short.MaxValue); }

        private void TimeCodeBasedTimeRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableDivisionControls(); }

        private void FramesPerSecondComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { this.EnableOkButton(); }

        private void TicksPerFrameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        { this.ValidateNumericInput(this.TicksPerFrameLabel, ref this._TicksPerFrame, 0, byte.MaxValue); }

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

        #endregion
    }
}
