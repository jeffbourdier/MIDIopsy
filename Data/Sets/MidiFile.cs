/* MidiFile.cs - Implementation of MidiFile class, which represents a standard MIDI file.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
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
        public MidiFile(string filePath)
            : base(new List<MidiChunk>())
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            this.Replace(bytes);
        }

        /// <summary>Initializes a new instance of the MidiFile class with a MidiHeaderData object.</summary>
        /// <param name="data">A MidiHeaderData object (based on user input).</param>
        /// <remarks>
        /// A header (MThd) chunk, as well as the appropriate number of track (MTrk) chunks
        /// (with End of Track events), will be created and added to the file automatically.
        /// </remarks>
        public MidiFile(MidiHeaderData data)
            : base(new List<MidiChunk>(1 + (int)data.NumberOfTracks))
        {
            this.Header = new MidiHeaderChunk(this, data);
            this.AddChunk(this.Header);
            for (int i = 0; i < data.NumberOfTracks; ++i)
            {
                MidiTrackChunk track = new MidiTrackChunk(this);
                this.AddChunk(track);
            }
            this.AddIndex(-1);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private MidiHeaderChunk Header;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Specifies the overall organization of the MIDI file.</summary>
        public uint Format { get { return this.Header.Format; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

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
            this.Header = null;
            this.Clear();

            s = string.Format("parsing {0} bytes", bytes.Length);
            Logger.WriteMessage(s);

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
                    case "MThd":
                        chunk = new MidiHeaderChunk(this, n, bytes, i);
                        if (this.Header == null) this.Header = chunk as MidiHeaderChunk;
                        break;
                    case "MTrk": chunk = new MidiTrackChunk(this, n, bytes, i); break;
                    default: chunk = new MidiChunk(this, s, n, bytes, i); break;
                }
                this.AddChunk(chunk);
            }
            this.AddIndex(-1);

            /* Detect errors (track number mismatch). */
            n = (int)this.Header.NumberOfTracks;
            i = this.DataCount - 1;
            if (n != i)
            {
                s = string.Format(Properties.Resources.MismatchFormat, Properties.Resources.Track, n, i);
                Logger.WriteMessage(s);
                if (!string.IsNullOrEmpty(this.ErrorText)) s += Environment.NewLine;
                this.PrependErrorText(s);
            }
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

            if (!string.IsNullOrEmpty(chunk.ErrorText))
            {
                if (!string.IsNullOrEmpty(this.ErrorText)) this.AppendErrorText(Environment.NewLine);
                string s = Text.ChangeCase(Properties.Resources.Chunk, TextCase.Title);
                s = string.Format("{0} {1}", s, this.DataCount);
                if (chunk is MidiTrackChunk) s += string.Format(" ({0} {1})", Properties.Resources.Track, this.DataCount - 1);
                this.AppendErrorText(s + ":");
                this.AppendErrorText(chunk.ErrorText);
            }
        }

        #endregion
    }
}
