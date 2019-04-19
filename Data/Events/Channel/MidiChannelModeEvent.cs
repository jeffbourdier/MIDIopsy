/* MidiChannelModeEvent.cs - Implementation of MidiChannelModeEvent class, which
 * corresponds to a "Channel Mode" channel message/event in the MIDI file spec.
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
    /// <summary>Corresponds to a "Channel Mode" channel message/event in the MIDI file spec.</summary>
    public class MidiChannelModeEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChannelModeEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiChannelModeEvent(int deltaTime, uint channel, byte[] bytes, int index)
            : base(deltaTime, 0xB, channel, 3, Properties.Resources.ChannelMode) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiChannelModeEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiChannelModeEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 2) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _ControllerNumber;
        private string Description;
        private uint _ControlValue;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Controller number.</summary>
        public uint ControllerNumber
        {
            get { return this._ControllerNumber; }

            set
            {
                this.Description = MidiChannelModeEvent.GetDescription(value);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._ControllerNumber = value;
                this.SetDataComment();
            }
        }

        /// <summary>Control value.</summary>
        public uint ControlValue
        {
            get { return this._ControlValue; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.ControlValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset + 1);
                this._ControlValue = value;
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
            this.ControllerNumber = MidiData.ReadNumber(bytes, 1, index);
            this.ControlValue = MidiData.ReadNumber(bytes, 1, index + 1);
        }

        private static string GetDescription(uint n)
        {
            switch (n)
            {
                case 120: return Properties.Resources.Mode120;
                case 121: return Properties.Resources.Mode121;
                case 122: return Properties.Resources.Mode122;
                case 123: return Properties.Resources.Mode123;
                case 124: return Properties.Resources.Mode124;
                case 125: return Properties.Resources.Mode125;
                case 126: return Properties.Resources.Mode126;
                case 127: return Properties.Resources.Mode127;
            }
            string s = string.Format(Properties.Resources.ValueRangeFormat,
                Properties.Resources.ControllerNumber, 120, sbyte.MaxValue);
            throw new ApplicationException(s);
        }

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0} {1} ({2}), {3} {4}",
                Properties.Resources.ControllerNumber, this.ControllerNumber, this.Description,
                Properties.Resources.ControlValue.ToLower(), this.ControlValue);
            this.SetComment();
        }

        #endregion
    }
}
