/* MidiPitchBendEvent.cs - Implementation of MidiPitchBendEvent class, which
 * corresponds to a "Pitch Bend Change" channel message/event in the MIDI file spec.
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
    /// <summary>Corresponds to a "Pitch Bend Change" channel message/event in the MIDI file spec.</summary>
    public class MidiPitchBendEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiPitchBendEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiPitchBendEvent(MidiTrackChunk owner, int deltaTime, uint channel, byte[] bytes, int index)
            : base(owner, deltaTime, 0xE, channel, 3, Properties.Resources.PitchBend) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiPitchBendEvent class using running status.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiPitchBendEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 2) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _LSB;
        private uint _MSB;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Least Significant Byte (LSB) of pitch bend change value.</summary>
        public uint LSB
        {
            get { return this._LSB; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.PitchBendChangeValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._LSB = value;
                this.SetDataComment();
            }
        }

        /// <summary>Most Significant Byte (MSB) of pitch bend change value.</summary>
        public uint MSB
        {
            get { return this._MSB; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.PitchBendChangeValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset + 1);
                this._MSB = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void Initialize(byte[] bytes, int index)
        {
            this.LSB = MidiData.ReadNumber(bytes, 1, index);
            this.MSB = MidiData.ReadNumber(bytes, 1, index + 1);
        }

        private void SetDataComment()
        {
            this.DataComment = string.Format("LSB {0}, MSB {1}", this.LSB, this.MSB);
            this.SetComment();
        }

        #endregion
    }
}
