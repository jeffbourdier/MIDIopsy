/* DialogWindow.cs - Implementation of DialogWindow class, which standardizes the way dialogs are created.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* IInputElement, ResizeMode, SizeToContent, Thickness, UIElement, Window, WindowStyle */
using System.Windows;

/* Button, Dock, DockPanel, StackPanel */
using System.Windows.Controls;

/* FocusManager */
using System.Windows.Input;


namespace JeffBourdier
{
    /// <summary>Extends the Window class by standardizing the way dialogs are created.</summary>
    public class DialogWindow : Window
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the DialogWindow class.</summary>
        public DialogWindow()
        {
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected IInputElement InitialElement = null;

        #endregion

        #region Private Fields

        private StackPanel MainPanel = new StackPanel();
        private Button _OkButton;

        #endregion

        /**************
         * Properties *
         **************/

        #region Protected Properties

        /// <summary>Gets the number of controls on the main panel of the dialog.</summary>
        protected int ControlCount { get { return this.MainPanel.Children.Count; } }

        /// <summary>Gets the OK button (e.g., to disable/enable).</summary>
        protected Button OkButton { get { return this._OkButton; } }

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
            if (this.InitialElement != null) FocusManager.SetFocusedElement(this, this.InitialElement);
            return this.ShowDialog();
        }

        #endregion

        #region Protected Methods

        /// <summary>Adds a top-level UI element to the main stack panel.</summary>
        /// <param name="element">The UI element to add.</param>
        protected void AddUIElement(UIElement element) { this.MainPanel.Children.Add(element); }

        /// <summary>
        /// Builds out the window by adding an area containing an OK button and a Cancel button
        /// to the main stack panel, setting the width of the main stack panel, setting the
        /// content of the window to the main stack panel, and setting the window's title.
        /// </summary>
        /// <param name="width">The width of the main stack panel, in device-independent units (1/96th inch per unit).</param>
        /// <param name="title">A string that contains the window's title.</param>
        protected void BuildOut(double width, string title)
        {
            DockPanel panel = new DockPanel();
            panel.LastChildFill = false;
            DialogWindow.InitializeButton(panel, false);
            DialogWindow.InitializeButton(panel, true);
            this._OkButton = panel.Children[1] as Button;
            this.MainPanel.Children.Add(panel);
            this.MainPanel.Width = width;
            this.Content = this.MainPanel;
            this.Title = title;
        }

        /// <summary>Enables or disables the OK button based on whether or not the required input has been entered.</summary>
        /// <remarks>This method calls CheckRequiredInput, which can be overridden in derived classes.</remarks>
        protected void EnableOkButton() { this.OkButton.IsEnabled = this.CheckRequiredInput(); }

        /// <summary>
        /// When overridden in a derived class, determines whether or not the required input has been entered.
        /// </summary>
        /// <returns>True if the required input has been entered; otherwise, false.</returns>
        /// <remarks>This method is called by EnableOkButton.</remarks>
        protected virtual bool CheckRequiredInput() { return true; }

        #endregion

        #region Private Methods

        /// <summary>Adds an OK or Cancel button to a dock panel.</summary>
        /// <param name="panel">The dock panel to which the button is added.</param>
        /// <param name="ok">True for an OK button; otherwise (for a Cancel button), false.</param>
        private static void InitializeButton(DockPanel panel, bool ok)
        {
            /* These attributes are common to both buttons (mostly). */
            Button button = new Button();
            button.Width = UI.ButtonWidth;
            button.Height = UI.ButtonHeight;
            button.Margin = new Thickness(UI.UnitSpace, UI.DoubleSpace, ok ? UI.UnitSpace : UI.TripleSpace, UI.TripleSpace);

            /* These attributes differ depending on the button. */
            if (ok)
            {
                button.Content = Properties.Resources.OK;
                button.Click += UI.OkButton_Click;
                button.IsDefault = true;
                button.TabIndex = int.MaxValue - 1;
            }
            else
            {
                button.Content = Properties.Resources.Cancel;
                button.IsCancel = true;
                button.TabIndex = int.MaxValue;
            }

            /* The rest applies to both buttons. */
            DockPanel.SetDock(button, Dock.Right);
            panel.Children.Add(button);
        }

        #endregion
    }
}
