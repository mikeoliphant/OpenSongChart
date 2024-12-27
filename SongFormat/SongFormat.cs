using System;
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

        public void AddOrReplacePart(SongInstrumentPart part)
        {
            InstrumentParts.RemoveAll(p => (p.InstrumentName == part.InstrumentName));

            InstrumentParts.Add(part);
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
            return InstrumentName + ((Tuning == null) ? "" : " (" + Tuning.GetTuning() + ((CapoFret > 0) ? (" C" + CapoFret) : "") + ")");
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
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
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
        Drums,
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
                string key = (StringSemitoneOffsets[1] < 0) ? GetOffsetNoteFlat(StringSemitoneOffsets[1]) : GetOffsetNoteSharp(StringSemitoneOffsets[1]);

                if (key == null)
                    return GetTuningAsNotes();

                if (StringSemitoneOffsets[0] == StringSemitoneOffsets[1])
                {
                    return key + " Std";
                }
                else // Drop tuning
                {
                    string drop = GetOffsetNoteFlat(StringSemitoneOffsets[0]);

                    if (drop == null)
                        return GetTuningAsNotes();

                    if (key == "E")
                        return "Drop " + drop;

                    return key + " Drop " + drop;
                }
            }

            return GetTuningAsNotes();
        }

        static int[] StringOffsetsFromE = { 0, 5, 10, 3, 7, 0 };

        public string GetTuningAsNotes()
        {
            string tuning = null;

            for (int i = 0; i < StringSemitoneOffsets.Count; i++)
            {
                tuning += GetOffsetNoteSharp(StringSemitoneOffsets[i] + StringOffsetsFromE[i]);
            }

            switch (tuning)
            {
                case "DGDGBD":
                    return "Open G";

                case "DADF#AD":
                    return "Open D";

                case "EBEG#BE":
                    return "Open E";

                case "EAEAC#E":
                    return "Open A";

                case "CGCGCE":
                    return "Open C";
            }

            return tuning;
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
        /// Get note name offset from E using sharps
        /// </summary>
        /// <param name="offset">The offset in semitones</param>
        /// <returns>The offset note name</returns>
        public static string GetOffsetNoteSharp(int offset)
        {
            if (offset < 0)
                offset += 12;

            switch (offset % 12)
            {
                case 0:
                    return "E";
                case 1:
                    return "F";
                case 2:
                    return "F#";
                case 3:
                    return "G";
                case 4:
                    return "G#";
                case 5:
                    return "A";
                case 6:
                    return "A#";
                case 7:
                    return "B";
                case 8: return "C";
                case 9:
                    return "C#";
                case 10:
                    return "D";
                case 11:
                    return "D#";
            }

            return null;
        }

        /// <summary>
        /// Get note name offset from E using flats
        /// </summary>
        /// <param name="offset">The offset in semitones</param>
        /// <returns>The offset note name</returns>
        public static string GetOffsetNoteFlat(int offset)
        {
            if (offset < 0)
                offset += 12;

            switch (offset % 12)
            {
                case 0:
                    return "E";
                case 1:
                    return "F";
                case 2:
                    return "Gb";
                case 3:
                    return "G";
                case 4:
                    return "Ab";
                case 5:
                    return "A";
                case 6:
                    return "Bb";
                case 7:
                    return "B";
                case 8: return "C";
                case 9:
                    return "Db";
                case 10:
                    return "D";
                case 11:
                    return "Eb";
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
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
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
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
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
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float TimeOffset { get; set; } = 0;
        public float TimeLength { get; set; } = 0;
        public int Note { get; set; } = 0;
        public int Velocity { get; set; } = 0;

        public SongKeyboardNote()
        {

        }
    }

    public enum EDrumKitPieceType
    {
        None,
        Kick,
        Snare,
        HiHat,
        Crash,
        Ride,
        Tom,
        Flexi
    }

    public enum EDrumKitPiece
    {
        None,
        Kick,
        Snare,
        HiHat,
        Crash,
        Crash2,
        Crash3,
        Ride,
        Ride2,
        Tom1,
        Tom2,
        Tom3,
        Tom4,
        Tom5,
        Flexi1,
        Flexi2,
        Flexi3,
        Flexi4
    }

    public enum EDrumArticulation
    {
        None,
        DrumHead,
        DrumHeadEdge,
        DrumRim,
        SideStick,
        HiHatClosed,
        HiHatOpen,
        HiHatChick,
        HiHatSplash,
        CymbalEdge,
        CymbalBow,
        CymbalBell,
        CymbalChoke,
        FlexiA,
        FlexiB,
        FlexiC
    }

    public struct SongDrumNote
    {
        public static EDrumKitPieceType GetKitPieceType(EDrumKitPiece kitPiece)
        {
            switch (kitPiece)
            {
                case EDrumKitPiece.None:
                    return EDrumKitPieceType.None;
                case EDrumKitPiece.Kick:
                    return EDrumKitPieceType.Kick;
                case EDrumKitPiece.Snare:
                    return EDrumKitPieceType.Snare;
                case EDrumKitPiece.HiHat:
                    return EDrumKitPieceType.HiHat;
                case EDrumKitPiece.Crash:
                case EDrumKitPiece.Crash2:
                case EDrumKitPiece.Crash3:
                    return EDrumKitPieceType.Crash;
                case EDrumKitPiece.Ride:
                case EDrumKitPiece.Ride2:
                    return EDrumKitPieceType.Ride;
                case EDrumKitPiece.Tom1:
                case EDrumKitPiece.Tom2:
                case EDrumKitPiece.Tom3:
                case EDrumKitPiece.Tom4:
                case EDrumKitPiece.Tom5:
                    return EDrumKitPieceType.Tom;
                case EDrumKitPiece.Flexi1:
                case EDrumKitPiece.Flexi2:
                case EDrumKitPiece.Flexi3:
                case EDrumKitPiece.Flexi4:
                    return EDrumKitPieceType.Flexi;
            }

            return EDrumKitPieceType.None;
        }

        public static EDrumArticulation GetDefaultArticulation(EDrumKitPiece kitPiece)
        {
            return GetDefaultArticulation(GetKitPieceType(kitPiece));
        }

        public static EDrumArticulation GetDefaultArticulation(EDrumKitPieceType kitPieceType)
        {
            EDrumArticulation articulation = EDrumArticulation.None;

            switch (kitPieceType)
            {
                case EDrumKitPieceType.Kick:
                    articulation = EDrumArticulation.DrumHead;
                    break;
                case EDrumKitPieceType.Snare:
                    articulation = EDrumArticulation.DrumHead;
                    break;
                case EDrumKitPieceType.HiHat:
                    articulation = EDrumArticulation.HiHatClosed;
                    break;
                case EDrumKitPieceType.Crash:
                    articulation = EDrumArticulation.CymbalEdge;
                    break;
                case EDrumKitPieceType.Ride:
                    articulation = EDrumArticulation.CymbalBow;
                    break;
                case EDrumKitPieceType.Flexi:
                    articulation = EDrumArticulation.FlexiA;
                    break;
                case EDrumKitPieceType.Tom:
                    articulation = EDrumArticulation.DrumHead;
                    break;
                default:
                    break;
            }

            return articulation;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float TimeOffset { get; set; } = 0;
        public EDrumKitPiece KitPiece { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public EDrumArticulation Articulation { get; set; }

        public SongDrumNote()
        {

        }
    }

    public class SongDrumNotes
    {
        public List<SongSection> Sections { get; set; } = new List<SongSection>();
        public List<SongDrumNote> Notes { get; set; } = new List<SongDrumNote>();
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
