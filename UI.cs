/* UI.cs - Implementation of UI class, which makes certain UI-related tasks easier.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* DependencyObject, MessageBox, MessageBoxButton, MessageBoxImage, RoutedEventArgs, Thickness, Window */
using System.Windows;

/* CheckBox, FrameworkElement, Label, TextBox */
using System.Windows.Controls;

/* FontFamily */
using System.Windows.Media;


namespace JeffBourdier
{
    /// <summary>Specifies enumerated constants to define margin types.</summary>
    public enum MarginType { None, Standard, Top, Indent }

    /// <summary>Provides constants and methods that make certain UI-related tasks easier.</summary>
    public static class UI
    {
        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>The standard default width for the client area of a window.</summary>
        public const int ClientWidth = 480;

        /// <summary>
        /// The standard number of pixels between controls that are closely related (e.g., a label and its target).
        /// </summary>
        public const int UnitSpace = 4;

        /// <summary>Half of UI.UnitSpace.</summary>
        public const int HalfSpace = UI.UnitSpace / 2;

        /// <summary>The standard number of pixels between controls that are not closely related.</summary>
        public const int DoubleSpace = UI.UnitSpace * 2;

        /// <summary>The standard number of pixels between the edge of a window and the nearest control.</summary>
        public const int TripleSpace = UI.UnitSpace * 3;

        /// <summary>
        /// The standard number of pixels between the edge of a window and a control that should appear "indented."
        /// </summary>
        public const int IndentSpace = 7 * UI.UnitSpace;

        /// <summary>The standard width of a button (in pixels).</summary>
        public const int ButtonWidth = 20 * UI.UnitSpace;

        /// <summary>The standard height of a button (in pixels).</summary>
        public const int ButtonHeight = 10 * UI.UnitSpace;

        /// <summary>The standard monospace font.</summary>
        public static readonly FontFamily MonospaceFont = new FontFamily("Courier New");

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of an OK button by closing the parent window and setting the dialog result to true.
        /// </summary>
        /// <param name="sender">The element where the handler is being invoked.</param>
        /// <param name="e">Contains information about the routed event.</param>
        public static void OkButton_Click(object sender, RoutedEventArgs e)
        {
            /* Set the dialog result to true and close the parent window. */
            Window window = Window.GetWindow(sender as DependencyObject);
            window.DialogResult = true;
            window.Close();
        }

        /// <summary>Handles the GotFocus event of a text box by selecting all of its contents.</summary>
        /// <param name="sender">The element where the handler is being invoked.</param>
        /// <param name="e">Contains information about the routed event.</param>
        public static void TextBox_GotFocus(object sender, RoutedEventArgs e) { (sender as TextBox).SelectAll(); }

        #endregion

        /// <summary>Creates a standardized label.</summary>
        /// <param name="marginType">Specifies what type of margin to give the label.</param>
        /// <param name="content">The content of the label (a colon is appended).</param>
        /// <param name="fit">True if the label is to be fit to its content; otherwise, false.</param>
        /// <returns>A new instance of the Label class.</returns>
        public static Label CreateLabel(MarginType marginType, object content, bool fit)
        {
            Label label = new Label();
            label.Content = content + ":";
            if (marginType != MarginType.None) label.Margin = UI.CreateMargin(marginType, UI.HalfSpace);
            if (fit)
            {
                label.Padding = new Thickness();
                label.Height = label.FontSize * 3 / 2;
            }
            return label;
        }

        /// <summary>Creates a standardized check box.</summary>
        /// <param name="tabIndex">
        /// Determines the order in which the check box receives focus
        /// when the user navigates through controls by using the TAB key.
        /// </param>
        /// <param name="marginType">Specifies what type of margin to give the check box.</param>
        /// <param name="content">The content of the check box.</param>
        /// <param name="check">If non-null, determines whether or not the box is checked.</param>
        /// <returns>A new instance of the CheckBox class.</returns>
        public static CheckBox CreateCheckBox(int tabIndex, MarginType marginType, object content, bool? check)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.TabIndex = tabIndex;
            if (marginType != MarginType.None) checkBox.Margin = UI.CreateMargin(marginType, UI.UnitSpace);
            checkBox.Content = content;
            if (check != null) checkBox.IsChecked = check;
            return checkBox;
        }

        /// <summary>Determine whether or not a text box contains a valid number (integer) within a range.</summary>
        /// <param name="textBox">A TextBox object.</param>
        /// <param name="value">
        /// Previous numeric value, to which the text box is reset if it does not contain a valid number.
        /// If the text box does contain a valid number, receives the numeric value entered in therein.
        /// </param>
        /// <param name="min">Minimum value for the number.</param>
        /// <param name="max">Maximum value for the number.</param>
        /// <param name="description">
        /// A short description of the numeric value (for display purposes).  May be label content.
        /// </param>
        /// <returns>True if the text box contains a valid number within the given range; otherwise, false.</returns>
        public static bool ValidateNumericInput(TextBox textBox, ref int value, int min, int max, string description)
        {
            int n;
            FrameworkElement element;

            /* Remove leading and trailing white-space. */
            textBox.Text = textBox.Text.Trim();

            /* If the user entered something non-numeric, it's invalid. */
            if (!int.TryParse(textBox.Text, out n))
            {
                /* Revert text to previous value. */
                textBox.Text = value.ToString();
                return false;
            }

            /* If the user entered a valid number, assign the new value and we're done. */
            if (n >= min && n <= max) { value = n; return true; }

            /* Give the user a message for an invalid number. */
            string s = UI.ParseLabel(description);
            s = string.Format(Properties.Resources.ValueRangeFormat, s, min, max);
            for (element = textBox; element.Parent != null; element = element.Parent as FrameworkElement) ;
            MessageBox.Show(element as Window, s, MIDIopsyApp.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            /* Revert text to previous value. */
            textBox.Text = value.ToString();
            return false;
        }

        /// <summary>
        /// Parses a string meant to serve as the content of a label.  If the string contains parentheses, the text
        /// within the parentheses is returned.  Otherwise, the string with underscores and colons removed is returned.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A copy of the string s with underscores and colons removed.</returns>
        public static string ParseLabel(string s)
        {
            int i = s.IndexOf('(') + 1;
            int n = s.LastIndexOf(')') - i;
            if (i > 0 && n > 0) return s.Substring(i, n);
            return s.Replace("_", null).Replace(":", null);
        }

        #endregion

        #region Private Methods

        private static Thickness CreateMargin(MarginType marginType, double bottom)
        {
            return new Thickness(
                (marginType == MarginType.Indent) ? UI.IndentSpace : UI.TripleSpace,
                (marginType == MarginType.Top) ? UI.TripleSpace : UI.UnitSpace,
                UI.TripleSpace, bottom);
        }

        #endregion
    }
}
