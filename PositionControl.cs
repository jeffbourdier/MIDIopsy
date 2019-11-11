/* PositionControl.cs - Implementation of PositionControl class, used to display or edit a media playback position.
 *
 * Copyright (c) 2019 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* TimeSpan */
using System;

/* GridLength, TextAlignment */
using System.Windows;

/* UserControl, Grid, ColumnDefinition, RowDefinition, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a control that can be used to display or edit a media playback position.</summary>
    public class PositionControl : UserControl
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the PositionControl object.</summary>
        /// <param name="text">The content of the label for the position.</param>
        /// <param name="readOnly">Indicates whether or not the control should be read-only.</param>
        public PositionControl(string text, bool readOnly)
        {
            const double width = UI.IndentSpace;

            int i = -1;
            StandardLabel label;

            /* This grid will serve as the content for the control. */
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[++i].Width = new GridLength(width);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[++i].Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[++i].Width = new GridLength(width);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[++i].Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[++i].Width = new GridLength(width);
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions[0].Height = GridLength.Auto;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions[1].Height = GridLength.Auto;
            this.Content = grid;

            /* Add a label for the position to the top row, spanning all columns. */
            label = PositionControl.CreateLabel(text, 0, 0);
            label.Margin = new Thickness(0, UI.HalfSpace, 0, UI.UnitSpace);
            Grid.SetColumnSpan(label, 5);
            grid.Children.Add(label);

            /* Add a text box for hours (bottom row, left column). */
            i = -1;
            this.HoursTextBox = PositionControl.CreateTextBox(readOnly, ++i);
            this.HoursTextBox.TextChanged += this.HoursTextBox_TextChanged;
            grid.Children.Add(this.HoursTextBox);
            label.Target = this.HoursTextBox;

            /* Put a colon in between hours and minutes. */
            label = PositionControl.CreateLabel(string.Empty, 1, ++i);
            grid.Children.Add(label);

            /* Add a text box for minutes (bottom row, middle column). */
            this.MinutesTextBox = PositionControl.CreateTextBox(readOnly, ++i);
            this.MinutesTextBox.TextChanged += this.MinutesTextBox_TextChanged;
            grid.Children.Add(this.MinutesTextBox);

            /* Put a colon in between minutes and seconds. */
            label = PositionControl.CreateLabel(string.Empty, 1, ++i);
            grid.Children.Add(label);

            /* Add a text box for seconds (bottom row, right column). */
            this.SecondsTextBox = PositionControl.CreateTextBox(readOnly, ++i);
            this.SecondsTextBox.TextChanged += this.SecondsTextBox_TextChanged;
            grid.Children.Add(this.SecondsTextBox);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private TimeSpan _Position = new TimeSpan();

        private TextBox HoursTextBox;
        private TextBox MinutesTextBox;
        private TextBox SecondsTextBox;
        private bool NoValidation = false;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

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

        #region Private Methods

        #region Event Handlers

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

        private static StandardLabel CreateLabel(string text, int row, int column)
        {
            StandardLabel label = new StandardLabel(text + ":", true);
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            return label;
        }

        private static TextBox CreateTextBox(bool readOnly, int column)
        {
            TextBox textBox = new TextBox();
            textBox.Text = "00";
            textBox.IsReadOnly = readOnly;
            textBox.TextAlignment = TextAlignment.Right;
            textBox.GotFocus += UI.TextBox_GotFocus;
            Grid.SetRow(textBox, 1);
            Grid.SetColumn(textBox, column);
            return textBox;
        }

        private bool ValidateNumericInput(TextBox textBox, ref int value, uint max, string description)
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
            uint n = (uint)value;
            bool b = UI.ValidateNumericInput(textBox, ref n, max, description + "s");
            if (b) value = (int)n;

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
