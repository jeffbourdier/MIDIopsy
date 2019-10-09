/* UI.cs - Implementation of UI class, which makes certain UI-related tasks easier.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Window, RoutedEventArgs, DependencyObject, UIElement */
using System.Windows;

/* Panel, Control */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Provides constants and methods that make certain UI-related tasks easier.</summary>
    public static class UI
    {
        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>The standard default width for the client area of a window.</summary>
        public const int ClientWidth = 512;

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

        /// <summary>The maximum valid page width for a flow document.</summary>
        public const double MaxFlowDocPageWidth = 1000000;

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
            Window window = Window.GetWindow((DependencyObject)sender);
            window.DialogResult = true;
            window.Close();
        }

        #endregion

        /// <summary>
        /// Offsets the tab index of each child of a panel, starting at the specified index position, by the specified amount.
        /// </summary>
        /// <param name="panel">The panel whose children's tab indexes are to be offset.</param>
        /// <param name="index">The index position at which to start offsetting.</param>
        /// <param name="offset">The amount by which to offset the tab indexes.</param>
        public static void OffsetTabIndexes(Panel panel, int index, int offset)
        {
            /* Save ourselves a little time if the offset is zero. */
            if (offset == 0) return;

            /* Offset the tab index of each panel child, starting at the specified index position. */
            for (int i = index; i < panel.Children.Count; ++i)
            {
                Control control = panel.Children[i] as Control;
                if (control == null) continue;
                control.TabIndex += offset;
            }
        }

        /// <summary>
        /// Counts the number of childless elements ("leaf" nodes) contained within an
        /// element.  If the element contains no other elements, then the count is one.
        /// </summary>
        /// <param name="element">The element whose contained elements are counted.</param>
        /// <returns>The number of elements contained within the element.</returns>
        public static int CountElements(UIElement element)
        {
            /* If the element cannot contain other elements, then return one for the element itself. */
            Panel panel = element as Panel;
            if (panel == null) return 1;

            /* The element can contain other elements, so return the number of elements it contains. */
            int n = 0;
            foreach (UIElement child in panel.Children) n += UI.CountElements(child);
            return n;
        }

        #endregion
    }
}
