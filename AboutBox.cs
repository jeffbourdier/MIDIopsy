/* AboutBox.cs - Implementation of AboutBox class, which displays information about the application.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Uri */
using System;

/* Process */
using System.Diagnostics;

/* Assembly, AssemblyCopyrightAttribute, AssemblyFileVersionAttribute, AssemblyProductAttribute */
using System.Reflection;

/* GridLength, ResizeMode, SizeToContent, Thickness, Window */
using System.Windows;

/* Button, ColumnDefinition, Grid, Image, RowDefinition, TextBlock */
using System.Windows.Controls;

/* Hyperlink, LineBreak */
using System.Windows.Documents;

/* FocusManager */
using System.Windows.Input;

/* RequestNavigateEventArgs */
using System.Windows.Navigation;


namespace JeffBourdier
{
    /// <summary>Displays an About box that contains information about the application.</summary>
    public static class AboutBox
    {
        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static AboutBox()
        {
            object[] objects;

            /* Retrieve the process executable in the default application domain (i.e., the entry assembly). */
            Assembly assembly = Assembly.GetEntryAssembly();

            /* Retrieve the product attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            AboutBox.Product = ((AssemblyProductAttribute)objects[0]).Product;

            /* Retrieve the version attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            AboutBox.Version = ((AssemblyFileVersionAttribute)objects[0]).Version;

            /* Retrieve the copyright attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AboutBox.Copyright = ((AssemblyCopyrightAttribute)objects[0]).Copyright;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>Predefined width of the banner to be displayed across the top of the About box.</summary>
        public const int BannerWidth = 512;

        /// <summary>Predefined height of the banner to be displayed across the top of the About box.</summary>
        public const int BannerHeight = 80;

        #endregion

        #region Private Fields

        private const int MarginLength = AboutBox.BannerHeight / 5;
        private static readonly string Product;
        private static readonly string Version;
        private static readonly string Copyright;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Displays an About box in front of the specified window.</summary>
        /// <param name="owner">The owner window of the About box.</param>
        public static void Show(Window owner)
        {
            /* Define the grid content panel. */
            Grid grid = new Grid();
            grid.Width = AboutBox.BannerWidth;
            grid.Height = 2 * AboutBox.BannerHeight;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions[0].Width = new GridLength(AboutBox.BannerWidth - AboutBox.BannerHeight);

            /* Initialize the banner image.  (Note: "Build Action" property must be set to "Resource") */
            Image image = new Image();
            image.Source = MIDIopsyApp.CreateBitmapSource("Banner.bmp");
            Grid.SetRow(image, 0);
            Grid.SetColumn(image, 0);
            Grid.SetColumnSpan(image, 2);
            grid.Children.Add(image);

            /* Initialize the info text block. */
            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(AboutBox.MarginLength, AboutBox.MarginLength / 2, 0, 0);
            textBlock.Inlines.Add(AboutBox.Product);
            textBlock.Inlines.Add(new LineBreak());
            string s = string.Format("{0} {1}", Properties.Resources.Version, AboutBox.Version);
            textBlock.Inlines.Add(s);
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(AboutBox.Copyright);
            textBlock.Inlines.Add(new LineBreak());
            Hyperlink hyperlink = new Hyperlink();
            hyperlink.NavigateUri = new Uri("https://jeffbourdier.github.io/" + MIDIopsyApp.Name.ToLower());
            hyperlink.Inlines.Add(hyperlink.NavigateUri.AbsoluteUri);
            hyperlink.RequestNavigate += AboutBox.Hyperlink_RequestNavigate;
            textBlock.Inlines.Add(hyperlink);
            Grid.SetRow(textBlock, 1);
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            /* Initialize the OK button. */
            Button button = new Button();
            button.Margin = new Thickness(AboutBox.MarginLength);
            button.Content = Properties.Resources.OK;
            button.Click += UI.OkButton_Click;
            button.IsCancel = true;
            button.IsDefault = true;
            Grid.SetRow(button, 1);
            Grid.SetColumn(button, 1);
            grid.Children.Add(button);

            /* Build and show the window. */
            Window window = new Window();
            window.WindowStyle = WindowStyle.ToolWindow;
            window.ResizeMode = ResizeMode.NoResize;
            window.Title = Properties.Resources.About + " " + MIDIopsyApp.Name;
            window.Content = grid;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Owner = owner;
            FocusManager.SetFocusedElement(window, button);
            window.ShowDialog();
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        { Process.Start(e.Uri.AbsoluteUri); }

        #endregion

        #endregion
    }
}
