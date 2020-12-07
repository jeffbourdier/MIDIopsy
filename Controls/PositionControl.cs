/* PositionControl.cs - Implementation of PositionControl class, used to display/edit a media playback position.
 *
 * Copyright (c) 2019-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* TimeSpan */
using System;

/* FontWeights, GridLength, HorizontalAlignment, RoutedEventArgs, TextAlignment, UIElement */
using System.Windows;

/* ColumnDefinition, Grid, Label, RowDefinition, TextBox, TextChangedEventArgs, UserControl */
using System.Windows.Controls;

/* Brush, SystemColors */
using System.Windows.Media;


namespace JeffBourdier
{
    /// <summary>Represents a control that can be used to display/edit a media playback position.</summary>
    public class PositionControl : UserControl
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the PositionControl object.</summary>
        public PositionControl()
        {
            /* This grid will serve as the content for the control. */
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 5; ++i)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                /* Even-numbered columns are for hours/minutes/seconds. */
                int n;
                if (i % 2 == 0)
                {
                    n = 34;
                    TextBox textBox = new TextBox();
                    textBox.Text = "00";
                    textBox.TextAlignment = TextAlignment.Right;
                    textBox.GotFocus += UI.TextBox_GotFocus;
                    Grid.SetColumn(textBox, i);
                    grid.Children.Add(textBox);
                }
                /* Odd-numbered columns are for colons between hours/minutes/seconds. */
                else
                {
                    n = UI.DoubleSpace;
                    Label label = UI.CreateLabel(MarginType.None, string.Empty, true);
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(label, i);
                    grid.Children.Add(label);
                }
                grid.ColumnDefinitions[i].Width = new GridLength(n);
            }
            this.Content = grid;

            /* Assign text boxes and event handlers for hours, minutes, and seconds. */
            this.HoursTextBox = grid.Children[0] as TextBox;
            this.HoursTextBox.TextChanged += this.HoursTextBox_TextChanged;
            this.MinutesTextBox = grid.Children[2] as TextBox;
            this.MinutesTextBox.TextChanged += this.MinutesTextBox_TextChanged;
            this.SecondsTextBox = grid.Children[4] as TextBox;
            this.SecondsTextBox.TextChanged += this.SecondsTextBox_TextChanged;

            /* Handle the Loaded event to set tab indexes. */
            this.Loaded += this.PositionControl_Loaded;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private TextBox HoursTextBox;
        private TextBox MinutesTextBox;
        private TextBox SecondsTextBox;
        private bool NoValidation = false;
        private TimeSpan _Position = TimeSpan.Zero;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        public UIElement InitialElement { get { return this.HoursTextBox; } }

        /// <summary>Gets or sets the media playback position represented by this control.</summary>
        public TimeSpan Position
        {
            get { return this._Position; }

            set
            {
                this.NoValidation = true;
                this.HoursTextBox.Text = value.Hours.ToString("00");
                this.MinutesTextBox.Text = value.Minutes.ToString("00");
                this.SecondsTextBox.Text = value.Seconds.ToString("00");
                this.NoValidation = false;
                this._Position = value;
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        public void Highlight(Brush brush)
        {
            this.SecondsTextBox.FontWeight = FontWeights.Bold;
            this.SecondsTextBox.Background = brush;
            this.MinutesTextBox.FontWeight = FontWeights.Bold;
            this.MinutesTextBox.Background = brush;
            this.HoursTextBox.FontWeight = FontWeights.Bold;
            this.HoursTextBox.Background = brush;
        }

        public void Unhighlight(object unused)
        {
            this.HoursTextBox.Background = SystemColors.WindowBrush;
            this.HoursTextBox.FontWeight = FontWeights.Normal;
            this.MinutesTextBox.Background = SystemColors.WindowBrush;
            this.MinutesTextBox.FontWeight = FontWeights.Normal;
            this.SecondsTextBox.Background = SystemColors.WindowBrush;
            this.SecondsTextBox.FontWeight = FontWeights.Normal;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void PositionControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.TabIndex == int.MaxValue) return;
            int i = this.TabIndex;
            this.HoursTextBox.TabIndex = ++i;
            this.MinutesTextBox.TabIndex = ++i;
            this.SecondsTextBox.TabIndex = ++i;
        }

        private void HoursTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n = this.Position.Hours;
            if (!this.ValidateNumericInput(this.HoursTextBox, ref n, 23, Properties.Resources.Hour)) return;
            this._Position = new TimeSpan(n, this.Position.Minutes, this.Position.Seconds);
        }

        private void MinutesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n = this.Position.Minutes;
            if (!this.ValidateNumericInput(this.MinutesTextBox, ref n, 59, Properties.Resources.Minute)) return;
            this._Position = new TimeSpan(this.Position.Hours, n, this.Position.Seconds);
        }

        private void SecondsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n = this.Position.Seconds;
            if (!this.ValidateNumericInput(this.SecondsTextBox, ref n, 59, Properties.Resources.Second)) return;
            this._Position = new TimeSpan(this.Position.Hours, this.Position.Minutes, n);
        }

        #endregion

        private bool ValidateNumericInput(TextBox textBox, ref int value, int max, string description)
        {
            /* Prevent unnecessary recursion. */
            if (this.NoValidation) return false;
            this.NoValidation = true;

            /* If the text box is null/empty, revert to zero. */
            if (string.IsNullOrEmpty(textBox.Text))
            {
                value = 0;
                textBox.Text = "00";
                this.NoValidation = false;
                return true;
            }

            /* Otherwise, normal validation rules apply. */
            int n = value;
            bool b = UI.ValidateNumericInput(textBox, ref n, 0, max, description + "s");
            if (b) value = n;

            /* Number should be two digits. */
            while (textBox.Text.Length > 2) if (textBox.Text.StartsWith("0")) textBox.Text = textBox.Text.Substring(1);
            while (textBox.Text.Length < 2) textBox.Text = "0" + textBox.Text;

            /* Put caret at end of text box. */
            textBox.CaretIndex = textBox.Text.Length;
            this.NoValidation = false;
            return b;
        }

        #endregion
    }
}
