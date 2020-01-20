/* MidiKeySignatureEvent.cs - Implementation of MidiKeySignatureEvent class,
 * which corresponds to a Key Signature meta-event in the MIDI file spec.
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
    /// <summary>
    /// Specifies enumerated constants to define valid values for the Key Signature meta-event in the MIDI file spec.
    /// </summary>
    public enum MidiKeySignature : uint               //     Key Signature       0   1   2   3   4   5   6   7   8   9  10  11
    {                                                 //  --------------------  --  --  --  --  --  --  --  --  --  --  --  --
        CFlatMajor  = 0xF900,  AFlatMinor  = 0xF901,  //  Bb Eb Ab Db Gb Cb Fb  Cb  C   Db  D   Eb  Fb  F   Gb  G   Ab  A   Bb
        GFlatMajor  = 0xFA00,  EFlatMinor  = 0xFA01,  //  Bb Eb Ab Db Gb Cb     Gb  G   Ab  A   Bb  Cb  C   Db  D   Eb  E   F
        DFlatMajor  = 0xFB00,  BFlatMinor  = 0xFB01,  //  Bb Eb Ab Db Gb        Db  D   Eb  E   F   Gb  G   Ab  A   Bb  B   C
        AFlatMajor  = 0xFC00,  FMinor      = 0xFC01,  //  Bb Eb Ab Db           Ab  A   Bb  B   C   Db  D   Eb  E   F   Gb  G
        EFlatMajor  = 0xFD00,  CMinor      = 0xFD01,  //  Bb Eb Ab              Eb  E   F   Gb  G   Ab  A   Bb  B   C   Db  D
        BFlatMajor  = 0xFE00,  GMinor      = 0xFE01,  //  Bb Eb                 Bb  B   C   Db  D   Eb  E   F   Gb* G   Ab  A
        FMajor      = 0xFF00,  DMinor      = 0xFF01,  //  Bb                    F   F#  G   Ab  A   Bb  B   C   Db* D   Eb  E
        CMajor      = 0x0000,  AMinor      = 0x0001,  //                        C   C#  D   Eb  E   F   F#  G   Ab* A   Bb  B
        GMajor      = 0x0100,  EMinor      = 0x0101,  //  F#                    G   G#  A   Bb  B   C   C#  D   Eb* E   F   F#
        DMajor      = 0x0200,  BMinor      = 0x0201,  //  F# C#                 D   D#  E   F   F#  G   G#  A   Bb* B   C   C#
        AMajor      = 0x0300,  FSharpMinor = 0x0301,  //  F# C# G#              A   A#  B   C   C#  D   D#  E   F*  F#  G   G#
        EMajor      = 0x0400,  CSharpMinor = 0x0401,  //  F# C# G# D#           E   E#  F#  G   G#  A   A#  B   C*  C#  D   D#
        BMajor      = 0x0500,  GSharpMinor = 0x0501,  //  F# C# G# D# A#        B   B#  C#  D   D#  E   E#  F#  G   G#  A   A#
        FSharpMajor = 0x0600,  DSharpMinor = 0x0601,  //  F# C# G# D# A# E#     F#  G   G#  A   A#  B   B#  C#  D   D#  E   E#
        CSharpMajor = 0x0700,  ASharpMinor = 0x0701,  //  F# C# G# D# A# E# B#  C#  D   D#  E   E#  F#  G   G#  A   A#  B   B#
        NA = 0xFFFFFFFF                               //                                 * Notate the other way for minor keys
    }

    /// <summary>Corresponds to a Key Signature meta-event in the MIDI file spec.</summary>
    public sealed class MidiKeySignatureEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiKeySignatureEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiKeySignatureEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x59, bytes, 5, index)
        {
            this.ValidateDataLength(2, Properties.Resources.KeySignature);
            this._KeySignature = (MidiKeySignature)MidiData.ReadNumber(bytes, 2, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.KeySignature, this.DataLength);
            this.SetDataComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private MidiKeySignature _KeySignature;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        public MidiKeySignature KeySignature
        {
            get { return this._KeySignature; }

            set
            {
                MidiData.WriteNumber((uint)value, 2, this.Bytes, this.DataIndex);
                this._KeySignature = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void SetDataComment()
        {
            this.DataComment = this.KeySignature.ToString();
            this.DataComment = this.DataComment.Replace("Flat", "b").Replace("Sharp", "#");
            this.DataComment = this.DataComment.Replace("Major", " major").Replace("Minor", " minor");
            this.SetComment();
        }

        #endregion
    }
}
