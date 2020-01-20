/* MidiTimeSignatureEvent.cs - Implementation of MidiTimeSignatureEvent class,
 * which corresponds to a Time Signature meta-event in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Math */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to a Time Signature meta-event in the MIDI file spec.</summary>
    public sealed class MidiTimeSignatureEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiTimeSignatureEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiTimeSignatureEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x58, bytes, 7, index)
        {
            this.ValidateDataLength(4, Properties.Resources.TimeSignature);
            this._Numerator = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize);
            this._Denominator = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 1);
            this._ClocksPerClick = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 2);
            this._NotesPerQuarterNote = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 3);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.TimeSignature, this.DataLength);
            this.SetDataComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _Numerator;
        private uint _Denominator;
        private uint _ClocksPerClick;
        private uint _NotesPerQuarterNote;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>The numerator of the time signature.</summary>
        public uint Numerator
        {
            get { return this._Numerator; }

            set
            {
                MidiData.ValidateNumber(value, 0, byte.MaxValue, Properties.Resources.Numerator);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex);
                this._Numerator = value;
                this.SetDataComment();
            }
        }

        /// <summary>
        /// The denominator of the time signature, as a negative power of two
        /// (2 represents a quarter-note, 3 represents an eighth-note, etc.).
        /// </summary>
        public uint Denominator
        {
            get { return this._Denominator; }

            set
            {
                MidiData.ValidateNumber(value, 0, byte.MaxValue, Properties.Resources.Denominator);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 1);
                this._Denominator = value;
                this.SetDataComment();
            }
        }

        /// <summary>The number of MIDI clocks in a metronome click.</summary>
        public uint ClocksPerClick
        {
            get { return this._ClocksPerClick; }

            set
            {
                MidiData.ValidateNumber(value, 0, byte.MaxValue, Properties.Resources.ClocksPerClick);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 2);
                this._ClocksPerClick = value;
                this.SetDataComment();
            }
        }

        /// <summary>The number of notated 32nd-notes in what MIDI thinks of as a quarter-note (24 MIDI Clocks).</summary>
        public uint NotesPerQuarterNote
        {
            get { return this._NotesPerQuarterNote; }

            set
            {
                MidiData.ValidateNumber(value, 0, byte.MaxValue, Properties.Resources.NotesPerQuarterNote);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 3);
                this._NotesPerQuarterNote = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0}/{1} {2}, {3} {4}, {5} {6}",
                this.Numerator, Math.Pow(2, this.Denominator), Properties.Resources.Time,
                this.ClocksPerClick, Properties.Resources.ClocksPerClick,
                this.NotesPerQuarterNote, Properties.Resources.NotesPerQuarterNote);
            this.SetComment();
        }

        #endregion
    }
}
