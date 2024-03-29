﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SongFormat
{
    /// <summary>
    /// Top level song metadata
    /// </summary>
    public class SongData
    {
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public float A440CentsOffset { get; set; }
        public List<SongInstrumentPart> InstrumentParts { get; set; } = new List<SongInstrumentPart>();

        public SongInstrumentPart GetPart(string instrumentName)
        {
            return InstrumentParts.FirstOrDefault(p => p.InstrumentName == instrumentName);
        }

        public override string ToString()
        {
            return ArtistName + " - " + SongName;
        }
    }

    /// <summary>
    /// Instrument part metadata
    /// </summary>
    public class SongInstrumentPart
    {
        public string InstrumentName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ESongInstrumentType InstrumentType { get; set; }
        public StringTuning Tuning { get; set; }
        public int CapoFret { get; set; } = 0;

        public override string ToString()
        {
            return InstrumentName + " (" + Tuning.GetTuning() + ((CapoFret > 0) ? (" C" + CapoFret) : "") + ")";
        }
    }

    /// <summary>
    /// Song structure/arrangement information
    /// </summary>
    public class SongStructure
    {        
        public List<SongSection> Sections { get; set; } = new List<SongSection>();
        public List<SongBeat> Beats { get; set; } = new List<SongBeat>();
    }

    /// <summary>
    /// Song section (ie: "verse", "chorus")
    /// </summary>
    public class SongSection
    {
        public string Name { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }

        public override string ToString()
        {
            return Name + "[" + StartTime + "-" + EndTime + "]";
        }
    }

    /// <summary>
    /// An individual beat in a song
    /// </summary>
    public struct SongBeat
    {
        public float TimeOffset { get; set; }
        public bool IsMeasure { get; set; }
    }

    /// <summary>
    /// The type of instrument
    /// </summary>
    public enum ESongInstrumentType
    {
        LeadGuitar,
        RhythmGuitar,
        BassGuitar,
        Keys,
        Vocals
    }

    /// <summary>
    /// Tuning information for a stringed instrument
    /// </summary>
    public class StringTuning
    {
        public List<int> StringSemitoneOffsets { get; set; } = null;

        public string GetTuning()
        {
            // If we don't have any offsets, assume E Standard
            if ((StringSemitoneOffsets == null) || (StringSemitoneOffsets.Count < 4))
                return "E Std";

            if (IsOffsetFromStandard())
            {
                string key = GetOffsetNote(StringSemitoneOffsets[1]);

                if (key == null)
                    return "Custom";

                if (StringSemitoneOffsets[0] == StringSemitoneOffsets[1])
                {
                    return key + " Std";
                }
                else // Drop tuning
                {
                    string drop = GetDropNote(StringSemitoneOffsets[0]);

                    if (drop == null)
                        return "Custom";

                    if (key == "E")
                        return "Drop " + drop;

                    return key + " Drop " + drop;
                }
            }

            return "Custom";
        }

        /// <summary>
        /// Check if a tuning is offset from standard tuning (including first-string drop tunings)
        /// </summary>
        /// <returns>Whether the tuning is offset from standard</returns>
        public bool IsOffsetFromStandard()
        {
            for (int i = 2; i < StringSemitoneOffsets.Count; i++)
                if (StringSemitoneOffsets[1] != StringSemitoneOffsets[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Get note name offset from E standard
        /// </summary>
        /// <param name="offset">The offset in semitones</param>
        /// <returns>The offset note name</returns>
        public static string GetOffsetNote(int offset)
        {
            switch (offset)
            {
                case 0:
                    return "E";
                case 1:
                    return "F";
                case 2:
                    return "F#";
                case -1:
                    return "Eb";
                case -2:
                    return "D";
                case -3:
                    return "C#";
                case -4:
                    return "C";
                case -5:
                    return "B";
            }

            return null;
        }

        /// <summary>
        /// Get a dropped note name (prefer flats) offset from E standard
        /// </summary>
        /// <param name="offset">The offset in semitones</param>
        /// <returns>The offset note name</returns>
        public static string GetDropNote(int offset)
        {
            switch (offset)
            {
                case -1:
                    return "Eb";
                case -2:
                    return "D";
                case -3:
                    return "Db";
                case -4:
                    return "C";
                case -5:
                    return "B";
            }

            return null;
        }

        public override string ToString()
        {
            return GetTuning();
        }
    }

    /// <summary>
    /// Notes and chords for an instrument part
    /// </summary>
    public class SongInstrumentNotes
    {
        public List<SongSection> Sections { get; set; } = new List<SongSection>();
        public List<SongChord> Chords { get; set; } = new List<SongChord>();
        public List<SongNote> Notes { get; set; } = new List<SongNote>();
    }

    /// <summary>
    /// Chord notes/fingering
    /// </summary>
    public class SongChord
    {
        public string Name { get; set; }
        public List<int> Fingers { get; set; } = new List<int>();
        public List<int> Frets { get; set;} = new List<int>();
    }

    /// <summary>
    /// An individual note/chord event in a song
    /// </summary>
    public struct SongNote
    {
        /// <summary>
        /// Start offset of the note in seconds
        /// </summary>
        public float TimeOffset { get; set; } = 0;
        /// <summary>
        /// Sustain length of the note in seconds
        /// </summary>
        public float TimeLength { get; set; } = 0;
        /// <summary>
        /// Fret number of the note - "0" is open string, "-1" is unfretted
        /// </summary>
        public int Fret { get; set; } = -1;
        /// <summary>
        /// String of the note (zero-based)
        /// </summary>
        public int String { get; set; } = -1;
        /// <summary>
        /// Array of bend offsets
        /// </summary>
        public CentsOffset[] CentsOffsets { get; set; } = null;
        /// <summary>
        /// Song technique flags
        /// </summary>
        public ESongNoteTechnique Techniques { get; set; } = 0;
        /// <summary>
        /// Bottom fret of hand position
        /// </summary>
        public int HandFret { get; set; } = -1;
        /// <summary>
        /// Fret that note slides to over the course of its sustain
        /// </summary>
        public int SlideFret { get; set; } = -1;
        /// <summary>
        /// Index into chord array to use for notes
        /// </summary>
        public int ChordID { get; set; } = -1;
        /// <summary>
        /// Index into chord array to use for fingering
        /// </summary>
        public int FingerID { get; set; } = -1;

        public SongNote()
        {

        }
    }

    /// <summary>
    /// Offset structure for bends
    /// </summary>
    public struct CentsOffset
    {
        /// <summary>
        /// Time offset of the bend position
        /// </summary>
        public float TimeOffset { get; set; }
        /// <summary>
        /// Amount of the bend, in cents (100th of a semitone)
        /// </summary>
        public int Cents { get; set; }
    }

    /// <summary>
    /// Technique flags
    /// </summary>
    [Flags]
    public enum ESongNoteTechnique
    {
        HammerOn = 1 << 1,
        PullOff = 1 << 2,
        Accent = 1 << 3,
        PalmMute = 1 << 4,
        FretHandMute = 1 << 5,
        Slide = 1 << 6,
        Bend = 1 << 7,
        Tremolo = 1 << 8,
        Vibrato = 1 << 9,
        Harmonic = 1 << 10,
        PinchHarmonic = 1 << 11,
        Tap = 1 << 12,
        Slap = 1 << 13,
        Pop = 1 << 14,
        Chord = 1 << 15,
        ChordNote = 1 << 16,
        Continued = 1 << 17,
        Arpeggio = 1 << 18
    }

    /// <summary>
    /// Notes for a keys part
    /// </summary>
    public class SongKeyboardNotes
    {
        public List<SongSection> Sections { get; set; } = new List<SongSection>();
        public List<SongKeyboardNote> Notes { get; set; } = new List<SongKeyboardNote>();
    }

    public struct SongKeyboardNote
    {
        public float TimeOffset { get; set; } = 0;
        public float TimeLength { get; set; } = 0;
        public int Note { get; set; } = 0;
        public int Velocity { get; set; } = 0;

        public SongKeyboardNote()
        {

        }
    }

    /// <summary>
    /// Vocal/lyric events in a song
    /// </summary>
    public struct SongVocal
    {
        public string Vocal { get; set; }
        public float TimeOffset { get; set; }
    }
}
