/* MidiFile.cs - Implementation of MidiFile class, which represents a standard MIDI file.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Array */
using System;

/* List */
using System.Collections.Generic;

/* File */
using System.IO;


namespace JeffBourdier
{
    /// <summary>Represents a standard MIDI file.</summary>
    public class MidiFile
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiFile class by loading an existing MIDI file from disk.</summary>
        /// <param name="filePath">The path of the MIDI file.</param>
        public MidiFile(string filePath)
        {
            /* Read all data from the MIDI file. */
            byte[] bytes = File.ReadAllBytes(filePath);

            /* A MIDI file is made up of chunks. */
            this.Chunks = new List<MidiChunk>();
            this.Replace(bytes);
        }

        /// <summary>Initializes a new instance of the MidiFile class with a header (MThd) chunk.</summary>
        /// <param name="header">MidiHeaderChunk object representing the header chunk.</param>
        public MidiFile(MidiHeaderChunk header)
        {
            this.Chunks = new List<MidiChunk>(1);
            this.AddChunk(header);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private List<MidiChunk> Chunks;
        private string _Hex;
        private string _Comments;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of chunks contained in the MIDI file.</summary>
        public int ChunkCount { get { return this.Chunks.Count; } }

        /// <summary>Representation of all chunks contained in this MIDI file, in hexadecimal format.</summary>
        public string Hex { get { return this._Hex; } }

        /// <summary>Human-readable text describing each chunk contained in this MIDI file.</summary>
        public string Comments { get { return this._Comments; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Gets the chunk at the specified index.</summary>
        /// <param name="index">The zero-based index of the chunk to get.</param>
        /// <returns>The chunk at the specified index.</returns>
        public MidiChunk GetChunk(int index) { return this.Chunks[index]; }

        /// <summary>Replaces the chunks of this MIDI file with those contained in a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the chunks.</param>
        public void Replace(byte[] bytes)
        {
            int n, i;
            string s;
            MidiChunk chunk;

            /* Make sure we start with a clean slate. */
            this._Comments = null;
            this._Hex = null;
            this.Chunks.Clear();

            /* Process each chunk from the byte array. */
            for (i = 0; i < bytes.Length; i += n)
            {
                /* Parse out the four-character chunk type. */
                s = MidiData.ReadText(bytes, 4, i);
                i += 4;

                /* Parse out the length of the chunk.  (Note that this does not include the first eight bytes). */
                n = (int)MidiData.ReadNumber(bytes, 4, i);
                i += 4;

                /* Instantiate a chunk object of the appropriate type. */
                switch (s)
                {
                    case "MThd": chunk = new MidiHeaderChunk(n, bytes, i); break;
                    case "MTrk": chunk = new MidiTrackChunk(n, bytes, i); break;
                    default: chunk = new MidiChunk(s, n, bytes, i); break;
                }
                this.AddChunk(chunk);
            }
        }

        /// <summary>Writes this MIDI file to disk at the specified path.</summary>
        /// <param name="filePath">The path at which to write the MIDI file.</param>
        public void WriteToDisk(string filePath)
        {
            int i = 0, j;
            byte[] bytes = new byte[0];
            MidiData data;

            /* Iterate through each MidiData object of each chunk, copying its bytes into our own local array. */
            foreach (MidiChunk chunk in this.Chunks)
                for (j = 0; j < chunk.DataCount; ++j)
                {
                    data = chunk.GetData(j);
                    Array.Resize(ref bytes, bytes.Length + data.Length);
                    data.CopyBytes(bytes, i);
                    i += data.Length;
                }

            /* Write the data from our local byte array into the named MIDI file on disk. */
            File.WriteAllBytes(filePath, bytes);
        }

        #endregion

        #region Private Methods

        private void AddChunk(MidiChunk chunk)
        {
            this.Chunks.Add(chunk);
            this._Hex += chunk.Hex + Environment.NewLine;
            this._Comments += chunk.Comments + Environment.NewLine;
        }

        #endregion
    }
}
