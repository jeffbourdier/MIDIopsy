/* MidiChannelEvent.cs - Implementation of MidiChannelEvent class, which corresponds
 * to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.
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
    /// <summary>Corresponds to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.</summary>
    public abstract class MidiChannelEvent : MidiEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiChannelEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="messageType">
        /// Identifies the type of channel message (corresponds to the high nibble of the event's status byte).
        /// </param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="length">
        /// Number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// This number should be 2 or 3 (i.e., 1 status byte plus 1 or 2 data bytes, depending on the message type).
        /// </param>
        /// <param name="description">A short description of the channel message (for display purposes).</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        protected MidiChannelEvent(int deltaTime, uint messageType, uint channel, int length, string description)
            : base(deltaTime, length)
        {
            MidiData.ValidateNumber(messageType, 0x8, 0xE, Properties.Resources.MessageType);
            this._RunningStatus = false;
            this.MessageTypeComment = description;
            this._MessageType = messageType;
            this.Channel = channel;
            this._DataOffset = 1;
        }

        /// <summary>Initializes a new instance of the MidiChannelEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="length">
        /// Number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// This number should be 1 or 2 (data bytes only; no status byte with running status).
        /// </param>
        protected MidiChannelEvent(int deltaTime, int length)
            : base(deltaTime, length)
        {
            this._RunningStatus = true;
            this._MessageType = uint.MaxValue;
            this._Channel = uint.MaxValue;
            this.TypeComment = new string(' ', 22);
            this.SetComment();
            this._DataOffset = 0;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private bool _RunningStatus;
        private string MessageTypeComment;
        private uint _MessageType;
        private uint _Channel;
        private int _DataOffset;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Representation of the byte array in hexadecimal format.</summary>
        public override string Hex
        {
            get
            {
                string s = base.Hex;
                return this.RunningStatus ? (s.Substring(0, 12) + "   " + s.Substring(12)) : s;
            }
        }

        /// <summary>Indicates whether or not this event was created with running status.</summary>
        public bool RunningStatus { get { return this._RunningStatus; } }

        /// <summary>Identifies the type of channel message, or uint.MaxValue to indicate running status.</summary>
        /// <remarks>This corresponds to the high nibble of the event's status byte.</remarks>
        public uint MessageType { get { return this._MessageType; } }

        /// <summary>
        /// One of the sixteen logical MIDI channels on which this event
        /// is transmitted, or uint.MaxValue to indicate running status.
        /// </summary>
        /// <remarks>This corresponds to the low nibble of the event's status byte.</remarks>
        public uint Channel
        {
            get { return this._Channel; }

            set
            {
                /* If this event uses running status, its channel cannot be changed arbitrarily. */
                if (this.RunningStatus) throw new ApplicationException(Properties.Resources.ChannelNotChanged);

                MidiData.ValidateNumber(value, 0, 0xF, Properties.Resources.Channel);
                MidiData.WriteNumber((this.MessageType << 4) | value, 1, this.Bytes, this.DeltaTimeSize);
                this._Channel = value;
                this.TypeComment = string.Format("{0,-17} | {1,2}", this.MessageTypeComment, this.Channel);
                this.SetComment();
            }
        }

        #endregion

        #region Protected Properties

        public int DataOffset { get { return this._DataOffset; } }

        #endregion
    }
}
