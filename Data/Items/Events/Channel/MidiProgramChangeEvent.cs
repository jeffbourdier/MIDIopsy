/* MidiProgramChangeEvent.cs - Implementation of MidiProgramChangeEvent class, which
 * corresponds to a "Program Change" channel message/event in the MIDI file spec.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to a "Program Change" channel message/event in the MIDI file spec.</summary>
    public class MidiProgramChangeEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiProgramChangeEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiProgramChangeEvent(int deltaTime, uint channel, byte[] bytes, int index)
            : base(deltaTime, 0xC, channel, 2, Properties.Resources.ProgramChange) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiProgramChangeEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiProgramChangeEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 1) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _ProgramNumber;
        private string Instrument;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Program/patch number (instrument preset).</summary>
        /// <remarks>For more information, see General MIDI Sound Set.</remarks>
        public uint ProgramNumber
        {
            get { return this._ProgramNumber; }

            set
            {
                this.Instrument = MidiProgramChangeEvent.GetInstrument(value);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._ProgramNumber = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void Initialize(byte[] bytes, int index) { this.ProgramNumber = MidiData.ReadNumber(bytes, 1, index); }

        private static string GetInstrument(uint n)
        {
            switch (n)
            {
                case 0: return Properties.Resources.Sound0;
                case 1: return Properties.Resources.Sound1;
                case 2: return Properties.Resources.Sound2;
                case 3: return Properties.Resources.Sound3;
                case 4: return Properties.Resources.Sound4;
                case 5: return Properties.Resources.Sound5;
                case 6: return Properties.Resources.Sound6;
                case 7: return Properties.Resources.Sound7;
                case 8: return Properties.Resources.Sound8;
                case 9: return Properties.Resources.Sound9;
                case 10: return Properties.Resources.Sound10;
                case 11: return Properties.Resources.Sound11;
                case 12: return Properties.Resources.Sound12;
                case 13: return Properties.Resources.Sound13;
                case 14: return Properties.Resources.Sound14;
                case 15: return Properties.Resources.Sound15;
                case 16: return Properties.Resources.Sound16;
                case 17: return Properties.Resources.Sound17;
                case 18: return Properties.Resources.Sound18;
                case 19: return Properties.Resources.Sound19;
                case 20: return Properties.Resources.Sound20;
                case 21: return Properties.Resources.Sound21;
                case 22: return Properties.Resources.Sound22;
                case 23: return Properties.Resources.Sound23;
                case 24: return Properties.Resources.Sound24;
                case 25: return Properties.Resources.Sound25;
                case 26: return Properties.Resources.Sound26;
                case 27: return Properties.Resources.Sound27;
                case 28: return Properties.Resources.Sound28;
                case 29: return Properties.Resources.Sound29;
                case 30: return Properties.Resources.Sound30;
                case 31: return Properties.Resources.Sound31;
                case 32: return Properties.Resources.Sound32;
                case 33: return Properties.Resources.Sound33;
                case 34: return Properties.Resources.Sound34;
                case 35: return Properties.Resources.Sound35;
                case 36: return Properties.Resources.Sound36;
                case 37: return Properties.Resources.Sound37;
                case 38: return Properties.Resources.Sound38;
                case 39: return Properties.Resources.Sound39;
                case 40: return Properties.Resources.Sound40;
                case 41: return Properties.Resources.Sound41;
                case 42: return Properties.Resources.Sound42;
                case 43: return Properties.Resources.Sound43;
                case 44: return Properties.Resources.Sound44;
                case 45: return Properties.Resources.Sound45;
                case 46: return Properties.Resources.Sound46;
                case 47: return Properties.Resources.Sound47;
                case 48: return Properties.Resources.Sound48;
                case 49: return Properties.Resources.Sound49;
                case 50: return Properties.Resources.Sound50;
                case 51: return Properties.Resources.Sound51;
                case 52: return Properties.Resources.Sound52;
                case 53: return Properties.Resources.Sound53;
                case 54: return Properties.Resources.Sound54;
                case 55: return Properties.Resources.Sound55;
                case 56: return Properties.Resources.Sound56;
                case 57: return Properties.Resources.Sound57;
                case 58: return Properties.Resources.Sound58;
                case 59: return Properties.Resources.Sound59;
                case 60: return Properties.Resources.Sound60;
                case 61: return Properties.Resources.Sound61;
                case 62: return Properties.Resources.Sound62;
                case 63: return Properties.Resources.Sound63;
                case 64: return Properties.Resources.Sound64;
                case 65: return Properties.Resources.Sound65;
                case 66: return Properties.Resources.Sound66;
                case 67: return Properties.Resources.Sound67;
                case 68: return Properties.Resources.Sound68;
                case 69: return Properties.Resources.Sound69;
                case 70: return Properties.Resources.Sound70;
                case 71: return Properties.Resources.Sound71;
                case 72: return Properties.Resources.Sound72;
                case 73: return Properties.Resources.Sound73;
                case 74: return Properties.Resources.Sound74;
                case 75: return Properties.Resources.Sound75;
                case 76: return Properties.Resources.Sound76;
                case 77: return Properties.Resources.Sound77;
                case 78: return Properties.Resources.Sound78;
                case 79: return Properties.Resources.Sound79;
                case 80: return Properties.Resources.Sound80;
                case 81: return Properties.Resources.Sound81;
                case 82: return Properties.Resources.Sound82;
                case 83: return Properties.Resources.Sound83;
                case 84: return Properties.Resources.Sound84;
                case 85: return Properties.Resources.Sound85;
                case 86: return Properties.Resources.Sound86;
                case 87: return Properties.Resources.Sound87;
                case 88: return Properties.Resources.Sound88;
                case 89: return Properties.Resources.Sound89;
                case 90: return Properties.Resources.Sound90;
                case 91: return Properties.Resources.Sound91;
                case 92: return Properties.Resources.Sound92;
                case 93: return Properties.Resources.Sound93;
                case 94: return Properties.Resources.Sound94;
                case 95: return Properties.Resources.Sound95;
                case 96: return Properties.Resources.Sound96;
                case 97: return Properties.Resources.Sound97;
                case 98: return Properties.Resources.Sound98;
                case 99: return Properties.Resources.Sound99;
                case 100: return Properties.Resources.Sound100;
                case 101: return Properties.Resources.Sound101;
                case 102: return Properties.Resources.Sound102;
                case 103: return Properties.Resources.Sound103;
                case 104: return Properties.Resources.Sound104;
                case 105: return Properties.Resources.Sound105;
                case 106: return Properties.Resources.Sound106;
                case 107: return Properties.Resources.Sound107;
                case 108: return Properties.Resources.Sound108;
                case 109: return Properties.Resources.Sound109;
                case 110: return Properties.Resources.Sound110;
                case 111: return Properties.Resources.Sound111;
                case 112: return Properties.Resources.Sound112;
                case 113: return Properties.Resources.Sound113;
                case 114: return Properties.Resources.Sound114;
                case 115: return Properties.Resources.Sound115;
                case 116: return Properties.Resources.Sound116;
                case 117: return Properties.Resources.Sound117;
                case 118: return Properties.Resources.Sound118;
                case 119: return Properties.Resources.Sound119;
                case 120: return Properties.Resources.Sound120;
                case 121: return Properties.Resources.Sound121;
                case 122: return Properties.Resources.Sound122;
                case 123: return Properties.Resources.Sound123;
                case 124: return Properties.Resources.Sound124;
                case 125: return Properties.Resources.Sound125;
                case 126: return Properties.Resources.Sound126;
                case 127: return Properties.Resources.Sound127;
            }
            string s = string.Format(Common.Resources.ValueRangeFormat, Properties.Resources.ProgramNumber, 0, sbyte.MaxValue);
            throw new ApplicationException(s);
        }

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0} {1} ({2})",
                Properties.Resources.ProgramNumber, this.ProgramNumber, this.Instrument);
            this.SetComment();
        }

        #endregion
    }
}
