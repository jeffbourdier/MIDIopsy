/* GoToDialog.cs - Implementation of GoToDialog class, used for jumping to a specific place in a MIDI file.
 *
 * Copyright (c) 2020 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* TimeSpan */
using System;

/* CancelEventArgs */
using System.ComponentModel;

/* MessageBox, MessageBoxButton, MessageBoxImage, RoutedEventArgs, TextAlignment, Thickness */
using System.Windows;

/* ComboBox, Label, RadioButton, SelectionChangedEventArgs, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that is used to jump to a specific place in a MIDI file.</summary>
    public class GoToDialog : StandardDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the GoToDialog class.</summary>
        /// <param name="trackNames">Array of strings representing track names.  (Empty is valid; null is ignored.)</param>
        /// <param name="totalTime">Maximum total (cumulative) time in the MIDI file, in ticks.</param>
        /// <param name="duration">Duration of the MIDI file, or TimeSpan.MinValue if no duration.</param>
        public GoToDialog(string[] trackNames, int totalTime, TimeSpan duration)
        {
            int i, j = 0;

            /* Initialize the "Track" label. */
            Label label = UI.CreateLabel(MarginType.Top, Properties.Resources.Track, true);

            /* Initialize the "Track" combo box. */
            this.TrackComboBox = new ComboBox();
            this.TrackComboBox.TabIndex = ++j;
            this.TrackComboBox.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);
            for (i = 0; i < trackNames.Length; ++i)
            {
                if (trackNames[i] == null) continue;
                string s = (i + 1).ToString();
                if (trackNames[i] != string.Empty) s += string.Format(" ({0})", trackNames[i]);
                this.TrackComboBox.Items.Add(s);
            }
            this.TrackComboBox.SelectionChanged += this.TrackComboBox_SelectionChanged;
            label.Target = this.TrackComboBox;
            this.InitialElement = this.TrackComboBox;

            /* Initialize the "Total time" radio button. */
            this.TotalTimeRadioButton = new RadioButton();
            this.TotalTimeRadioButton.TabIndex = ++j;
            this.TotalTimeRadioButton.Margin = new Thickness(UI.TripleSpace, UI.DoubleSpace, UI.TripleSpace, UI.UnitSpace);
            this.TotalTimeRadioButton.Content = string.Format("_{0} ({1}):",
                GoToDialog.TotalTimeString, Properties.Resources.Ticks);
            this.TotalTimeRadioButton.Checked += this.TotalTimeRadioButton_Checked;

            /* Initialize the "Total time" text box. */
            this.TotalTimeTextBox = new TextBox();
            this.TotalTimeTextBox.TabIndex = ++j;
            this.TotalTimeTextBox.Margin = new Thickness(UI.IndentSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);
            this.TotalTimeTextBox.TextAlignment = TextAlignment.Right;
            this.TotalTimeTextBox.Text = "0";
            this.TotalTimeTextBox.IsEnabled = false;
            this.TotalTimeTextBox.GotFocus += UI.TextBox_GotFocus;
            this.TotalTimeTextBox.TextChanged += TotalTimeTextBox_TextChanged;

            /* Initialize the "Position" radio button. */
            this.PositionRadioButton = new RadioButton();
            this.PositionRadioButton.TabIndex = ++j;
            this.PositionRadioButton.Margin = new Thickness(UI.TripleSpace, UI.DoubleSpace, UI.TripleSpace, UI.UnitSpace);
            this.PositionRadioButton.Content = Properties.Resources.Position + ":";
            if (duration == TimeSpan.MinValue) this.PositionRadioButton.IsEnabled = false;
            else this.PositionRadioButton.Checked += this.PositionRadioButton_Checked;

            /* Initialize the position control. */
            this.PositionControl = new PositionControl();
            this.PositionControl.TabIndex = ++j;
            this.PositionControl.Margin = new Thickness(UI.IndentSpace, UI.UnitSpace, UI.TripleSpace, UI.TripleSpace);
            this.PositionControl.IsEnabled = false;

            /* Build out the window and its content. */
            this.AddUIElement(label);
            this.AddUIElement(this.TrackComboBox);
            this.AddUIElement(this.TotalTimeRadioButton);
            this.AddUIElement(this.TotalTimeTextBox);
            this.AddUIElement(this.PositionRadioButton);
            this.AddUIElement(this.PositionControl);
            this.BuildOut(320, Properties.Resources.GoTo);

            /* The OK button should start out disabled and stay that way until all required input is entered. */
            this.OkButton.IsEnabled = false;

            /* Set upper limits on total time and (if applicable) position. */
            this.MaxTotalTime = totalTime;
            if (duration != TimeSpan.MinValue) this.Duration = duration;

            /* Handle the Closing event to validate the position (if applicable). */
            this.Closing += this.GoToDialog_Closing;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static readonly string TotalTimeString = Text.ChangeCase(Properties.Resources.TotalTime, TextCase.Sentence);

        private ComboBox TrackComboBox;
        private RadioButton TotalTimeRadioButton;
        private TextBox TotalTimeTextBox;
        private RadioButton PositionRadioButton;
        private PositionControl PositionControl;
        private int MaxTotalTime;
        private TimeSpan Duration;
        private int _TotalTime = 0;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Track number entered by the user.</summary>
        public int TrackNumber { get { return this.TrackComboBox.SelectedIndex + 1; } }

        /// <summary>Total (cumulative) time (in ticks) entered by the user (if applicable).</summary>
        public int TotalTime { get { return (this.TotalTimeRadioButton.IsChecked == true) ? this._TotalTime : -1; } }

        /// <summary>Position entered by the user (if applicable).</summary>
        public TimeSpan Position { get { return this.PositionControl.Position; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            /* Track is required. */
            if (this.TrackComboBox.SelectedIndex < 0) return false;

            /* Either Total time or Position must be checked. */
            if (this.TotalTimeRadioButton.IsChecked == false && this.PositionRadioButton.IsChecked == false) return false;

            /* All required input has been provided. */
            return true;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void TrackComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { this.EnableOkButton(); }

        private void TotalTimeRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableControls(); }

        private void TotalTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UI.ValidateNumericInput(this.TotalTimeTextBox, ref this._TotalTime,
                0, this.MaxTotalTime, GoToDialog.TotalTimeString);
        }

        private void PositionRadioButton_Checked(object sender, RoutedEventArgs e) { this.EnableControls(); }

        /* Validate the position (if applicable). */
        private void GoToDialog_Closing(object sender, CancelEventArgs e)
        {
            if (this.DialogResult != true || this.PositionRadioButton.IsChecked == false ||
                this.PositionControl.Position <= this.Duration) return;
            string s = Text.ParseLabel(Properties.Resources.Position);
            s = string.Format(Common.Resources.ValueRangeFormat, s, TimeSpan.Zero, this.Duration);
            MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            e.Cancel = true;
        }

        #endregion

        private void EnableControls()
        {
            bool b = (this.PositionRadioButton.IsChecked == true);
            this.TotalTimeTextBox.IsEnabled = !b;
            this.PositionControl.IsEnabled = b;
            this.EnableOkButton();
        }

        #endregion
    }
}
