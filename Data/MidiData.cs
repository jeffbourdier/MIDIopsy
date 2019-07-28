/* MidiData.cs - Implementation of MidiData class, which provides common functionality for objects that encapsulate MIDI data.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array, ApplicationException, Math */
using System;

/* Run */
using System.Windows.Documents;


namespace JeffBourdier
{
    /// <summary>Encapsulates a byte array representing the smallest commentable unit of a MIDI file chunk.</summary>
    public class MidiData
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiData class.</summary>
        /// <param name="bytes">Array of bytes containing the data.</param>
        /// <param name="length">The number of bytes in this portion of the chunk.</param>
        /// <param name="index">Index in the byte array at which the data begins.</param>
        /// <remarks>This constructor instantiates the byte array and copies the data.</remarks>
        public MidiData(byte[] bytes, int length, int index)
        {
            this.Bytes = new byte[length];
            Array.Copy(bytes, index, this.Bytes, 0, length);
        }

        #endregion

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiData class.</summary>
        /// <param name="length">The number of bytes in this portion of the chunk.</param>
        /// <remarks>This constructor instantiates the byte array but does not copy any data.</remarks>
        protected MidiData(int length) { this.Bytes = new byte[length]; }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>The largest number allowed for a variable-length quantity (VLQ).</summary>
        public const uint MaxVLQ = 0x0FFFFFFF;

        #endregion

        #region Protected Fields

        protected byte[] Bytes;
        protected string Comment;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes in the array.</summary>
        public int Length { get { return this.Bytes.Length; } }

        /// <summary>Gets a run of text representing the byte array in hexadecimal format.</summary>
        public Run HexRun
        {
            get
            {
                string s = null;
                for (int i = 0; i < this.Bytes.Length; ++i)
                {
                    if (i > 0) s += " ";
                    s += this.Bytes[i].ToString("X2");
                }
                return new Run(s);
            }
        }

        /// <summary>Gets a run of user-friendly text describing this object.</summary>
        public Run CommentRun
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Comment)) return new Run(this.Comment);

                /* Attempt to convert the data bytes to a string for the comment. */
                string s = MidiData.ReadText(this.Bytes, this.Bytes.Length, 0);
                s = string.Format(Properties.Resources.UnrecognizedDataFormat, s);
                return new Run(s);
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Copies the bytes from this array into another array starting at the specified index.</summary>
        /// <param name="bytes">The array that receives the data.</param>
        /// <param name="index">Index in the array at which storing begins.</param>
        public void CopyBytes(byte[] bytes, int index) { Array.Copy(this.Bytes, 0, bytes, index, this.Length); }

        /// <summary>Converts a numeric value to a string representation of its assigned note(s).</summary>
        /// <param name="n">Numeric value assigned to note (middle C has a reference value of 60).</param>
        /// <returns>String representation of note(s) to which numeric value is assigned.</returns>
        public static string NumberToNote(uint n)
        {
            switch (n % 12)
            {
                case 0: return "C / B#";
                case 1: return "Db / C#";
                case 2: return "D / D";
                case 3: return "Eb / D#";
                case 4: return "Fb / E";
                case 5: return "F / E#";
                case 6: return "Gb / F#";
                case 7: return "G / G";
                case 8: return "Ab / G#";
                case 9: return "A / A";
                case 10: return "Bb / A#";
                case 11: return "Cb / B";
                default: return string.Empty;
            }
        }

        /// <summary>Reads text as a string of ASCII characters from a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the text.</param>
        /// <param name="length">The number of bytes (ASCII characters) to read.</param>
        /// <param name="index">Index in the byte array at which the text is stored.</param>
        /// <returns>The text that is read.</returns>
        public static string ReadText(byte[] bytes, int length, int index)
        {
            char[] chars = new char[length];

            for (int i = 0; i < length; ++i) chars[i] = (char)bytes[index + i];
            return new string(chars);
        }

        /// <summary>
        /// Reads a number as an unsigned integer (of any number of bytes, up to 4, in network byte order) from a byte array.
        /// </summary>
        /// <param name="bytes">Array of bytes containing the number.</param>
        /// <param name="length">The number of bytes to read (should not be greater than 4).</param>
        /// <param name="index">Index in the byte array at which the number is stored (most significant byte first).</param>
        /// <returns>The number that is read.</returns>
        public static uint ReadNumber(byte[] bytes, int length, int index)
        {
            uint n = 0;

            if (length > 4) throw new ApplicationException(Properties.Resources.NumberNotSupported);
            for (int i = 0; i < length; ++i) n |= (uint)bytes[index + i] << (8 * (length - i - 1));
            return n;
        }

        /// <summary>Reads a variable-length quantity (VLQ) from a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the VLQ.</param>
        /// <param name="index">Index in the byte array at which the VLQ is stored.</param>
        /// <returns>The VLQ that is read.</returns>
        public static int ReadVLQ(byte[] bytes, int index)
        {
            int i, n = 0;

            /* A variable-length quantity is in big-endian order, and only the lowest seven bits of each byte are part of the
             * quantity.  The Most Significant Bit (MSB) is set in all bytes except the last.  It can be up to four bytes long.
             */
            for (i = 0; i < 4; ++i)
            {
                /* As appropriate, initialize the result (or shift it seven bits
                 * to the left) and add to it the lowest seven bits of this byte.
                 */
                n = (i > 0) ? (n << 7) : 0;
                n += bytes[index + i] & 0x7F;

                /* If the MSB is set, we have not yet encountered the last byte of the VLQ, so continue
                 * processing.  Otherwise, we have encountered the last byte, so return the result.
                 */
                if ((bytes[index + i] & 0x80) > 0) continue;
                return n;
            }

            /* This means we processed four bytes and never encountered the last byte (MSB clear). */
            throw new ApplicationException(Properties.Resources.InvalidVLQ);
        }

        /// <summary>Writes text as a string of ASCII characters into a byte array.</summary>
        /// <param name="text">The text to write.</param>
        /// <param name="bytes">Array of bytes into which the text is written.</param>
        /// <param name="index">Index in the byte array at which the text is written.</param>
        public static void WriteText(string text, byte[] bytes, int index)
        { for (int i = 0; i < text.Length; ++i) bytes[index + i] = (byte)text[i]; }

        /// <summary>
        /// Writes a number as an unsigned integer (of any number of bytes, up to 4) into a byte array (in network byte order).
        /// </summary>
        /// <param name="n">The number to write.</param>
        /// <param name="length">The number of bytes to write (should not be greater than 4).</param>
        /// <param name="bytes">Array of bytes into which the number is written.</param>
        /// <param name="index">Index in the byte array at which the number is written (most significant byte first).</param>
        public static void WriteNumber(uint n, int length, byte[] bytes, int index)
        {
            if (length > 4) throw new ApplicationException(Properties.Resources.NumberNotSupported);
            for (int i = 0; i < length; ++i) bytes[index + i] = (byte)(0xFF & (n >> (8 * (length - i - 1))));
        }

        /// <summary>Writes a variable-length quantity (VLQ) into a byte array.</summary>
        /// <param name="n">The VLQ to write.</param>
        /// <param name="bytes">Array of bytes into which the VLQ is written.</param>
        /// <param name="index">Index in the byte array at which the VLQ is written.</param>
        public static void WriteVLQ(int n, byte[] bytes, int index)
        {
            int i, j = 0;
            byte[] a = new byte[4];

            /* For the simple (and common) case of a single-byte VLQ, take a shortcut. */
            if (n < 0x80) { bytes[index] = (byte)n; return; }

            /* If the value is invalid, we can go no further. */
            if (n < 0 || n > MidiData.MaxVLQ) throw new ApplicationException(Properties.Resources.InvalidVLQ);

            /* The VLQ is stored as 7 bits per byte (most significant byte first).  All bytes except the last have the
             * Most Significant Bit (MSB) set, and the last byte has the MSB clear.  The easiest way to accomplish this
             * is to write the VLQ "backwards" (least significant byte first) to a temporary byte array, then reverse it.
             */
            for (i = n; i > 0; i >>= 7)
            {
                bytes[index + j] = (byte)((i % 0x100) & 0x7F);
                if (j > 0) bytes[index + j] |= 0x80;
                ++j;
            }
            Array.Reverse(bytes, index, j);
        }

        /// <summary>Returns the number of bytes required to store a variable-length quantity (VLQ).</summary>
        /// <param name="n">The VLQ.</param>
        /// <returns>The number of bytes required to store the VLQ.</returns>
        public static int SizeVLQ(int n) { return (n > 1) ? (int)Math.Truncate(Math.Log(n, 0x80) + 1) : 1; }

        /// <summary>Validates a number against a range of unsigned integers.</summary>
        /// <param name="n">The number to validate.</param>
        /// <param name="min">Minimum value for the number.</param>
        /// <param name="max">Maximum value for the number.</param>
        /// <param name="description">
        /// A short description of the numeric value (for display purposes).  May be label content.
        /// </param>
        public static void ValidateNumber(uint n, uint min, uint max, string description)
        {
            if (n >= min && n <= max) return;

            string s = Text.ParseLabel(description);
            s = string.Format(Properties.Resources.ValueRangeFormat, s, min, max);
            throw new ApplicationException(s);
        }

        #endregion
    }
}
