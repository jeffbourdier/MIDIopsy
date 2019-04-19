/* MidiChannelPressureEvent.cs - Implementation of MidiChannelPressureEvent class, which
 * corresponds to a "Channel Pressure (Aftertouch)" channel message/event in the MIDI file spec.
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
    /// <summary>Corresponds to a "Channel Pressure (Aftertouch)" channel message/event in the MIDI file spec.</summary>
    public class MidiChannelPressureEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChannelPressureEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiChannelPressureEvent(int deltaTime, uint channel, byte[] bytes, int index)
            : base(deltaTime, 0xD, channel, 2, Properties.Resources.ChannelPressure) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiChannelPressureEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiChannelPressureEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 1) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _PressureValue;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Pressure value.</summary>
        public uint PressureValue
        {
            get { return this._PressureValue; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.PressureValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._PressureValue = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void Initialize(byte[] bytes, int index) { this.PressureValue = MidiData.ReadNumber(bytes, 1, index); }

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0} {1}", Properties.Resources.PressureValue, this.PressureValue);
            this.SetComment();
        }

        #endregion
    }
}
