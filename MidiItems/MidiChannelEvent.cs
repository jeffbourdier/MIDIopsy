/* MidiChannelEvent.cs - Implementation of MidiChannelEvent class, which corresponds
 * to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Dictionary */
using System.Collections.Generic;


namespace JeffBourdier
{
    /// <summary>Specifies enumerated constants to define valid values for types of MIDI channel messages.</summary>
    public enum MidiMessageType
    {
        NoteOff = 0x8,
        NoteOn = 0x9,
        PolyKeyPressure = 0xA,
        ControlChangeOrChannelMode = 0xB,
        ProgramChange = 0xC,
        ChannelPressure = 0xD,
        PitchBend = 0xE,
        NA = 0xF
    }

    /// <summary>Corresponds to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.</summary>
    public sealed class MidiChannelEvent : MidiEvent
    {
        /*********
         * Types *
         *********/

        #region Private Types

        /// <summary>Describes a data byte of a channel message/event.</summary>
        /// <param name="data">The data byte.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which the event is transmitted.</param>
        /// <param name="keySignature">A MidiKeySignature value that applies to the event.</param>
        /// <returns>A description of the data byte.</returns>
        private delegate string DescribeData(int data, int channel, MidiKeySignature keySignature);

        /// <summary>Contains information on a type of channel message.</summary>
        private struct TypeInfo
        {
            public TypeInfo(string comment, string data1Name, string data2Name,
                DescribeData describeData1, DescribeData describeData2)
            {
                this._Comment = comment;
                this._Data1Name = data1Name;
                this._Data2Name = data2Name;
                this._DescribeData1 = describeData1;
                this._DescribeData2 = describeData2;
            }

            private string _Comment;
            private string _Data1Name;
            private string _Data2Name;
            private DescribeData _DescribeData1;
            private DescribeData _DescribeData2;

            public string Comment { get { return this._Comment; } }
            public string Data1Name { get { return this._Data1Name; } }
            public string Data2Name { get { return this._Data2Name; } }
            public DescribeData DescribeData1 { get { return this._DescribeData1; } }
            public DescribeData DescribeData2 { get { return this._DescribeData2; } }
        }

        #endregion

        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static MidiChannelEvent()
        {
            int i;

            MidiChannelEvent.TypeMap = new Dictionary<MidiMessageType, TypeInfo>(7);

            MidiChannelEvent.TypeMap.Add(MidiMessageType.NoteOff,
                new TypeInfo(Properties.Resources.NoteOff, Properties.Resources.NoteNumber,
                    Properties.Resources.Velocity, MidiChannelEvent.DescribeNote, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.NoteOn,
                new TypeInfo(Properties.Resources.NoteOn, Properties.Resources.NoteNumber,
                    Properties.Resources.Velocity, MidiChannelEvent.DescribeNote, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.PolyKeyPressure,
                new TypeInfo(Properties.Resources.PolyKeyPressure, Properties.Resources.NoteNumber,
                    Properties.Resources.PressureValue, MidiChannelEvent.DescribeNote, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.ControlChangeOrChannelMode,
                new TypeInfo(Properties.Resources.ControlChange + " / " + Properties.Resources.ChannelMode,
                    Properties.Resources.ControllerNumber, Properties.Resources.ControlValue,
                    MidiChannelEvent.DescribeController, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.ProgramChange,
                new TypeInfo(Properties.Resources.ProgramChange, Properties.Resources.ProgramNumber,
                    null, MidiChannelEvent.DescribeInstrument, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.ChannelPressure,
                new TypeInfo(Properties.Resources.ChannelPressure, Properties.Resources.PressureValue, null, null, null));

            MidiChannelEvent.TypeMap.Add(MidiMessageType.PitchBend,
                new TypeInfo(Properties.Resources.PitchBend, "LSB", "MSB", null, null));

            MidiChannelEvent.PercussionMap = new Dictionary<int, string>(47);
            MidiChannelEvent.PercussionMap.Add(35, Properties.Resources.Percussion35);
            MidiChannelEvent.PercussionMap.Add(36, Properties.Resources.Percussion36);
            MidiChannelEvent.PercussionMap.Add(37, Properties.Resources.Percussion37);
            MidiChannelEvent.PercussionMap.Add(38, Properties.Resources.Percussion38);
            MidiChannelEvent.PercussionMap.Add(39, Properties.Resources.Percussion39);
            MidiChannelEvent.PercussionMap.Add(40, Properties.Resources.Percussion40);
            MidiChannelEvent.PercussionMap.Add(41, Properties.Resources.Percussion41);
            MidiChannelEvent.PercussionMap.Add(42, Properties.Resources.Percussion42);
            MidiChannelEvent.PercussionMap.Add(43, Properties.Resources.Percussion43);
            MidiChannelEvent.PercussionMap.Add(44, Properties.Resources.Percussion44);
            MidiChannelEvent.PercussionMap.Add(45, Properties.Resources.Percussion45);
            MidiChannelEvent.PercussionMap.Add(46, Properties.Resources.Percussion46);
            MidiChannelEvent.PercussionMap.Add(47, Properties.Resources.Percussion47);
            MidiChannelEvent.PercussionMap.Add(48, Properties.Resources.Percussion48);
            MidiChannelEvent.PercussionMap.Add(49, Properties.Resources.Percussion49);
            MidiChannelEvent.PercussionMap.Add(50, Properties.Resources.Percussion50);
            MidiChannelEvent.PercussionMap.Add(51, Properties.Resources.Percussion51);
            MidiChannelEvent.PercussionMap.Add(52, Properties.Resources.Percussion52);
            MidiChannelEvent.PercussionMap.Add(53, Properties.Resources.Percussion53);
            MidiChannelEvent.PercussionMap.Add(54, Properties.Resources.Percussion54);
            MidiChannelEvent.PercussionMap.Add(55, Properties.Resources.Percussion55);
            MidiChannelEvent.PercussionMap.Add(56, Properties.Resources.Percussion56);
            MidiChannelEvent.PercussionMap.Add(57, Properties.Resources.Percussion57);
            MidiChannelEvent.PercussionMap.Add(58, Properties.Resources.Percussion58);
            MidiChannelEvent.PercussionMap.Add(59, Properties.Resources.Percussion59);
            MidiChannelEvent.PercussionMap.Add(60, Properties.Resources.Percussion60);
            MidiChannelEvent.PercussionMap.Add(61, Properties.Resources.Percussion61);
            MidiChannelEvent.PercussionMap.Add(62, Properties.Resources.Percussion62);
            MidiChannelEvent.PercussionMap.Add(63, Properties.Resources.Percussion63);
            MidiChannelEvent.PercussionMap.Add(64, Properties.Resources.Percussion64);
            MidiChannelEvent.PercussionMap.Add(65, Properties.Resources.Percussion65);
            MidiChannelEvent.PercussionMap.Add(66, Properties.Resources.Percussion66);
            MidiChannelEvent.PercussionMap.Add(67, Properties.Resources.Percussion67);
            MidiChannelEvent.PercussionMap.Add(68, Properties.Resources.Percussion68);
            MidiChannelEvent.PercussionMap.Add(69, Properties.Resources.Percussion69);
            MidiChannelEvent.PercussionMap.Add(70, Properties.Resources.Percussion70);
            MidiChannelEvent.PercussionMap.Add(71, Properties.Resources.Percussion71);
            MidiChannelEvent.PercussionMap.Add(72, Properties.Resources.Percussion72);
            MidiChannelEvent.PercussionMap.Add(73, Properties.Resources.Percussion73);
            MidiChannelEvent.PercussionMap.Add(74, Properties.Resources.Percussion74);
            MidiChannelEvent.PercussionMap.Add(75, Properties.Resources.Percussion75);
            MidiChannelEvent.PercussionMap.Add(76, Properties.Resources.Percussion76);
            MidiChannelEvent.PercussionMap.Add(77, Properties.Resources.Percussion77);
            MidiChannelEvent.PercussionMap.Add(78, Properties.Resources.Percussion78);
            MidiChannelEvent.PercussionMap.Add(79, Properties.Resources.Percussion79);
            MidiChannelEvent.PercussionMap.Add(80, Properties.Resources.Percussion80);
            MidiChannelEvent.PercussionMap.Add(81, Properties.Resources.Percussion81);

            MidiChannelEvent.ControllerMap = new Dictionary<int, string>(128);
            MidiChannelEvent.ControllerMap.Add(0, Properties.Resources.Controller0);
            MidiChannelEvent.ControllerMap.Add(1, Properties.Resources.Controller1);
            MidiChannelEvent.ControllerMap.Add(2, Properties.Resources.Controller2);
            MidiChannelEvent.ControllerMap.Add(3, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(4, Properties.Resources.Controller4);
            MidiChannelEvent.ControllerMap.Add(5, Properties.Resources.Controller5);
            MidiChannelEvent.ControllerMap.Add(6, Properties.Resources.Controller6);
            MidiChannelEvent.ControllerMap.Add(7, Properties.Resources.Controller7);
            MidiChannelEvent.ControllerMap.Add(8, Properties.Resources.Controller8);
            MidiChannelEvent.ControllerMap.Add(9, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(10, Properties.Resources.Controller10);
            MidiChannelEvent.ControllerMap.Add(11, Properties.Resources.Controller11);
            MidiChannelEvent.ControllerMap.Add(12, Properties.Resources.Controller12);
            MidiChannelEvent.ControllerMap.Add(13, Properties.Resources.Controller13);
            MidiChannelEvent.ControllerMap.Add(14, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(15, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(16, Properties.Resources.Controller16);
            MidiChannelEvent.ControllerMap.Add(17, Properties.Resources.Controller17);
            MidiChannelEvent.ControllerMap.Add(18, Properties.Resources.Controller18);
            MidiChannelEvent.ControllerMap.Add(19, Properties.Resources.Controller19);
            for (i = 20; i < 32; ++i) MidiChannelEvent.ControllerMap.Add(i, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(32, Properties.Resources.Controller32);
            MidiChannelEvent.ControllerMap.Add(33, Properties.Resources.Controller33);
            MidiChannelEvent.ControllerMap.Add(34, Properties.Resources.Controller34);
            MidiChannelEvent.ControllerMap.Add(35, Properties.Resources.Controller35);
            MidiChannelEvent.ControllerMap.Add(36, Properties.Resources.Controller36);
            MidiChannelEvent.ControllerMap.Add(37, Properties.Resources.Controller37);
            MidiChannelEvent.ControllerMap.Add(38, Properties.Resources.Controller38);
            MidiChannelEvent.ControllerMap.Add(39, Properties.Resources.Controller39);
            MidiChannelEvent.ControllerMap.Add(40, Properties.Resources.Controller40);
            MidiChannelEvent.ControllerMap.Add(41, Properties.Resources.Controller41);
            MidiChannelEvent.ControllerMap.Add(42, Properties.Resources.Controller42);
            MidiChannelEvent.ControllerMap.Add(43, Properties.Resources.Controller43);
            MidiChannelEvent.ControllerMap.Add(44, Properties.Resources.Controller44);
            MidiChannelEvent.ControllerMap.Add(45, Properties.Resources.Controller45);
            MidiChannelEvent.ControllerMap.Add(46, Properties.Resources.Controller46);
            MidiChannelEvent.ControllerMap.Add(47, Properties.Resources.Controller47);
            MidiChannelEvent.ControllerMap.Add(48, Properties.Resources.Controller48);
            MidiChannelEvent.ControllerMap.Add(49, Properties.Resources.Controller49);
            MidiChannelEvent.ControllerMap.Add(50, Properties.Resources.Controller50);
            MidiChannelEvent.ControllerMap.Add(51, Properties.Resources.Controller51);
            MidiChannelEvent.ControllerMap.Add(52, Properties.Resources.Controller52);
            MidiChannelEvent.ControllerMap.Add(53, Properties.Resources.Controller53);
            MidiChannelEvent.ControllerMap.Add(54, Properties.Resources.Controller54);
            MidiChannelEvent.ControllerMap.Add(55, Properties.Resources.Controller55);
            MidiChannelEvent.ControllerMap.Add(56, Properties.Resources.Controller56);
            MidiChannelEvent.ControllerMap.Add(57, Properties.Resources.Controller57);
            MidiChannelEvent.ControllerMap.Add(58, Properties.Resources.Controller58);
            MidiChannelEvent.ControllerMap.Add(59, Properties.Resources.Controller59);
            MidiChannelEvent.ControllerMap.Add(60, Properties.Resources.Controller60);
            MidiChannelEvent.ControllerMap.Add(61, Properties.Resources.Controller61);
            MidiChannelEvent.ControllerMap.Add(62, Properties.Resources.Controller62);
            MidiChannelEvent.ControllerMap.Add(63, Properties.Resources.Controller63);
            MidiChannelEvent.ControllerMap.Add(64, Properties.Resources.Controller64);
            MidiChannelEvent.ControllerMap.Add(65, Properties.Resources.Controller65);
            MidiChannelEvent.ControllerMap.Add(66, Properties.Resources.Controller66);
            MidiChannelEvent.ControllerMap.Add(67, Properties.Resources.Controller67);
            MidiChannelEvent.ControllerMap.Add(68, Properties.Resources.Controller68);
            MidiChannelEvent.ControllerMap.Add(69, Properties.Resources.Controller69);
            MidiChannelEvent.ControllerMap.Add(70, Properties.Resources.Controller70);
            MidiChannelEvent.ControllerMap.Add(71, Properties.Resources.Controller71);
            MidiChannelEvent.ControllerMap.Add(72, Properties.Resources.Controller72);
            MidiChannelEvent.ControllerMap.Add(73, Properties.Resources.Controller73);
            MidiChannelEvent.ControllerMap.Add(74, Properties.Resources.Controller74);
            MidiChannelEvent.ControllerMap.Add(75, Properties.Resources.Controller75);
            MidiChannelEvent.ControllerMap.Add(76, Properties.Resources.Controller76);
            MidiChannelEvent.ControllerMap.Add(77, Properties.Resources.Controller77);
            MidiChannelEvent.ControllerMap.Add(78, Properties.Resources.Controller78);
            MidiChannelEvent.ControllerMap.Add(79, Properties.Resources.Controller79);
            MidiChannelEvent.ControllerMap.Add(80, Properties.Resources.Controller80);
            MidiChannelEvent.ControllerMap.Add(81, Properties.Resources.Controller81);
            MidiChannelEvent.ControllerMap.Add(82, Properties.Resources.Controller82);
            MidiChannelEvent.ControllerMap.Add(83, Properties.Resources.Controller83);
            MidiChannelEvent.ControllerMap.Add(84, Properties.Resources.Controller84);
            for (i = 85; i < 91; ++i) MidiChannelEvent.ControllerMap.Add(i, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(91, Properties.Resources.Controller91);
            MidiChannelEvent.ControllerMap.Add(92, Properties.Resources.Controller92);
            MidiChannelEvent.ControllerMap.Add(93, Properties.Resources.Controller93);
            MidiChannelEvent.ControllerMap.Add(94, Properties.Resources.Controller94);
            MidiChannelEvent.ControllerMap.Add(95, Properties.Resources.Controller95);
            MidiChannelEvent.ControllerMap.Add(96, Properties.Resources.Controller96);
            MidiChannelEvent.ControllerMap.Add(97, Properties.Resources.Controller97);
            MidiChannelEvent.ControllerMap.Add(98, Properties.Resources.Controller98);
            MidiChannelEvent.ControllerMap.Add(99, Properties.Resources.Controller99);
            MidiChannelEvent.ControllerMap.Add(100, Properties.Resources.Controller100);
            MidiChannelEvent.ControllerMap.Add(101, Properties.Resources.Controller101);
            for (i = 102; i < 120; ++i) MidiChannelEvent.ControllerMap.Add(i, Properties.Resources.Undefined);
            MidiChannelEvent.ControllerMap.Add(120, Properties.Resources.Mode120);
            MidiChannelEvent.ControllerMap.Add(121, Properties.Resources.Mode121);
            MidiChannelEvent.ControllerMap.Add(122, Properties.Resources.Mode122);
            MidiChannelEvent.ControllerMap.Add(123, Properties.Resources.Mode123);
            MidiChannelEvent.ControllerMap.Add(124, Properties.Resources.Mode124);
            MidiChannelEvent.ControllerMap.Add(125, Properties.Resources.Mode125);
            MidiChannelEvent.ControllerMap.Add(126, Properties.Resources.Mode126);
            MidiChannelEvent.ControllerMap.Add(127, Properties.Resources.Mode127);

            MidiChannelEvent.InstrumentMap = new Dictionary<int, string>(128);
            MidiChannelEvent.InstrumentMap.Add(0, Properties.Resources.Sound0);
            MidiChannelEvent.InstrumentMap.Add(1, Properties.Resources.Sound1);
            MidiChannelEvent.InstrumentMap.Add(2, Properties.Resources.Sound2);
            MidiChannelEvent.InstrumentMap.Add(3, Properties.Resources.Sound3);
            MidiChannelEvent.InstrumentMap.Add(4, Properties.Resources.Sound4);
            MidiChannelEvent.InstrumentMap.Add(5, Properties.Resources.Sound5);
            MidiChannelEvent.InstrumentMap.Add(6, Properties.Resources.Sound6);
            MidiChannelEvent.InstrumentMap.Add(7, Properties.Resources.Sound7);
            MidiChannelEvent.InstrumentMap.Add(8, Properties.Resources.Sound8);
            MidiChannelEvent.InstrumentMap.Add(9, Properties.Resources.Sound9);
            MidiChannelEvent.InstrumentMap.Add(10, Properties.Resources.Sound10);
            MidiChannelEvent.InstrumentMap.Add(11, Properties.Resources.Sound11);
            MidiChannelEvent.InstrumentMap.Add(12, Properties.Resources.Sound12);
            MidiChannelEvent.InstrumentMap.Add(13, Properties.Resources.Sound13);
            MidiChannelEvent.InstrumentMap.Add(14, Properties.Resources.Sound14);
            MidiChannelEvent.InstrumentMap.Add(15, Properties.Resources.Sound15);
            MidiChannelEvent.InstrumentMap.Add(16, Properties.Resources.Sound16);
            MidiChannelEvent.InstrumentMap.Add(17, Properties.Resources.Sound17);
            MidiChannelEvent.InstrumentMap.Add(18, Properties.Resources.Sound18);
            MidiChannelEvent.InstrumentMap.Add(19, Properties.Resources.Sound19);
            MidiChannelEvent.InstrumentMap.Add(20, Properties.Resources.Sound20);
            MidiChannelEvent.InstrumentMap.Add(21, Properties.Resources.Sound21);
            MidiChannelEvent.InstrumentMap.Add(22, Properties.Resources.Sound22);
            MidiChannelEvent.InstrumentMap.Add(23, Properties.Resources.Sound23);
            MidiChannelEvent.InstrumentMap.Add(24, Properties.Resources.Sound24);
            MidiChannelEvent.InstrumentMap.Add(25, Properties.Resources.Sound25);
            MidiChannelEvent.InstrumentMap.Add(26, Properties.Resources.Sound26);
            MidiChannelEvent.InstrumentMap.Add(27, Properties.Resources.Sound27);
            MidiChannelEvent.InstrumentMap.Add(28, Properties.Resources.Sound28);
            MidiChannelEvent.InstrumentMap.Add(29, Properties.Resources.Sound29);
            MidiChannelEvent.InstrumentMap.Add(30, Properties.Resources.Sound30);
            MidiChannelEvent.InstrumentMap.Add(31, Properties.Resources.Sound31);
            MidiChannelEvent.InstrumentMap.Add(32, Properties.Resources.Sound32);
            MidiChannelEvent.InstrumentMap.Add(33, Properties.Resources.Sound33);
            MidiChannelEvent.InstrumentMap.Add(34, Properties.Resources.Sound34);
            MidiChannelEvent.InstrumentMap.Add(35, Properties.Resources.Sound35);
            MidiChannelEvent.InstrumentMap.Add(36, Properties.Resources.Sound36);
            MidiChannelEvent.InstrumentMap.Add(37, Properties.Resources.Sound37);
            MidiChannelEvent.InstrumentMap.Add(38, Properties.Resources.Sound38);
            MidiChannelEvent.InstrumentMap.Add(39, Properties.Resources.Sound39);
            MidiChannelEvent.InstrumentMap.Add(40, Properties.Resources.Sound40);
            MidiChannelEvent.InstrumentMap.Add(41, Properties.Resources.Sound41);
            MidiChannelEvent.InstrumentMap.Add(42, Properties.Resources.Sound42);
            MidiChannelEvent.InstrumentMap.Add(43, Properties.Resources.Sound43);
            MidiChannelEvent.InstrumentMap.Add(44, Properties.Resources.Sound44);
            MidiChannelEvent.InstrumentMap.Add(45, Properties.Resources.Sound45);
            MidiChannelEvent.InstrumentMap.Add(46, Properties.Resources.Sound46);
            MidiChannelEvent.InstrumentMap.Add(47, Properties.Resources.Sound47);
            MidiChannelEvent.InstrumentMap.Add(48, Properties.Resources.Sound48);
            MidiChannelEvent.InstrumentMap.Add(49, Properties.Resources.Sound49);
            MidiChannelEvent.InstrumentMap.Add(50, Properties.Resources.Sound50);
            MidiChannelEvent.InstrumentMap.Add(51, Properties.Resources.Sound51);
            MidiChannelEvent.InstrumentMap.Add(52, Properties.Resources.Sound52);
            MidiChannelEvent.InstrumentMap.Add(53, Properties.Resources.Sound53);
            MidiChannelEvent.InstrumentMap.Add(54, Properties.Resources.Sound54);
            MidiChannelEvent.InstrumentMap.Add(55, Properties.Resources.Sound55);
            MidiChannelEvent.InstrumentMap.Add(56, Properties.Resources.Sound56);
            MidiChannelEvent.InstrumentMap.Add(57, Properties.Resources.Sound57);
            MidiChannelEvent.InstrumentMap.Add(58, Properties.Resources.Sound58);
            MidiChannelEvent.InstrumentMap.Add(59, Properties.Resources.Sound59);
            MidiChannelEvent.InstrumentMap.Add(60, Properties.Resources.Sound60);
            MidiChannelEvent.InstrumentMap.Add(61, Properties.Resources.Sound61);
            MidiChannelEvent.InstrumentMap.Add(62, Properties.Resources.Sound62);
            MidiChannelEvent.InstrumentMap.Add(63, Properties.Resources.Sound63);
            MidiChannelEvent.InstrumentMap.Add(64, Properties.Resources.Sound64);
            MidiChannelEvent.InstrumentMap.Add(65, Properties.Resources.Sound65);
            MidiChannelEvent.InstrumentMap.Add(66, Properties.Resources.Sound66);
            MidiChannelEvent.InstrumentMap.Add(67, Properties.Resources.Sound67);
            MidiChannelEvent.InstrumentMap.Add(68, Properties.Resources.Sound68);
            MidiChannelEvent.InstrumentMap.Add(69, Properties.Resources.Sound69);
            MidiChannelEvent.InstrumentMap.Add(70, Properties.Resources.Sound70);
            MidiChannelEvent.InstrumentMap.Add(71, Properties.Resources.Sound71);
            MidiChannelEvent.InstrumentMap.Add(72, Properties.Resources.Sound72);
            MidiChannelEvent.InstrumentMap.Add(73, Properties.Resources.Sound73);
            MidiChannelEvent.InstrumentMap.Add(74, Properties.Resources.Sound74);
            MidiChannelEvent.InstrumentMap.Add(75, Properties.Resources.Sound75);
            MidiChannelEvent.InstrumentMap.Add(76, Properties.Resources.Sound76);
            MidiChannelEvent.InstrumentMap.Add(77, Properties.Resources.Sound77);
            MidiChannelEvent.InstrumentMap.Add(78, Properties.Resources.Sound78);
            MidiChannelEvent.InstrumentMap.Add(79, Properties.Resources.Sound79);
            MidiChannelEvent.InstrumentMap.Add(80, Properties.Resources.Sound80);
            MidiChannelEvent.InstrumentMap.Add(81, Properties.Resources.Sound81);
            MidiChannelEvent.InstrumentMap.Add(82, Properties.Resources.Sound82);
            MidiChannelEvent.InstrumentMap.Add(83, Properties.Resources.Sound83);
            MidiChannelEvent.InstrumentMap.Add(84, Properties.Resources.Sound84);
            MidiChannelEvent.InstrumentMap.Add(85, Properties.Resources.Sound85);
            MidiChannelEvent.InstrumentMap.Add(86, Properties.Resources.Sound86);
            MidiChannelEvent.InstrumentMap.Add(87, Properties.Resources.Sound87);
            MidiChannelEvent.InstrumentMap.Add(88, Properties.Resources.Sound88);
            MidiChannelEvent.InstrumentMap.Add(89, Properties.Resources.Sound89);
            MidiChannelEvent.InstrumentMap.Add(90, Properties.Resources.Sound90);
            MidiChannelEvent.InstrumentMap.Add(91, Properties.Resources.Sound91);
            MidiChannelEvent.InstrumentMap.Add(92, Properties.Resources.Sound92);
            MidiChannelEvent.InstrumentMap.Add(93, Properties.Resources.Sound93);
            MidiChannelEvent.InstrumentMap.Add(94, Properties.Resources.Sound94);
            MidiChannelEvent.InstrumentMap.Add(95, Properties.Resources.Sound95);
            MidiChannelEvent.InstrumentMap.Add(96, Properties.Resources.Sound96);
            MidiChannelEvent.InstrumentMap.Add(97, Properties.Resources.Sound97);
            MidiChannelEvent.InstrumentMap.Add(98, Properties.Resources.Sound98);
            MidiChannelEvent.InstrumentMap.Add(99, Properties.Resources.Sound99);
            MidiChannelEvent.InstrumentMap.Add(100, Properties.Resources.Sound100);
            MidiChannelEvent.InstrumentMap.Add(101, Properties.Resources.Sound101);
            MidiChannelEvent.InstrumentMap.Add(102, Properties.Resources.Sound102);
            MidiChannelEvent.InstrumentMap.Add(103, Properties.Resources.Sound103);
            MidiChannelEvent.InstrumentMap.Add(104, Properties.Resources.Sound104);
            MidiChannelEvent.InstrumentMap.Add(105, Properties.Resources.Sound105);
            MidiChannelEvent.InstrumentMap.Add(106, Properties.Resources.Sound106);
            MidiChannelEvent.InstrumentMap.Add(107, Properties.Resources.Sound107);
            MidiChannelEvent.InstrumentMap.Add(108, Properties.Resources.Sound108);
            MidiChannelEvent.InstrumentMap.Add(109, Properties.Resources.Sound109);
            MidiChannelEvent.InstrumentMap.Add(110, Properties.Resources.Sound110);
            MidiChannelEvent.InstrumentMap.Add(111, Properties.Resources.Sound111);
            MidiChannelEvent.InstrumentMap.Add(112, Properties.Resources.Sound112);
            MidiChannelEvent.InstrumentMap.Add(113, Properties.Resources.Sound113);
            MidiChannelEvent.InstrumentMap.Add(114, Properties.Resources.Sound114);
            MidiChannelEvent.InstrumentMap.Add(115, Properties.Resources.Sound115);
            MidiChannelEvent.InstrumentMap.Add(116, Properties.Resources.Sound116);
            MidiChannelEvent.InstrumentMap.Add(117, Properties.Resources.Sound117);
            MidiChannelEvent.InstrumentMap.Add(118, Properties.Resources.Sound118);
            MidiChannelEvent.InstrumentMap.Add(119, Properties.Resources.Sound119);
            MidiChannelEvent.InstrumentMap.Add(120, Properties.Resources.Sound120);
            MidiChannelEvent.InstrumentMap.Add(121, Properties.Resources.Sound121);
            MidiChannelEvent.InstrumentMap.Add(122, Properties.Resources.Sound122);
            MidiChannelEvent.InstrumentMap.Add(123, Properties.Resources.Sound123);
            MidiChannelEvent.InstrumentMap.Add(124, Properties.Resources.Sound124);
            MidiChannelEvent.InstrumentMap.Add(125, Properties.Resources.Sound125);
            MidiChannelEvent.InstrumentMap.Add(126, Properties.Resources.Sound126);
            MidiChannelEvent.InstrumentMap.Add(127, Properties.Resources.Sound127);
        }

        #endregion

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChannelEvent class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiChannelEvent(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static readonly Dictionary<MidiMessageType, TypeInfo> TypeMap;
        private static readonly Dictionary<int, string> PercussionMap;
        private static readonly Dictionary<int, string> ControllerMap;
        private static readonly Dictionary<int, string> InstrumentMap;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size
        { get { return MidiChannelEvent.SizeItem(this.DeltaTime, this.RunningStatus, this.MessageType); } }

        /// <summary>Gets the text to display in the "Type" field (i.e., event type).</summary>
        public override string TypeField
        {
            get
            {
                MidiMessageType messageType = this.MessageType;
                return this.RunningStatus ? null : (messageType == MidiMessageType.ControlChangeOrChannelMode)
                    ? ((this.Data1 >= 120) ? Properties.Resources.ChannelMode : Properties.Resources.ControlChange)
                    : MidiChannelEvent.GetTypeComment(messageType);
            }
        }

        /// <summary>Gets the text to display in the "Count" field (i.e., MIDI channel).</summary>
        public override string CountField
        { get { return this.RunningStatus ? null : UI.ParseLabel(Properties.Resources.Channel) + " " + this.Channel; } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public override string DataField
        {
            get
            {
                MidiMessageType messageType = this.MessageType;
                MidiKeySignature keySignature = this.File.GetKeySignature(this.TotalTime);
                string s1 = MidiChannelEvent.GetData1Comment(messageType, this.Data1, this.Channel, keySignature);
                string s2 = MidiChannelEvent.GetData2Comment(messageType, this.Data2);
                return string.IsNullOrEmpty(s2) ? s1 : s1 + ", " + s2;
            }
        }

        /// <summary>
        /// The status byte of the event.  The high nibble identifies the type of channel message, and the
        /// low nibble indicates one of the sixteen logical MIDI channels on which the event is transmitted.
        /// </summary>
        /// <remarks>
        /// If the Most Significant Bit (MSB) of this value is clear, the event uses running
        /// status.  The RunningStatus property should be called to determine whether or not
        /// this is the case.  If RunningStatus returns true, this value should not be used.
        /// </remarks>
        public int Status
        {
            get { return this.File.ReadNumber(this.StatusOffset, 1); }

            /* This should not be called for running status. */
            set
            {
                int n = this.Status;
                if (value == n) return;

                /* If message type is changing and it causes a change in event size, the file must be resized accordingly. */
                MidiMessageType oldMessageType = (MidiMessageType)Midi.GetHighNibble(n),
                    newMessageType = (MidiMessageType)Midi.GetHighNibble(value);
                if (oldMessageType != MidiMessageType.NA && oldMessageType != newMessageType)
                {
                    bool had = MidiChannelEvent.HasData2(oldMessageType), has = MidiChannelEvent.HasData2(newMessageType);
                    n = (had && !has) ? -1 : (!had && has) ? 1 : 0;
                    if (n != 0) this.File.Resize(n, this.Data2Offset + ((n < 0) ? 1 : 0), 0);
                }

                /* The file should be sized appropriately now; proceed. */
                this.File.WriteNumber(this.StatusOffset, value, 1);
                this.File.SetRunningStatus(this.Offset, value);
            }
        }

        /// <summary>Indicates whether or not this event uses running status.</summary>
        /// <remarks>
        /// If the Most Significant Bit (MSB) of the presumed status byte is set, it is a
        /// legitimate status byte.  Otherwise (MSB is clear), the event uses running status.
        /// </remarks>
        public bool RunningStatus { get { return this.Status < 0x80; } }

        /// <summary>Identifies the type of channel message.</summary>
        /// <remarks>This corresponds to the high nibble of the event's status byte.</remarks>
        public MidiMessageType MessageType
        {
            get
            {
                int b = this.RunningStatus ? this.File.GetRunningStatus(this.Offset) : this.Status;
                return (MidiMessageType)Midi.GetHighNibble(b);
            }
        }

        /// <summary>One of the sixteen logical MIDI channels on which this event is transmitted.</summary>
        /// <remarks>This corresponds to the low nibble of the event's status byte.</remarks>
        public int Channel
        { get { return Midi.GetLowNibble(this.RunningStatus ? this.File.GetRunningStatus(this.Offset) : this.Status); } }

        /// <summary>The first data byte of the event.</summary>
        public int Data1
        {
            get { return this.File.ReadNumber(this.Data1Offset, 1); }
            set { this.File.WriteNumber(this.Data1Offset, value, 1); }
        }

        /// <summary>The second data byte of the event.</summary>
        public int Data2
        {
            /* This should not be called for an event that has only one data byte. */
            get { return this.File.ReadNumber(this.Data2Offset, 1); }

            set
            {
                if (!MidiChannelEvent.HasData2(this.MessageType)) return;
                this.File.WriteNumber(this.Data2Offset, value, 1);
            }
        }

        #endregion

        #region Private Properties

        private int Data1Offset { get { return this.StatusOffset + (this.RunningStatus ? 0 : 1); } }
        private int Data2Offset { get { return this.Data1Offset + 1; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Returns the number of bytes required to store an item of this type.</summary>
        public static int SizeItem(int deltaTime, bool runningStatus, MidiMessageType messageType)
        { return Midi.SizeVLQ(deltaTime) + (runningStatus ? 0 : 1) + (MidiChannelEvent.HasData2(messageType) ? 2 : 1); }

        /// <summary>Returns a general comment on a type of channel message/event.</summary>
        /// <param name="messageType">Identifies the type of channel message (high nibble of status byte).</param>
        /// <returns>Comment on the type of channel message.</returns>
        public static string GetTypeComment(MidiMessageType messageType)
        { return MidiChannelEvent.TypeMap[messageType].Comment; }

        /// <summary>Returns a comment for the first data byte of a channel message/event.</summary>
        /// <param name="messageType">Identifies the type of channel message (high nibble of status byte).</param>
        /// <param name="data">The first data byte of the event.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which the event is transmitted.</param>
        /// <param name="keySignature">A MidiKeySignature value that applies to the event.</param>
        /// <returns>A comment for the data byte.</returns>
        public static string GetData1Comment(MidiMessageType messageType, int data, int channel, MidiKeySignature keySignature)
        {
            string s = (MidiChannelEvent.TypeMap[messageType].DescribeData1 == null) ? null :
                MidiChannelEvent.TypeMap[messageType].DescribeData1(data, channel, keySignature);
            return MidiChannelEvent.BuildDataComment(MidiChannelEvent.TypeMap[messageType].Data1Name, data, s);
        }

        /// <summary>Returns a comment for the second data byte of a channel message/event.</summary>
        /// <param name="messageType">Identifies the type of channel message (high nibble of status byte).</param>
        /// <param name="data">The second data byte of the event.</param>
        /// <returns>A comment for the data byte.</returns>
        public static string GetData2Comment(MidiMessageType messageType, int data)
        {
            if (!MidiChannelEvent.HasData2(messageType)) return null;
            string s = (MidiChannelEvent.TypeMap[messageType].DescribeData2 == null) ? null :
                MidiChannelEvent.TypeMap[messageType].DescribeData2(data, 0, MidiKeySignature.NA);
            return MidiChannelEvent.BuildDataComment(MidiChannelEvent.TypeMap[messageType].Data2Name, data, s);
        }

        /// <summary>Indicates whether a type of channel message has two data bytes or just one.</summary>
        /// <param name="messageType">Identifies the type of channel message (high nibble of status byte).</param>
        /// <returns>True if the type of channel message has two data bytes; otherwise (only one data byte), false.</returns>
        public static bool HasData2(MidiMessageType messageType)
        { return MidiChannelEvent.TypeMap[messageType].Data2Name != null; }

        #endregion

        #region Private Methods

        /// <summary>Converts a numeric value to a string representation of its assigned note(s).</summary>
        /// <param name="n">Numeric value assigned to note (middle C has a reference value of 60).</param>
        /// <returns>String representation of note(s) to which numeric value is assigned.</returns>
        private static string DescribeNote(int n, int channel, MidiKeySignature keySignature)
        {
            /* If appropriate, notate as percussion. */
            if (channel == 9 && MidiChannelEvent.PercussionMap.ContainsKey(n)) return MidiChannelEvent.PercussionMap[n];

            /* Otherwise, notate as pitch. */
            string s = null;
            int i = (n / 12) - 1;

            /* If there is no key signature, assume C major. */
            MidiKeySignature k = (keySignature == MidiKeySignature.NA) ? MidiKeySignature.CMajor : keySignature;

            /* Convert the numeric value based on key signature (see MidiKeySignature notation chart). */
            switch (n % 12)
            {
                case 0:
                    if (k > MidiKeySignature.EMajor && k < MidiKeySignature.CFlatMajor) { s = "B#"; --i; }
                    else s = "C"; break;
                case 1: s = (k > MidiKeySignature.FMajor || k < MidiKeySignature.CFlatMajor) ? "C#" : "Db"; break;
                case 2: s = "D"; break;
                case 3: s = (k > MidiKeySignature.GMajor && k < MidiKeySignature.CFlatMajor) ? "D#" : "Eb"; break;
                case 4: s = (k == MidiKeySignature.CFlatMajor || k == MidiKeySignature.AFlatMinor) ? "Fb" : "E"; break;
                case 5: s = (k > MidiKeySignature.AMajor && k < MidiKeySignature.CFlatMajor) ? "E#" : "F"; break;
                case 6: s = (k > MidiKeySignature.BFlatMinor || k < MidiKeySignature.CFlatMajor) ? "F#" : "Gb"; break;
                case 7: s = "G"; break;
                case 8: s = (k > MidiKeySignature.CMajor && k < MidiKeySignature.CFlatMajor) ? "G#" : "Ab"; break;
                case 9: s = "A"; break;
                case 10: s = (k > MidiKeySignature.DMajor && k < MidiKeySignature.CFlatMajor) ? "A#" : "Bb"; break;
                case 11:
                    if (k >= MidiKeySignature.CFlatMajor && k < MidiKeySignature.DFlatMajor) { s = "Cb"; ++i; }
                    else s = "B"; break;
                default: return string.Empty;
            }
            return s + " " + ((i < 0) ? "-" : (i > 9) ? "+" : i.ToString());
        }

        private static string DescribeController(int data, int channel, MidiKeySignature keySignature)
        { return MidiChannelEvent.ControllerMap[data]; }

        private static string DescribeInstrument(int data, int channel, MidiKeySignature keySignature)
        { return MidiChannelEvent.InstrumentMap[data]; }

        private static string BuildDataComment(string name, int data, string description)
        {
            string s = name + " " + data;
            return string.IsNullOrEmpty(description) ? s : (s + string.Format(" ({0})", description));
        }

        #endregion
    }
}
