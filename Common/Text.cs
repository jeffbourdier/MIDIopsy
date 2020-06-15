/* Text.cs - Implementation of Text class, which provides static methods for processing text.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Environment, Exception */
using System;


namespace JeffBourdier
{
    /// <summary>Specifies enumerated constants to define text cases.</summary>
    public enum TextCase { Title, Sentence }

    /// <summary>Provides static methods for processing text.</summary>
    public static class Text
    {
        /***********
         * Methods *
         ***********/

        #region Public Methods

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

        /// <summary>Double-quotes a text string.</summary>
        /// <param name="text">The text string to double-quote.</param>
        /// <returns>The text string in double-quotes.</returns>
        public static string DoubleQuote(string text) { return string.Format("\"{0}\"", text); }

        /// <summary>Returns an error message using the standard format.</summary>
        /// <param name="text">Error-specific text.</param>
        /// <param name="ex">The exception associated with the error.</param>
        /// <returns>An error message using the standard format.</returns>
        public static string FormatErrorMessage(string text, Exception ex)
        { return string.Format(Common.Resources.ErrorMessageFormat, text, Environment.NewLine, ex.Message); }

        /// <summary>Returns a string representing a quantity of some unit.</summary>
        /// <param name="n">The quantity to represent.</param>
        /// <param name="unit">The unit of the quantity (e.g., "byte").</param>
        /// <returns>A string representation of the quantity and units (e.g., "32 bytes").</returns>
        public static string FormatCount(int n, string unit)
        { return n + " " + Text.ParseLabel(unit).ToLower() + ((n == 1) ? string.Empty : "s"); }

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

        /// <summary>
        /// Returns a copy of a string with a space inserted before each capital letter.  This is useful for
        /// making a mixed-case identifier (e.g., "SpaceWords") more human-readable (e.g., "Space Words").
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A copy of the string s with a space before each capital letter.</returns>
        public static string SpaceWords(string s)
        {
            string r = string.Empty;
            foreach (char c in s)
            {
                if (char.IsUpper(c)) r += ' ';
                r += c;
            }
            return r;
        }

        /// <summary>Returns a copy of a string with each word capitalized as specified.</summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="textCase">Specifies how to capitalize each word.</param>
        /// <returns>A copy of the string s with each word capitalized as specified.</returns>
        public static string ChangeCase(string s, TextCase textCase)
        {
            string r = string.Empty;
            bool initial = true;
            for (int i = 0; i < s.Length; ++i)
            {
                if (char.IsWhiteSpace(s[i])) { initial = true; continue; }
                if (initial)
                {
                    r += s.Substring(i, 1).ToUpper();
                    initial = false;
                    continue;
                }
                if (textCase == TextCase.Sentence) return r + s.Substring(i).ToLower();
                r += s.Substring(i, 1).ToLower();
            }
            return r;
        }

        #endregion
    }
}
