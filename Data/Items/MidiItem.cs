/* MidiItem.cs - Implementation of MidiItem class, which represents the smallest commentable unit of a MIDI file chunk.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array */
using System;


namespace JeffBourdier
{
    /// <summary>Encapsulates a byte array representing the smallest commentable unit of a MIDI file chunk.</summary>
    public class MidiItem : MidiData
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiItem class.</summary>
        /// <param name="bytes">Array of bytes containing the data.</param>
        /// <param name="length">The number of bytes in this portion of the chunk.</param>
        /// <param name="index">Index in the byte array at which the data begins.</param>
        /// <remarks>This constructor instantiates the byte array and copies the data.</remarks>
        public MidiItem(byte[] bytes, int length, int index)
        {
            this.Bytes = new byte[length];
            Array.Copy(bytes, index, this.Bytes, 0, length);
        }

        #endregion

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiItem class.</summary>
        /// <param name="length">The number of bytes in this portion of the chunk.</param>
        /// <remarks>This constructor instantiates the byte array but does not copy any data.</remarks>
        protected MidiItem(int length) { this.Bytes = new byte[length]; }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected byte[] Bytes;
        protected string _Comment;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes in the array.</summary>
        public int Length { get { return this.Bytes.Length; } }

        /// <summary>Representation of the byte array in hexadecimal format.</summary>
        public virtual string Hex
        {
            get
            {
                string s = null;
                for (int i = 0; i < this.Bytes.Length; ++i)
                {
                    if (i > 0) s += " ";
                    s += this.Bytes[i].ToString("X2");
                }
                return s;
            }
        }

        /// <summary>User-friendly text describing this object.</summary>
        public string Comment
        {
            get
            {
                if (!string.IsNullOrEmpty(this._Comment)) return this._Comment;

                /* Attempt to convert the data bytes to a string for the comment. */
                string s = MidiData.ReadText(this.Bytes, this.Bytes.Length, 0);
                return string.Format(Properties.Resources.UnrecognizedDataFormat, s);
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

        #endregion
    }
}
