/* MidiHeaderChunk.cs - Implementation of MidiHeaderChunk class, which represents a MIDI header (MThd) chunk.
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
    /// <summary>Represents a MIDI header (MThd) chunk.</summary>
    public class MidiHeaderChunk : MidiChunk
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiHeaderChunk class.</summary>
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        /// <remarks>This constructor is to be used when reading an existing MIDI file from disk.</remarks>
        public MidiHeaderChunk(MidiFile owner, int length, byte[] bytes, int index)
            : base(owner, "MThd", length)
        {
            this.Data = new MidiHeaderData(bytes, index);
            this.AddItem(this.Data);

            string s = string.Format("expecting {0} tracks", this.Data.NumberOfTracks);
            Logger.WriteMessage(s);
        }

        /// <summary>Initializes a new instance of the MidiHeaderChunk class using a MidiHeaderData object.</summary>
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        /// <param name="data">A MidiHeaderData object (based on user input).</param>
        /// <remarks>This constructor is to be used when creating a new MIDI file from scratch.</remarks>
        public MidiHeaderChunk(MidiFile owner, MidiHeaderData data)
            : base(owner, "MThd", 6)
        {
            this.Data = data;
            this.AddItem(data);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private MidiHeaderData Data;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Specifies the overall organization of the MIDI file.</summary>
        public uint Format { get { return this.Data.Format; } }

        /// <summary>Returns the number of track chunks in the MIDI file.</summary>
        public uint NumberOfTracks { get { return this.Data.NumberOfTracks; } }

        #endregion
    }
}
