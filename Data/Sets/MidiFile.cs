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


/* Array, Environment */
using System;

/* List */
using System.Collections.Generic;

/* File */
using System.IO;


namespace JeffBourdier
{
    /// <summary>Represents a standard MIDI file (essentially, a collection of MidiChunk objects).</summary>
    public class MidiFile : MidiSet
    {

 
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiFile class by loading an existing MIDI file from disk.</summary>
        /// <param name="filePath">The path of the MIDI file.</param>
        public MidiFile(string filePath) : base(new List<MidiChunk>())
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            try
            {
                this.Replace(bytes);
            }
            catch (Exception ex)
            {
                CreationException = ex;
            }
        }

        /// <summary>Initializes a new instance of the MidiFile class with a header (MThd) chunk.</summary>
        /// <param name="header">MidiHeaderChunk object representing the header chunk.</param>
        public MidiFile(MidiHeaderChunk header) : base(new List<MidiChunk>(1))
        {
            this.AddChunk(header);
            this.AddIndex(-1);
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        public Exception CreationException { get; set; }


        /// <summary>Gets the chunk at the specified index.</summary>
        /// <param name="index">The zero-based index of the chunk to get.</param>
        /// <returns>The chunk at the specified index.</returns>
        public MidiChunk GetChunk(int index) { return this.GetData(index) as MidiChunk; }

        /// <summary>Replaces the chunks of this MIDI file with those contained in a byte array.</summary>
        /// <param name="bytes">Array of bytes containing the chunks.</param>
        public void Replace(byte[] bytes)
        {
            int n, i;
            string s;
            MidiChunk chunk;

            /* Make sure we start with a clean slate. */
            this.Clear();

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
            this.AddIndex(-1);
        }

        /// <summary>Writes this MIDI file to disk at the specified path.</summary>
        /// <param name="filePath">The path at which to write the MIDI file.</param>
        public void WriteToDisk(string filePath)
        {
            int i, j, n = 0;
            MidiChunk chunk;
            MidiItem item;
            byte[] bytes = new byte[0];

            /* Iterate through each MidiData object of each chunk, copying its bytes into our own local array. */
            for (i = 0; i < this.DataCount; ++i)
            {
                chunk = this.GetChunk(i);
                for (j = 0; j < chunk.DataCount; ++j)
                {
                    item = chunk.GetItem(j);
                    Array.Resize(ref bytes, bytes.Length + item.Length);
                    item.CopyBytes(bytes, n);
                    n += item.Length;
                }
            }

            /* Write the data from our local byte array into the named MIDI file on disk. */
            File.WriteAllBytes(filePath, bytes);
        }

        #endregion

        #region Private Methods

        private void AddChunk(MidiChunk chunk)
        {
            this.AddData(chunk);
            if (this.DataCount > 1)
            {
                this.AddIndex(-1);
                this.AddHex(Environment.NewLine);
                this.AddComments(Environment.NewLine);
            }
            for (int i = 0; i < chunk.LineCount; this.AddIndex(i++)) ;
            this.AddHex(chunk.Hex);
            this.AddComments(chunk.Comments);
        }

        #endregion
    }
}
