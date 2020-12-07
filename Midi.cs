/* Midi.cs - Implementation of Midi class, which provides common functionality for MIDI data.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Environment, Math, StringSplitOptions */
using System;

/* List */
using System.Collections.Generic;

/* NumberStyles */
using System.Globalization;


namespace JeffBourdier
{
    /// <summary>Provides common functionality for MIDI data.</summary>
    public static class Midi
    {
        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>The largest number allowed for a variable-length quantity (VLQ).</summary>
        public const int MaxVLQ = 0x0FFFFFFF;

        #endregion

        #region Private Fields

        private static readonly string[] Separators = new string[] { " ", "\t", Environment.NewLine };

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Represents a portion of a byte array in hexadecimal format.</summary>
        /// <param name="bytes">Array of bytes.</param>
        /// <param name="offset">Offset into the byte array at which to start reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>Representation of the specified portion of the byte array in hexadecimal format.</returns>
        public static string FormatHex(byte[] bytes, int offset, int length)
        {
            string s = null;
            for (int n = offset + length, i = offset; i < n; ++i)
            {
                if (i > offset) s += " ";
                s += bytes[i].ToString("X2");
            }
            return s;
        }

        /// <summary>Convert a string representing some binary data in hexadecimal format to a byte array.</summary>
        /// <param name="hex">String representing some binary data in hexadecimal format.</param>
        /// <returns>Array of bytes (or null if the string is not hex).</returns>
        public static byte[] ParseHex(string hex)
        {
            List<byte> bytes = new List<byte>(hex.Length / 3);
            string[] strings = hex.Split(Midi.Separators, StringSplitOptions.RemoveEmptyEntries);
            try { foreach (string s in strings) bytes.Add(byte.Parse(s, NumberStyles.HexNumber)); }
            catch { return null; }
            return bytes.ToArray();
        }

        /// <summary>Reads text as a string of ASCII characters from a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the text.</param>
        /// <param name="offset">Offset into the byte array at which the text is stored.</param>
        /// <param name="length">The number of bytes (ASCII characters) to read.</param>
        /// <returns>The text that is read.</returns>
        public static string ReadText(byte[] bytes, int offset, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; ++i)
            {
                chars[i] = (char)bytes[offset + i];
                if (!Midi.IsPrintable(chars[i])) chars[i] = '.';
            }
            return new string(chars);
        }

        /// <summary>Reads a number as an integer (in network byte order) from a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the number.</param>
        /// <param name="offset">
        /// Offset into the byte array at which the number is stored (most significant byte first).
        /// </param>
        /// <param name="length">The number of bytes to read (should not be greater than 4).</param>
        /// <returns>The number that is read.</returns>
        public static int ReadNumber(byte[] bytes, int offset, int length)
        {
            int i, n = 0;
            for (i = 0; i < length; ++i) n |= bytes[offset + i] << (8 * (length - i - 1));
            return n;
        }

        /// <summary>Returns the number of bytes required to store a variable-length quantity (VLQ).</summary>
        /// <param name="vlq">The VLQ.</param>
        /// <returns>The number of bytes required to store the VLQ.</returns>
        public static int SizeVLQ(int vlq) { return (vlq > 1) ? (int)Math.Truncate(Math.Log(vlq, 0x80) + 1) : 1; }

        /// <summary>Creates a byte value by concatenating the specified values.</summary>
        /// <param name="high">High-order nibble (i.e., the more significant four bits) of the byte value.</param>
        /// <param name="low">Low-order nibble (i.e., the less significant four bits) of the byte value.</param>
        /// <returns>The byte value resulting from the concatenation of the two nibbles.</returns>
        public static int MakeByte(int high, int low) { return ((high << 4) | low) & 0xFF; }

        /// <summary>Returns the high-order nibble of a byte value.</summary>
        /// <param name="b">Byte value.  Only the least significant eight bits are considered.</param>
        /// <returns>High-order nibble (i.e., the more significant four bits) of the byte value.</returns>
        public static int GetHighNibble(int b) { return (b >> 4) & 0xF; }

        /// <summary>Returns the low-order nibble of a byte value.</summary>
        /// <param name="b">Byte value.  Only the least significant eight bits are considered.</param>
        /// <returns>Low-order nibble (i.e., the less significant four bits) of the byte value.</returns>
        public static int GetLowNibble(int b) { return b & 0xF; }

        #endregion

        #region Private Methods

        /// <summary>Indicates whether a character is considered printable according to Windows-1252 encoding.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>True if c is printable; otherwise, false.</returns>
        private static bool IsPrintable(char c)
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

        #endregion
    }
}
