/* StandardDialog.cs - Implementation of StandardDialog class, which standardizes the way dialogs are created.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ResizeMode, SizeToContent, UIElement, Window, WindowStyle */
using System.Windows;

/* Button, StackPanel */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Extends the Window class by standardizing the way dialogs are created.</summary>
    public class StandardDialog : Window
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the StandardDialog class.</summary>
        public StandardDialog()
        {
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private StackPanel MainPanel = new StackPanel();
        private Button _OkButton;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the OK button (e.g., to disable/enable).</summary>
        public Button OkButton
        {
            get
            {
                /* If the field has already been set, return it. */
                if (this._OkButton != null) return this._OkButton;

                /* Set and return the field (if possible; otherwise, return null). */
                int n = this.MainPanel.Children.Count;
                if (n < 1) return null;
                OkCancelPanel panel = this.MainPanel.Children[--n] as OkCancelPanel;
                if (panel == null) return null;
                this._OkButton = panel.Children[1] as Button;
                return this._OkButton;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>Gets the number of controls on the main panel of the dialog.</summary>
        protected int ControlCount { get { return this.MainPanel.Children.Count; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Opens a window and returns only when the newly opened window is closed.</summary>
        /// <param name="owner">The window's top-level window and owner.</param>
        /// <returns>A NullableBoolean value that signifies how a window was closed by the user.</returns>
        public bool? ShowDialog(Window owner)
        {
            this.Owner = owner;
            return this.ShowDialog();
        }

        #endregion

        #region Protected Methods

        /// <summary>Adds a top-level UI element to the main stack panel.</summary>
        /// <param name="element">The UI element to add.</param>
        protected void AddUIElement(UIElement element) { this.MainPanel.Children.Add(element); }

        /// <summary>Inserts a UI element into the main stack panel at a specified index.</summary>
        /// <param name="index">The index at which to insert the UI element.</param>
        /// <param name="element">The UI element to insert.</param>
        protected void InsertUIElement(int index, UIElement element) { this.MainPanel.Children.Insert(index, element); }

        /// <summary>Removes from the main stack panel the UI element at a specified index.</summary>
        /// <param name="index">The index of the UI element to remove.</param>
        protected void RemoveUIElement(int index) { this.MainPanel.Children.RemoveAt(index); }

        /// <summary>
        /// Builds out the window by adding an area containing an OK button and a Cancel button
        /// to the main stack panel, setting the width of the main stack panel, setting the
        /// content of the window to the main stack panel, and setting the window's title.
        /// </summary>
        /// <param name="width">The width of the main stack panel, in device-independent units (1/96th inch per unit).</param>
        /// <param name="title">A string that contains the window's title.</param>
        protected void BuildOut(double width, string title)
        {
            this.MainPanel.Children.Add(new OkCancelPanel());
            this.MainPanel.Width = width;
            this.Content = this.MainPanel;
            this.Title = title;
        }

        #endregion
    }
}
