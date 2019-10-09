/* MidiMetaEvent.cs - Implementation of MidiMetaEvent class, which corresponds to a meta-event in the MIDI file spec.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array, ApplicationException */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to a meta-event in the MIDI file spec.</summary>
    public class MidiMetaEvent : MidiEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiMetaEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>A strongly typed class must be used for a meta-event of known type.</remarks>
        public MidiMetaEvent(int deltaTime, uint type, byte[] bytes, int index)
            : base(deltaTime, 3) { this.Initialize(type, false, bytes, index); }

        #endregion

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiMetaEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="length">
        /// Minimum number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        protected MidiMetaEvent(int deltaTime, uint type, byte[] bytes, int length, int index)
            : base(deltaTime, length) { this.Initialize(type, true, bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected int DataLengthSize;

        #endregion

        #region Private Fields

        private uint _Type;
        private int _DataLength;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Meta-event type (always less than 128).</summary>
        public uint Type
        {
            get { return this._Type; }

            set
            {
                /* A strongly typed class must be used for a meta-event of known type. */
                if (value < 0x8 || value == 0x20 || value == 0x2F || value == 0x51 ||
                    value == 0x54 || value == 0x58 || value == 0x59 || value == 0x7F)
                    throw new ApplicationException(Properties.Resources.StronglyTypedClass);

                /* Otherwise, the type just must be less than 128. */
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.MetaEventType);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + 1);
                this._Type = value;
                this.TypeComment = string.Format(Properties.Resources.MetaEventFormat, value, this.DataLength);
                this.SetComment();
            }
        }

        /// <summary>Number of bytes which follow the data length (not including the data length itself).</summary>
        public int DataLength { get { return this._DataLength; } }

        #endregion

        #region Protected Properties

        /// <summary>Index in the byte array at which the data is written.</summary>
        protected int DataIndex { get { return this.DeltaTimeSize + 2 + this.DataLengthSize; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>
        /// Writes text as a string of ASCII characters into the byte array, resizing the byte array if necessary.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="index">Index in the byte array at which the text is written.</param>
        /// <param name="length">String length against which to compare the text for resizing purposes.</param>
        protected void StoreText(string text, int index, int length)
        {
            int i = text.Length - length;
            this.ResizeByteArray(i, index + length);
            MidiData.WriteText(text, this.Bytes, index);
        }

        /// <summary>Validates the data length of this meta-event against a minimum value.</summary>
        /// <param name="minLength">Minimum value for the data length.</param>
        /// <param name="description">A short description of the meta-event (for display purposes).</param>
        protected void ValidateDataLength(int minLength, string description)
        {
            if (this.DataLength >= minLength) return;

            string s = string.Format(Properties.Resources.MetaLengthFormat, description, minLength);
            throw new ApplicationException(s);
        }

        #endregion

        #region Private Methods

        private void Initialize(uint type, bool known, byte[] bytes, int index)
        {
            /* Read the data length (stored as a variable-length quantity)
             * from the passed byte array, and store it to our own byte array.
             */
            this._DataLength = MidiData.ReadVLQ(bytes, index);
            int n = this.DeltaTimeSize + 2;
            this.DataLengthSize = 1;
            this.StoreVLQ(this.DataLength, n, ref this.DataLengthSize);

            /* Wait until now to set the "status" and type bytes to ensure that the
             * comment gets set in the (probably rare) event of a zero data length.
             */
            MidiData.WriteNumber(0xFF, 1, this.Bytes, this.DeltaTimeSize);
            if (known)
            {
                MidiData.WriteNumber(type, 1, this.Bytes, this.DeltaTimeSize + 1);
                this._Type = type;
            }
            else this.Type = type;
            if (this.DataLength < 1) return;

            /* If necessary, resize the byte array. */
            n += this.DataLengthSize + this.DataLength;
            if (n > this.Bytes.Length) Array.Resize(ref this.Bytes, n);
            n -= this.DataLength;

            /* Copy the variable-length data (bytes). */
            Array.Copy(bytes, index + this.DataLengthSize, this.Bytes, n, this.DataLength);
            if (known) return;

            /* For unknown meta-event types, attempt to convert the data bytes to a string for the comment. */
            this.StringifyDataBytes(n);
        }

        #endregion
    }
}
