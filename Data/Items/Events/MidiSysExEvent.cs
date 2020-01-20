/* MidiSysExEvent.cs - Implementation of MidiSysExEvent class, which corresponds
 * to a system exclusive (SysEx) message/event in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to a system exclusive (SysEx) message/event in the MIDI file spec.</summary>
    public sealed class MidiSysExEvent : MidiEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSysExEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="escape">
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSysExEvent(MidiTrackChunk owner, int deltaTime, bool escape, byte[] bytes, int index)
            : base(owner, deltaTime, 2)
        {
            /* Read the data length (stored as a variable-length quantity)
             * from the passed byte array, and store it to our own byte array.
             */
            this._DataLength = MidiData.ReadVLQ(bytes, index);
            int n = this.DeltaTimeSize + 1;
            this.DataLengthSize = 1;
            this.StoreVLQ(this.DataLength, n, ref this.DataLengthSize);

            /* Wait until now to set the "status" byte to ensure that the comment
             * gets set in the (probably rare) event of a zero data length.
             */
            this.Escape = escape;
            if (this.DataLength < 1) return;

            /* Resize the byte array and copy the variable-length data (bytes). */
            Array.Resize(ref this.Bytes, this.Bytes.Length + this.DataLength);
            n += this.DataLengthSize;
            Array.Copy(bytes, index + this.DataLengthSize, this.Bytes, n, this.DataLength);

            /* Attempt to convert the data bytes to a string for the comment. */
            this.StringifyDataBytes(n);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private bool _Escape;
        private int _DataLength;
        private int DataLengthSize;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </summary>
        public bool Escape
        {
            get { return this._Escape; }

            set
            {
                MidiData.WriteNumber((uint)(value ? 0xF7 : 0xF0), 1, this.Bytes, this.DeltaTimeSize);
                this._Escape = value;
                this.TypeComment = string.Format(Properties.Resources.SysExEventFormat,
                    value ? Properties.Resources.Escape : Properties.Resources.Message, this.DataLength);
                this.SetComment();
            }
        }

        /// <summary>Number of bytes which follow the data length (not including the data length itself).</summary>
        public int DataLength { get { return this._DataLength; } }

        #endregion
    }
}
