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
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        /// <remarks>This constructor is to be used when reading an existing MIDI file from disk.</remarks>
        public MidiHeaderChunk(int length, byte[] bytes, int index)
            : base("MThd", length)
        {
            MidiHeaderData data = new MidiHeaderData(bytes, index);
            this.AddItem(data);

            string s = string.Format("expecting {0} tracks", data.NumberOfTracks);
            Logger.WriteMessage(s);
        }

        /// <summary>Initializes a new instance of the MidiHeaderChunk class using metrical time.</summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="ticksPerQuarterNote">Number of delta-time "ticks" which make up a quarter-note.</param>
        /// <remarks>This constructor is to be used when creating a new MIDI file from scratch.</remarks>
        public MidiHeaderChunk(uint format, uint numberOfTracks, uint ticksPerQuarterNote)
            : base("MThd", 6)
        {
            MidiHeaderData data = new MidiHeaderData(format, numberOfTracks, ticksPerQuarterNote);
            this.AddItem(data);
        }

        /// <summary>Initializes a new instance of the MidiHeaderChunk class using time-code-based time.</summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="framesPerSecond">
        /// Standard SMPTE and MIDI time code format representing the number of frames per second.
        /// </param>
        /// <param name="ticksPerFrame">Resolution within a frame (ticks per frame).</param>
        /// <remarks>This constructor is to be used when creating a new MIDI file from scratch.</remarks>
        public MidiHeaderChunk(uint format, uint numberOfTracks, uint framesPerSecond, uint ticksPerFrame)
            : base("MThd", 6)
        {
            MidiHeaderData data = new MidiHeaderData(format, numberOfTracks, framesPerSecond, ticksPerFrame);
            this.AddItem(data);
        }

        #endregion
    }
}
