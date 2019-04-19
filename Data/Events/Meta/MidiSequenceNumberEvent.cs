/* MidiSequenceNumberEvent.cs - Implementation of MidiSequenceNumberEvent class,
 * which corresponds to a Sequence Number meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a Sequence Number meta-event in the MIDI file spec.</summary>
    public sealed class MidiSequenceNumberEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSequenceNumberEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSequenceNumberEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x00, bytes, 5, index)
        {
            this.ValidateDataLength(2, Properties.Resources.SequenceNumber);
            this._SequenceNumber = MidiData.ReadNumber(bytes, 2, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.SequenceNumber, this.DataLength);
            this.DataComment = this.SequenceNumber.ToString();
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _SequenceNumber;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Specifies the number of a sequence.</summary>
        public uint SequenceNumber
        {
            get { return this._SequenceNumber; }

            set
            {
                MidiData.ValidateNumber(value, 0, ushort.MaxValue, Properties.Resources.SequenceNumber);
                MidiData.WriteNumber(value, 2, this.Bytes, this.DataIndex);
                this._SequenceNumber = value;
                this.DataComment = value.ToString();
                this.SetComment();
            }
        }

        #endregion
    }
}
