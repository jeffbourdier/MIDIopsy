/* MidiChunkInfo.cs - Implementation of MidiChunkInfo class, which represents the type and length of a MIDI file chunk.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


namespace JeffBourdier
{
    /// <summary>Represents the eight bytes of type and length for a MIDI file chunk.</summary>
    public sealed class MidiChunkInfo : MidiItem
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChunkInfo class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiChunkInfo(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        public const int TypeSize = 4;
        public const int LengthSize = 4;
        public const int TotalSize = MidiChunkInfo.TypeSize + MidiChunkInfo.LengthSize;
        public const string HeaderType = "MThd";
        public const string TrackType = "MTrk";

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size { get { return MidiChunkInfo.TotalSize; } }

        /// <summary>
        /// Gets the text to display in the "Hex" field (i.e., the bytes represented by this item, in hexadecimal format).
        /// </summary>
        public override string HexField
        {
            get
            {
                /* For unknown chunk types, augment the size so that the entire chunk is displayed in the Hex field. */
                return (this.Type == MidiChunkInfo.HeaderType || this.Type == MidiChunkInfo.TrackType) ?
                    base.HexField : this.File.FormatHex(this.Offset, this.Size + this.Length);
            }
        }

        /// <summary>Gets the text to display in the "Type" field (i.e., chunk type).</summary>
        public override string TypeField
        {
            get
            {
                string s = string.Empty;
                switch (this.Type)
                {
                    case MidiChunkInfo.HeaderType: s = Properties.Resources.Header; break;
                    case MidiChunkInfo.TrackType: s = Properties.Resources.Track; break;
                }
                if (s.Length > 0)
                {
                    s = UI.ParseLabel(s).ToLower();
                    s = string.Format(" ({0})", s);
                }
                return this.Type + s + " " + Properties.Resources.Chunk;
            }
        }

        /// <summary>Gets the text to display in the "Count" field (i.e., chunk length).</summary>
        public override string CountField { get { return MidiItem.FormatCount(this.Length, Properties.Resources.Byte); } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public override string DataField
        {
            get
            {
                /* For unknown chunk types, attempt to interpret the data as ASCII text. */
                string type = this.Type;
                return (type == MidiChunkInfo.HeaderType || type == MidiChunkInfo.TrackType) ?
                    base.DataField : this.File.ReadText(this.LengthOffset + MidiChunkInfo.LengthSize, this.Length);
            }
        }

        /// <summary>Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</summary>
        public string Type
        {
            get { return this.File.ReadText(this.Offset, MidiChunkInfo.TypeSize); }
            set { this.File.WriteText(this.Offset, value, MidiChunkInfo.TypeSize); }
        }

        /// <summary>Number of bytes in the chunk (not including the eight bytes of type and length).</summary>
        public int Length
        {
            get { return this.File.ReadNumber(this.LengthOffset, MidiChunkInfo.LengthSize); }
            set { this.File.WriteNumber(this.LengthOffset, value, MidiChunkInfo.LengthSize); }
        }

        #endregion

        #region Private Properties

        private int LengthOffset { get { return this.Offset + MidiChunkInfo.TypeSize; } }

        #endregion
    }
}
