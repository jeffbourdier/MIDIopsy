/* MidiChunkInfo.cs - Implementation of MidiChunkInfo class, which encapsulates the type and length of a MIDI file chunk.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException */
using System;


namespace JeffBourdier
{
    /// <summary>Encapsulates the eight bytes of type and length for a MIDI file chunk.</summary>
    public sealed class MidiChunkInfo : MidiData
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChunkInfo class.</summary>
        /// <param name="type">Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        public MidiChunkInfo(string type, int length)
            : base(8)
        {
            this.ChunkType = type;
            this.ChunkLength = length;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string _Type;
        private int _Length;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</summary>
        public string ChunkType
        {
            get { return this._Type; }

            set
            {
                if (value.Length != 4) throw new ApplicationException(Properties.Resources.ChunkTypeLength);
                MidiData.WriteText(value, this.Bytes, 0);
                this._Type = value;
                this.SetComment();
            }
        }

        /// <summary>Number of bytes in the chunk (not including the eight bytes of type and length).</summary>
        public int ChunkLength
        {
            get { return this._Length; }

            set
            {
                MidiData.WriteNumber((uint)value, 4, this.Bytes, 4);
                this._Length = value;
                this.SetComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void SetComment()
        {
            string s;

            switch (this.ChunkType)
            {
                case "MThd": s = Properties.Resources.HeaderChunk; break;
                case "MTrk": s = Properties.Resources.TrackChunk; break;
                default: s = Properties.Resources.UnknownChunkType; break;
            }
            this.Comment = string.Format(Properties.Resources.ChunkCommentFormat, this.ChunkType, s, this.ChunkLength);
        }

        #endregion
    }
}
