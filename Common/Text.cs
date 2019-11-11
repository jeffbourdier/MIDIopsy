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

        /// <summary>Returns an error message using the standard format.</summary>
        /// <param name="text">Error-specific text.</param>
        /// <param name="ex">The exception associated with the error.</param>
        /// <returns>An error message using the standard format.</returns>
        public static string FormatErrorMessage(string text, Exception ex)
        { return string.Format(Common.Resources.ErrorMessageFormat, text, ex.Message); }

        /// <summary>Indicates whether a character is considered printable according to Windows-1252 encoding.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>True if c is printable; otherwise, false.</returns>
        public static bool IsPrintable(char c)
        {
            if (c < 0x20) return false;
            if (c < 0x7F) return true;
            if (c < 0x80) return false;
            if (c < 0x81) return true;
            if (c < 0x82) return false;
            if (c < 0x8D) return true;
            if (c < 0x8E) return false;
            if (c < 0x8F) return true;
            if (c < 0x91) return false;
            if (c < 0x9D) return true;
            if (c < 0x9E) return false;
            if (c < 0xA0) return true;
            if (c < 0xA1) return false;
            if (c < 0x100) return true;
            return false;
        }

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

        #endregion
    }
}
