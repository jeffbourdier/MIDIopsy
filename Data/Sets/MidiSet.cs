/* MidiSet.cs - Implementation of MidiSet class, which represents a collection of MidiData objects (e.g., a file or a chunk).
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* IList */
using System.Collections;

/* List, Dictionary */
using System.Collections.Generic;


namespace JeffBourdier
{
    /// <summary>Represents a collection of MidiData objects (e.g., a file or a chunk).</summary>
    public abstract class MidiSet : MidiData
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiSet class.</summary>
        /// <param name="list">The list to contain MidiData objects.</param>
        protected MidiSet(IList list) { this.DataList = list; }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private IList DataList;
        private string _Hex = string.Empty;
        private string _Comments = string.Empty;
        private List<int> Indexes = new List<int>();
        private Dictionary<int, MidiKeySignature> KeySignatureMap = new Dictionary<int, MidiKeySignature>();

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of MidiData objects contained in this set.</summary>
        public int DataCount { get { return this.DataList.Count; } }

        /// <summary>Representation of all MidiData objects contained in this set, in hexadecimal format.</summary>
        public string Hex { get { return this._Hex; } }

        /// <summary>User-friendly text describing each MidiData object contained in this set.</summary>
        public string Comments { get { return this._Comments; } }

        /// <summary>Gets the number of lines (of text) within Hex/Comments.</summary>
        public int LineCount { get { return this.Indexes.Count / 2; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Gets the index of the initial character of the specified line within Hex or Comments.</summary>
        /// <param name="lineNumber">The zero-based line number of the character index to get.</param>
        /// <param name="hex">
        /// True for the character index from Hex; otherwise (for the character index from Comments), false.
        /// </param>
        /// <returns>The character index of the specified line.</returns>
        public int GetIndex(int lineNumber, bool hex) { return this.Indexes[lineNumber * 2 + (hex ? 0 : 1)]; }

        /// <summary>Adds an entry to this set's key signature map.</summary>
        /// <param name="keySignatureEvent">A MidiKeySignatureEvent object.</param>
        public void AddKeySignature(MidiKeySignatureEvent keySignatureEvent)
        { this.KeySignatureMap[keySignatureEvent.CumulativeTime] = keySignatureEvent.KeySignature; }

        /// <summary>Gets the key signature at a given (cumulative) time.</summary>
        /// <param name="time">The cumulative time (in ticks) at which to get the key signature.</param>
        /// <returns>The key signature at the specified time.</returns>
        public virtual MidiKeySignature GetKeySignature(int time)
        {
            MidiKeySignature keySignature = MidiKeySignature.NA;

            foreach (KeyValuePair<int, MidiKeySignature> pair in this.KeySignatureMap)
            {
                if (time < pair.Key) break;
                if (keySignature == MidiKeySignature.NA) keySignature = pair.Value;
            }
            return keySignature;
        }

        #endregion

        #region Protected Methods

        /// <summary>Gets the MidiData object at the specified index.</summary>
        /// <param name="index">The zero-based index of the MidiData object to get.</param>
        /// <returns>The MidiData object at the specified index.</returns>
        protected object GetData(int index) { return this.DataList[index]; }

        /// <summary>Adds a MidiData object to this set.</summary>
        /// <param name="data">The MidiData object to add.</param>
        protected void AddData(MidiData data) { this.DataList.Add(data); }

        protected void AddHex(string text) { this._Hex += text; }

        protected void AddComments(string text) { this._Comments += text; }

        protected void AddIndex(int lineNumber)
        {
            MidiChunk chunk = (lineNumber < 0) ? null : this.GetData(this.DataCount - 1) as MidiChunk;
            this.Indexes.Add(this.Hex.Length + ((chunk == null) ? 0 : chunk.GetIndex(lineNumber, true)));
            this.Indexes.Add(this.Comments.Length + ((chunk == null) ? 0 : chunk.GetIndex(lineNumber, false)));
        }

        protected void Clear()
        {
            this.ClearErrorText();
            this.Indexes.Clear();
            this._Comments = string.Empty;
            this._Hex = string.Empty;
            this.DataList.Clear();
        }

        #endregion
    }
}
