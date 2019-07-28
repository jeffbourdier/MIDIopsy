/* MidiHeaderData.cs - Implementation of MidiHeaderData class, which encapsulates the data of a MIDI header (MThd) chunk.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException, Environment */
using System;


namespace JeffBourdier
{
    /// <summary>
    /// Encapsulates the six bytes of data (format, number of tracks, and delta-time division) for a MIDI header (MThd) chunk.
    /// </summary>
    public sealed class MidiHeaderData : MidiData
    {
        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static MidiHeaderData()
        {
            /* Initialize array containing valid string values for frames per second. */
            MidiHeaderData.FramesPerSecondStrings = new string[MidiHeaderData.FramesPerSecondCount] {
                Properties.Resources.FramesPerSecond24, Properties.Resources.FramesPerSecond25,
                Properties.Resources.FramesPerSecond29, Properties.Resources.FramesPerSecond30 };

            /* Initialize array containing valid numeric values for frames per second. */
            MidiHeaderData.FramesPerSecondValues = new uint[MidiHeaderData.FramesPerSecondCount];
            for (int i = 0; i < MidiHeaderData.FramesPerSecondCount; ++i)
                if (uint.TryParse(MidiHeaderData.FramesPerSecondStrings[i], out MidiHeaderData.FramesPerSecondValues[i])) continue;
                else MidiHeaderData.FramesPerSecondValues[i] = 29;
        }

        #endregion

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiHeaderData class.</summary>
        /// <param name="bytes">Array containing the six bytes of chunk data.</param>
        /// <param name="index">Index in the array at which the six bytes of chunk data begin.</param>
        public MidiHeaderData(byte[] bytes, int index)
            : base(bytes, 6, index)
        {
            /* Set format and number of tracks.  (These are straightforward.) */
            this._Format = MidiData.ReadNumber(bytes, 2, index);
            this._NumberOfTracks = MidiData.ReadNumber(bytes, 2, index + 2);

            /* Set delta-time division.  If the Most Significant Bit (MSB) is set, frames per
             * second and ticks per frame apply.  Otherwise, ticks per quarter-note applies.
             */
            uint n = bytes[index + 4];
            if ((n & 0x80) > 0)
            {
                this._FramesPerSecond = byte.MaxValue + 1 - n;
                this._TicksPerFrame = bytes[index + 5];
                this._TicksPerQuarterNote = uint.MaxValue;
            }
            else
            {
                this._TicksPerQuarterNote = MidiData.ReadNumber(bytes, 2, index + 4);
                this._FramesPerSecond = uint.MaxValue;
                this._TicksPerFrame = uint.MaxValue;
            }

            this.SetComment();
        }

        /// <summary>Initializes a new instance of the MidiHeaderData class.</summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="ticksPerQuarterNote">Number of delta-time "ticks" which make up a quarter-note.</param>
        public MidiHeaderData(uint format, uint numberOfTracks, uint ticksPerQuarterNote)
            : base(6)
        {
            this.Format = format;
            this.NumberOfTracks = numberOfTracks;
            this.SetMetricalTime(ticksPerQuarterNote);
        }

        /// <summary>Initializes a new instance of the MidiHeaderData class.</summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="framesPerSecond">
        /// Standard SMPTE and MIDI time code format representing the number of frames per second.
        /// </param>
        /// <param name="ticksPerFrame">Resolution within a frame (ticks per frame).</param>
        public MidiHeaderData(uint format, uint numberOfTracks, uint framesPerSecond, uint ticksPerFrame)
            : base(6)
        {
            this.Format = format;
            this.NumberOfTracks = numberOfTracks;
            this.SetTimeCodeBasedDivision(framesPerSecond, ticksPerFrame);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>
        /// The number of standard SMPTE and MIDI time code formats representing the number of frames per second.
        /// </summary>
        public const int FramesPerSecondCount = 4;

        /// <summary>Array containing valid string values for frames per second.</summary>
        public static readonly string[] FramesPerSecondStrings;

        /// <summary>Array containing valid numeric values for frames per second.</summary>
        public static readonly uint[] FramesPerSecondValues;

        #endregion

        #region Private Fields

        private uint _Format;
        private uint _NumberOfTracks;
        private uint _TicksPerQuarterNote;
        private uint _FramesPerSecond;
        private uint _TicksPerFrame;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Specifies the overall organization of the MIDI file.</summary>
        public uint Format
        {
            get { return this._Format; }

            set
            {
                MidiData.ValidateNumber(value, 0, ushort.MaxValue, Properties.Resources.Format);
                MidiData.WriteNumber(value, 2, this.Bytes, 0);
                this._Format = value;
                this.SetComment();
            }
        }

        /// <summary>Number of track chunks in the MIDI file.</summary>
        public uint NumberOfTracks
        {
            get { return this._NumberOfTracks; }

            set
            {
                MidiData.ValidateNumber(value, 0, ushort.MaxValue, Properties.Resources.NumberOfTracks);
                MidiData.WriteNumber(value, 2, this.Bytes, 2);
                this._NumberOfTracks = value;
                this.SetComment();
            }
        }

        /// <summary>Number of delta-time "ticks" which make up a quarter-note.</summary>
        /// <remarks>
        /// This applies only for metrical time.  For time-code-based time, FramesPerSecond
        /// and TicksPerFrame apply, and TicksPerQuarterNote is uint.MaxValue (to indicate
        /// that it does not apply).  This value must be set via SetMetricalDivision.
        /// </remarks>
        public uint TicksPerQuarterNote { get { return this._TicksPerQuarterNote; } }

        /// <summary>Standard SMPTE and MIDI time code format representing the number of frames per second.</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are uint.MaxValue (to indicate
        /// that they do not apply).  This value must be set via SetTimeCodeBasedDivision.
        /// </remarks>
        public uint FramesPerSecond { get { return this._FramesPerSecond; } }

        /// <summary>Resolution within a frame (ticks per frame).</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are uint.MaxValue (to indicate
        /// that they do not apply).  This value must be set via SetTimeCodeBasedDivision.
        /// </remarks>
        public uint TicksPerFrame { get { return this._TicksPerFrame; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Sets the applicable delta-time division values for metrical time.</summary>
        /// <param name="ticksPerQuarterNote">Number of delta-time "ticks" which make up a quarter-note.</param>
        public void SetMetricalTime(uint ticksPerQuarterNote)
        {
            MidiData.ValidateNumber(ticksPerQuarterNote, 0, (uint)short.MaxValue, Properties.Resources.TicksPerQuarterNote);
            MidiData.WriteNumber(ticksPerQuarterNote, 2, this.Bytes, 4);
            this._TicksPerQuarterNote = ticksPerQuarterNote;
            this._FramesPerSecond = uint.MaxValue;
            this._TicksPerFrame = uint.MaxValue;
            this.SetComment();
        }

        /// <summary>Sets the applicable delta-time division values for time-code-based time.</summary>
        /// <param name="framesPerSecond">
        /// Standard SMPTE and MIDI time code format representing the number of frames per second.
        /// </param>
        /// <param name="ticksPerFrame">Resolution within a frame (ticks per frame).</param>
        public void SetTimeCodeBasedDivision(uint framesPerSecond, uint ticksPerFrame)
        {
            MidiHeaderData.ValidateFramesPerSecond(framesPerSecond);
            MidiData.ValidateNumber(ticksPerFrame, 0, byte.MaxValue, Properties.Resources.TicksPerFrame);
            MidiData.WriteNumber(byte.MaxValue + 1 - framesPerSecond, 1, this.Bytes, 4);
            MidiData.WriteNumber(ticksPerFrame, 1, this.Bytes, 5);
            this._FramesPerSecond = framesPerSecond;
            this._TicksPerFrame = ticksPerFrame;
            this._TicksPerQuarterNote = uint.MaxValue;
            this.SetComment();
        }

        #endregion

        #region Private Methods

        private void SetComment()
        {
            string s, r;

            /* Determine which string to use for the file format. */
            switch (this.Format)
            {
                case 0: s = Properties.Resources.Format0; break;
                case 1: s = Properties.Resources.Format1; break;
                case 2: s = Properties.Resources.Format2; break;
                default: s = this.Format.ToString(); break;
            }

            /* Build the string for delta-time division. */
            r = (this.TicksPerQuarterNote < uint.MaxValue)
                ? string.Format("{0} {1}", this.TicksPerQuarterNote,
                    Text.ParseLabel(Properties.Resources.TicksPerQuarterNote).ToLower())
                : string.Format("{0} {1}, {2} {3}",
                    this.FramesPerSecond, Text.ParseLabel(Properties.Resources.FramesPerSecond),
                    this.TicksPerFrame, Text.ParseLabel(Properties.Resources.TicksPerFrame).ToLower());

            /* Assemble the entire comment string. */
            this.Comment = string.Format(Properties.Resources.HeaderDataFormat, s, this.NumberOfTracks, r);
        }

        private static void ValidateFramesPerSecond(uint n)
        {
            /* If the value is valid, all is well. */
            foreach (uint v in MidiHeaderData.FramesPerSecondValues) if (n == v) return;

            /* The value is invalid.  (This calls for an error.) */
            string s = Text.ParseLabel(Properties.Resources.FramesPerSecond);
            s = string.Format(Properties.Resources.ValueListFormat, s);
            for (int i = 0; i < MidiHeaderData.FramesPerSecondCount; ++i)
                s += Environment.NewLine + string.Format("  - {0}, ({1})",
                    MidiHeaderData.FramesPerSecondValues[i],
                    MidiHeaderData.FramesPerSecondStrings[i]);
            throw new ApplicationException(s);
        }

        #endregion
    }
}
