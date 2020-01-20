/* MidiChannelPrefixEvent.cs - Implementation of MidiChannelPrefixEvent class,
 * which corresponds to a MIDI Channel Prefix meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a MIDI Channel Prefix meta-event in the MIDI file spec.</summary>
    public sealed class MidiChannelPrefixEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChannelPrefixEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiChannelPrefixEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x20, bytes, 4, index)
        {
            this.ValidateDataLength(1, Properties.Resources.MidiChannelPrefix);
            this._Channel = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.MidiChannelPrefix, this.DataLength);
            this.DataComment = this.Channel.ToString();
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _Channel;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// MIDI channel (0-15) to associate with all events that follow, including System Exclusive and meta-events,
        /// until the next normal MIDI event (which contains a channel) or the next MIDI Channel Prefix meta-event.
        /// </summary>
        /// <remarks>
        /// If MIDI channels refer to "tracks", this may help jam several tracks into
        /// a format 0 file, keeping their non-MIDI data associated with a track.
        /// </remarks>
        public uint Channel
        {
            get { return this._Channel; }

            set
            {
                MidiData.ValidateNumber(value, 0, 0xF, Properties.Resources.Channel);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex);
                this._Channel = value;
                this.DataComment = value.ToString();
                this.SetComment();
            }
        }

        #endregion
    }
}
