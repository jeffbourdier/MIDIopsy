/* AppHelper.cs - Implementation of AppHelper class, which provides functionality related to the currently running application.
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


/* Type, Uri, UriKind */
using System;

/* Assembly, AssemblyTitleAttribute, AssemblyProductAttribute, AssemblyFileVersionAttribute, AssemblyCopyrightAttribute */
using System.Reflection;


namespace JeffBourdier
{
    /// <summary>Provides functionality related to the currently running application.</summary>
    public static class AppHelper
    {
        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static AppHelper()
        {
            object[] objects;

            /* Retrieve the process executable in the default application domain (i.e., the entry assembly). */
            Assembly assembly = Assembly.GetEntryAssembly();

            /* Attempt to retrieve the title attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            AppHelper.Title = (objects.Length < 1) ? string.Empty : ((AssemblyTitleAttribute)objects[0]).Title;

            /* Retrieve the product attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            AppHelper.Product = ((AssemblyProductAttribute)objects[0]).Product;

            /* Retrieve the version attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            AppHelper.Version = ((AssemblyFileVersionAttribute)objects[0]).Version;

            /* Retrieve the copyright attribute from the entry assembly. */
            objects = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AppHelper.Copyright = ((AssemblyCopyrightAttribute)objects[0]).Copyright;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>First and last name of the author.</summary>
        public const string AuthorName = "Jeff Bourdier";

        /// <summary>Full name of the author (for use mainly with copyright notices).</summary>
        public const string AuthorFullName = "Jeffrey Paul Bourdier";

        /// <summary>Title attribute from the entry assembly.</summary>
        public static readonly string Title;

        /// <summary>Product attribute from the entry assembly.</summary>
        public static readonly string Product;

        /// <summary>File version attribute from the entry assembly.</summary>
        public static readonly string Version;

        /// <summary>Copyright attribute from the entry assembly.</summary>
        public static readonly string Copyright;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Initializes a new instance of the Uri class with the specified resource file pack URI.</summary>
        /// <param name="common">True if the resource file is shared across applications; otherwise, false.</param>
        /// <param name="fileName">The name of the resource file.</param>
        /// <returns>A Uri instance created from the resource file pack URI.</returns>
        public static Uri CreateResourceUri(bool common, string fileName)
        {
            string s = string.Format("pack://application:,,,/{0}/{1}", common ? "common" : "resources", fileName);
            return new Uri(s, UriKind.RelativeOrAbsolute);
        }

        #endregion
    }
}
