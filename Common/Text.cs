/* Text.cs - Implementation of Text class, which provides static methods for processing text.
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


/* Exception */
using System;


namespace JeffBourdier
{
    /// <summary>Provides static methods for processing text.</summary>
    public static class Text
    {
        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>
        /// Parses a string meant to serve as the content of a label.  If the string contains parentheses, the text
        /// within the parentheses is returned.  Otherwise, the string with underscores and colons removed is returned.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>The string with underscores and colons removed.</returns>
        public static string ParseLabel(string s)
        {
            int i, n;

            i = s.IndexOf('(') + 1;
            n = s.LastIndexOf(')') - i;
            if (i > 0 && n > 0) return s.Substring(i, n);
            return s.Replace("_", null).Replace(":", null);
        }

        /// <summary>
        /// Inserts a space before each capital letter in a string.  This is useful for making a
        /// mixed-case identifier (e.g., "SpaceWords") more human-readable (e.g., "Space Words").
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>The string s with a space before each capital letter.</returns>
        public static string SpaceWords(string s)
        {
            string r = null;
            foreach (char c in s)
            {
                if (char.IsUpper(c)) r += ' ';
                r += c;
            }
            return r;
        }

        /// <summary>Returns an error message using the standard format.</summary>
        /// <param name="text">Error-specific text.</param>
        /// <param name="ex">The exception associated with the error.</param>
        /// <returns>An error message using the standard format.</returns>
        public static string FormatErrorMessage(string text, Exception ex)
        { return string.Format(Common.Resources.ErrorMessageFormat, text, ex.Message); }

        #endregion
    }
}
