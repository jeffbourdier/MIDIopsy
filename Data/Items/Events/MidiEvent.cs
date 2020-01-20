/* MidiEvent.cs - Implementation of MidiEvent class, which corresponds to an MTrk event in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array, Math */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to an MTrk event in the MIDI file spec.</summary>
    public abstract class MidiEvent : MidiItem
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="length">
        /// Minimum number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// </param>
        protected MidiEvent(MidiTrackChunk owner, int deltaTime, int length)
            : base(1 + length)
        {
            this._Track = owner;
            this.DeltaTime = deltaTime;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Protected Fields

        protected int DeltaTimeSize = 1;
        protected string TypeComment;
        protected string DataComment;

        #endregion

        #region Private Fields

        private MidiTrackChunk _Track;
        private int _DeltaTime;
        private int _CumulativeTime;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Representation of the byte array in hexadecimal format.</summary>
        public override string Hex { get { return new string(' ', (4 - this.DeltaTimeSize) * 3) + base.Hex; } }

        /// <summary>Gets the track (MTrk) chunk to which this event belongs (set by constructor).</summary>
        public MidiTrackChunk Track { get { return this._Track; } }

        /// <summary>The amount of time (in ticks) between the previous event in the track and this one.</summary>
        public int DeltaTime
        {
            get { return this._DeltaTime; }

            set
            {
                MidiData.ValidateNumber((uint)value, 0, MidiData.MaxVLQ, Properties.Resources.DeltaTime);
                this.StoreVLQ(value, 0, ref this.DeltaTimeSize);
                this._DeltaTime = value;
                this.SetComment();
            }
        }

        /// <summary>The amount of time (in ticks) between the beginning of the track and this event.</summary>
        /// <remarks>This should be greater than or equal to the delta-time.</remarks>
        public int CumulativeTime
        {
            get { return this._CumulativeTime; }
            set { this._CumulativeTime = value; }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>
        /// Writes a variable-length quantity (VLQ) into the byte array, validating
        /// the VLQ, updating its size, and resizing the byte array if necessary.
        /// </summary>
        /// <param name="n">The VLQ to write.</param>
        /// <param name="index">Index in the byte array at which the VLQ is written.</param>
        /// <param name="size">The number of bytes required to store the VLQ.</param>
        protected void StoreVLQ(int n, int index, ref int size)
        {
            int i = MidiData.SizeVLQ(n) - size;
            this.ResizeByteArray(i, index + size);
            size += i;
            MidiData.WriteVLQ(n, this.Bytes, index);
        }

        /// <summary>Resizes the byte array, preserving the data from a given index to the end.</summary>
        /// <param name="delta">The number of bytes by which to resize the array.</param>
        /// <param name="index">Index in the byte array at which to begin preservation.</param>
        protected void ResizeByteArray(int delta, int index)
        {
            int i, j, m, n;

            if (delta == 0) return;
            if (delta < 0)
            {
                m = index;
                n = this.Bytes.Length;
            }
            else
            {
                m = this.Bytes.Length - 1;
                n = index - 1;
                Array.Resize(ref this.Bytes, this.Bytes.Length + delta);
            }
            j = delta / Math.Abs(delta);
            for (i = m; i != n; i -= j) this.Bytes[i + delta] = this.Bytes[i];
            if (delta < 0) Array.Resize(ref this.Bytes, this.Bytes.Length + delta);
        }

        /// <summary>
        /// Attempt to convert the data in the byte array (starting at a given index, through the end)
        /// to a string for the "data" portion of the comment (the part following the last pipe bar).
        /// </summary>
        /// <param name="index">Index in the byte array at which to begin conversion.</param>
        protected void StringifyDataBytes(int index)
        {
            this.DataComment = string.Format("\"{0}\"", MidiData.ReadText(this.Bytes, this.Bytes.Length - index, index));
            this.SetComment();
        }

        /// <summary>Shortens the "data" portion of the comment (e.g., replace "number" with "no.").</summary>
        protected void ShortenDataComment()
        {
            if (string.IsNullOrEmpty(this.DataComment)) return;
            this.DataComment = this.DataComment.Replace(" number ", " no. ");
        }

        /// <summary>Sets the comment text for this event.  (Delta-time is handled automatically.)</summary>
        protected void SetComment()
        { this._Comment = string.Format("{0,9} | {1} | {2}", this.DeltaTime, this.TypeComment, this.DataComment); }

        #endregion
    }
}
