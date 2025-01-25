/* ExtraItem.cs - Implementation of ExtraItem class, which represents a set of "leftover" bytes
 * that don't belong to any other item within a MIDI file (typically indicating file corruption).
 *
 * Copyright (c) 2025 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


namespace JeffBourdier
{
    /// <summary>
    /// Represents a set of "leftover" bytes that don't belong to any other
    /// item within a MIDI file (typically indicating file corruption).
    /// </summary>
    public sealed class ExtraItem : MidiItem
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChunkInfo class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        /// <param name="size">The number of bytes in this item.</param>
        public ExtraItem(MidiFile file, int offset, int size) : base(file, offset) { this._Size = size; }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private int _Size;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size { get { return this._Size; } }

        /// <summary>Gets the text to display in the "Count" field (i.e., the number of bytes).</summary>
        public override string CountField { get { return MidiItem.FormatCount(this.Size, Properties.Resources.Byte); } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public override string DataField { get { return this.File.ReadText(this.Offset, this.Size); } }

        #endregion
    }
}
