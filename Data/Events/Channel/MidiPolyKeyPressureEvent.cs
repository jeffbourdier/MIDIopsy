/* MidiPolyKeyPressureEvent.cs - Implementation of MidiPolyKeyPressureEvent class, which corresponds
 * to a "Polyphonic Key Pressure (Aftertouch)" channel message/event in the MIDI file spec.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


namespace JeffBourdier
{
    /// <summary>Corresponds to a "Polyphonic Key Pressure (Aftertouch)" channel message/event in the MIDI file spec.</summary>
    public class MidiPolyKeyPressureEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiPolyKeyPressureEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiPolyKeyPressureEvent(int deltaTime, uint channel, byte[] bytes, int index)
            : base(deltaTime, 0xA, channel, 3, Properties.Resources.PolyKeyPressure) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiPolyKeyPressureEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiPolyKeyPressureEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 2) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _NoteNumber;
        private uint _PressureValue;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Numeric value assigned to note (middle C has a reference value of 60).</summary>
        public uint NoteNumber
        {
            get { return this._NoteNumber; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.NoteNumber);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._NoteNumber = value;
                this.SetDataComment();
            }
        }

        /// <summary>Pressure value.</summary>
        public uint PressureValue
        {
            get { return this._PressureValue; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.PressureValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset + 1);
                this._PressureValue = value;
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
            this.NoteNumber = MidiData.ReadNumber(bytes, 1, index);
            this.PressureValue = MidiData.ReadNumber(bytes, 1, index + 1);
        }

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0}, {1} {2}", MidiData.NumberToNote(this.NoteNumber),
                Properties.Resources.PressureValue.ToLower(), this.PressureValue);
            this.SetComment();
        }

        #endregion
    }
}
