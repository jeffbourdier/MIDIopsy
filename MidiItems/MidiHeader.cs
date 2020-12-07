/* MidiHeader.cs - Implementation of MidiHeader class, which represents the data of a MIDI header (MThd) chunk.
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
    /// <summary>
    /// Represents the six bytes of data (format, number of tracks, and delta-time division) for a MIDI header (MThd) chunk.
    /// </summary>
    public sealed class MidiHeader : MidiItem
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiHeader class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiHeader(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        public const int FormatSize = 2;
        public const int NumberOfTracksSize = 2;
        public const int DivisionSize = 2;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size { get { return MidiHeader.SizeItem(); } }

        /// <summary>Gets the text to display in the "Type" field (i.e., file format).</summary>
        public override string TypeField
        {
            get
            {
                int i = this.Format;
                string s = string.Empty;
                switch (i)
                {
                    case 0: s = Properties.Resources.Format0; break;
                    case 1: s = Properties.Resources.Format1; break;
                    case 2: s = Properties.Resources.Format2; break;
                    default: s = i.ToString(); break;
                }
                return UI.ParseLabel(Properties.Resources.Format) + " " + s;
            }
        }

        /// <summary>Gets the text to display in the "Count" field (i.e., number of tracks).</summary>
        public override string CountField
        { get { return MidiItem.FormatCount(this.NumberOfTracks, Properties.Resources.Track); } }

        /// <summary>Gets the text to display in the "Data" field (i.e., delta-time division).</summary>
        public override string DataField
        {
            get
            {
                int ticksPerQuarterNote, framesPerSecond, ticksPerFrame;
                this.ParseDivision(out ticksPerQuarterNote, out framesPerSecond, out ticksPerFrame);
                return (ticksPerQuarterNote < 0)
                    ? string.Format("{0} {1}, {2} {3}",
                        framesPerSecond, UI.ParseLabel(Properties.Resources.FramesPerSecond),
                        ticksPerFrame, UI.ParseLabel(Properties.Resources.TicksPerFrame).ToLower())
                    : string.Format("{0} {1}", ticksPerQuarterNote,
                        UI.ParseLabel(Properties.Resources.TicksPerQuarterNote).ToLower());
            }
        }

        /// <summary>Specifies the overall organization of the MIDI file.</summary>
        public int Format
        {
            get { return this.File.ReadNumber(this.Offset, MidiHeader.FormatSize); }
            set { this.File.WriteNumber(this.Offset, value, MidiHeader.FormatSize); }
        }

        /// <summary>Number of track chunks in the MIDI file.</summary>
        public int NumberOfTracks
        {
            get { return this.File.ReadNumber(this.NumberOfTracksOffset, MidiHeader.NumberOfTracksSize); }
            set { this.File.WriteNumber(this.NumberOfTracksOffset, value, MidiHeader.NumberOfTracksSize); }
        }

        /// <summary>Number of delta-time "ticks" which make up a quarter-note.</summary>
        /// <remarks>
        /// This applies only for metrical time.  For time-code-based time, FramesPerSecond
        /// and TicksPerFrame apply, and TicksPerQuarterNote is -1 (to indicate that
        /// it does not apply).  This value must be set via SetMetricalDivision.
        /// </remarks>
        public int TicksPerQuarterNote
        {
            get
            {
                int ticksPerQuarterNote, framesPerSecond, ticksPerFrame;
                this.ParseDivision(out ticksPerQuarterNote, out framesPerSecond, out ticksPerFrame);
                return ticksPerQuarterNote;
            }
        }

        /// <summary>Standard SMPTE and MIDI time code format representing the number of frames per second.</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are -1 (to indicate that
        /// they do not apply).  This value must be set via SetTimeCodeBasedDivision.
        /// </remarks>
        public int FramesPerSecond
        {
            get
            {
                int ticksPerQuarterNote, framesPerSecond, ticksPerFrame;
                this.ParseDivision(out ticksPerQuarterNote, out framesPerSecond, out ticksPerFrame);
                return framesPerSecond;
            }
        }

        /// <summary>Resolution within a frame (ticks per frame).</summary>
        /// <remarks>
        /// This applies only for time-code-based time.  For metrical time, TicksPerQuarterNote
        /// applies, and FramesPerSecond and TicksPerFrame are -1 (to indicate that
        /// they do not apply).  This value must be set via SetTimeCodeBasedDivision.
        /// </remarks>
        public int TicksPerFrame
        {
            get
            {
                int ticksPerQuarterNote, framesPerSecond, ticksPerFrame;
                this.ParseDivision(out ticksPerQuarterNote, out framesPerSecond, out ticksPerFrame);
                return ticksPerFrame;
            }
        }

        #endregion

        #region Private Properties

        private int NumberOfTracksOffset { get { return this.Offset + MidiHeader.FormatSize; } }
        private int DivisionOffset { get { return this.NumberOfTracksOffset + MidiHeader.NumberOfTracksSize; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Returns the number of bytes required to store an item of this type.</summary>
        public static int SizeItem()
        { return MidiHeader.FormatSize + MidiHeader.NumberOfTracksSize + MidiHeader.DivisionSize; }

        /// <summary>Sets the applicable delta-time division values for metrical time.</summary>
        /// <param name="ticksPerQuarterNote">Number of delta-time "ticks" which make up a quarter-note.</param>
        public void SetMetricalDivision(int ticksPerQuarterNote)
        { this.File.WriteNumber(this.DivisionOffset, ticksPerQuarterNote, MidiHeader.DivisionSize); }

        /// <summary>Sets the applicable delta-time division values for time-code-based time.</summary>
        /// <param name="framesPerSecond">
        /// Standard SMPTE and MIDI time code format representing the number of frames per second.
        /// </param>
        /// <param name="ticksPerFrame">Resolution within a frame (ticks per frame).</param>
        public void SetTimeCodeBasedDivision(int framesPerSecond, int ticksPerFrame)
        {
            this.File.WriteNumber(this.DivisionOffset, byte.MaxValue + 1 - framesPerSecond, 1);
            this.File.WriteNumber(this.DivisionOffset + 1, ticksPerFrame, 1);
        }

        #endregion

        #region Private Methods

        private void ParseDivision(out int ticksPerQuarterNote, out int framesPerSecond, out int ticksPerFrame)
        {
            /* Retrieve the entire numeric value representing delta-time division.  If the Most
             * Significant Bit (MSB) is set, the numeric value represents frames per second and ticks
             * per frame.  Otherwise (MSB is clear), the entire numeric value is ticks per quarter-note.
             */
            ticksPerQuarterNote = this.File.ReadNumber(this.DivisionOffset, MidiHeader.DivisionSize);
            if (ticksPerQuarterNote < 0x8000) { framesPerSecond = -1; ticksPerFrame = -1; return; }

            /* The MSB is set, so split the numeric value into frames per second and ticks per frame. */
            framesPerSecond = byte.MaxValue + 1 - ((ticksPerQuarterNote >> 8) & 0xFF);
            ticksPerFrame = ticksPerQuarterNote & 0xFF;
            ticksPerQuarterNote = -1;
        }

        #endregion
    }
}
