/* MidiKeySignatureEvent.cs - Implementation of MidiKeySignatureEvent class,
 * which corresponds to a Key Signature meta-event in the MIDI file spec.
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
    /// <summary>
    /// Specifies enumerated constants to define valid values for the Key Signature meta-event in the MIDI file spec.
    /// </summary>
    public enum MidiKeySignature : uint
    {
        CFlatMajor = 0xF900,
        GFlatMajor = 0xFA00,
        DFlatMajor = 0xFB00,
        AFlatMajor = 0xFC00,
        EFlatMajor = 0xFD00,
        BFlatMajor = 0xFE00,
        FMajor = 0xFF00,
        CMajor = 0x0000,
        GMajor = 0x0100,
        DMajor = 0x0200,
        AMajor = 0x0300,
        EMajor = 0x0400,
        BMajor = 0x0500,
        FSharpMajor = 0x0600,
        CSharpMajor = 0x0700,
        AFlatMinor = 0xF901,
        EFlatMinor = 0xFA01,
        BFlatMinor = 0xFB01,
        FMinor = 0xFC01,
        CMinor = 0xFD01,
        GMinor = 0xFE01,
        DMinor = 0xFF01,
        AMinor = 0x0001,
        EMinor = 0x0101,
        BMinor = 0x0201,
        FSharpMinor = 0x0301,
        CSharpMinor = 0x0401,
        GSharpMinor = 0x0501,
        DSharpMinor = 0x0601,
        ASharpMinor = 0x0701
    }

    /// <summary>Corresponds to a Key Signature meta-event in the MIDI file spec.</summary>
    public sealed class MidiKeySignatureEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiKeySignatureEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiKeySignatureEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x59, bytes, 5, index)
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
