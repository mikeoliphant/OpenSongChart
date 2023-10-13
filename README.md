# What is it?
This is a work-in-progress open format for song charts - the data required to capture instrument performances that make up a song.

# What is it not?
 - It is not a score format. It is designed to represent a song as actually performed, with events synchronized to an audio recording.
 - It is not an "editor" format. It is designed for playback.

# What is in this repo?
This repository currently has C# data structures designed to be deserialized from data files (currently using json)

# Song format details
A song is comprised of a folder of files.

Files are:
- song.json (song metadata)
- song.ogg (song audio recording)
- arrangement.json (song structure information - currently just measures/beats)
- <part>.json (one file per instrument part)

## song.json

```
{
  "SongName": "Cool Song",
  "ArtistName": "Cool Artist",
  "AlbumName": "Awesomee album",
  "A440CentsOffset": 30,
  "InstrumentParts": [
    {
      "InstrumentName": "bass",
      "InstrumentType": "BassGuitar",
      "Tuning": {
        "StringSemitoneOffsets": [
          -1,
          -1,
          -1,
          -1,
          -1
        ]
      }
    },
    {
      "InstrumentName": "lead",
      "InstrumentType": "LeadGuitar",
      "Tuning": {
        "StringSemitoneOffsets": [
          -1,
          -1,
          -1,
          -1,
          -1
        ]
      }
    },
    {
      "InstrumentName": "rhythm",
      "InstrumentType": "RhythmGuitar",
      "Tuning": {
        "StringSemitoneOffsets": [
          -1,
          -1,
          -1,
          -1,
          -1
        ]
      }
    },
    {
      "InstrumentName": "vocals",
      "InstrumentType": "Vocals"
    }
  ]
}
```

Notes:

"**A440CentsOffset**" is the numbers of cents (100th of a semitone) the song is out of tune. This is in addition to the individual instrument tunings described by the "**Tuning**" for each part. "**StringSemitoneOffsets**" specifies how many semitones each string is detuned from E standard tuning. So, for this song example, all of the guitar parts are tuned a half-step down to Eb. But the song is also 30 cents sharp.

"**InstrumentType** is an enumeration. It is separate from the name of the part, because there could be more than one part for an instrument type. Current types are:

```
LeadGuitar
RhythmGuitar
BassGuitar
Vocals
```

## arrangement.json
The arrangment file currently just has a "**Beats**" section, which is a list of time-indexed measures/beats.

## \<part\>.json
The individual stringed instrument parts have a "**Sections**"" list, a "**Chords**" list and a "**Notes**"" list.

A section is a division of an instrument part into logical sections. It looks like this:

```
{
  "Name": "intro",
  "StartTime": 4.18,
  "EndTime":19.737
}
```

Chords indicate the fingers and frets per string (with "-1" indicating the string is unused)

```
{
  "Name": "A",
  "Fingers": [-1,-1,1,2,3,-1],
  "Frets":[-1,0,2,2,2,-1]
}
```
In this case, it is an "A" chord with fingers 1, 2, and 3 all on fret 2 of the D, G, and B strings and the A string is open.

A note is an individual note event. It is the most complex structure.

Here is an example:

```
{
  "TimeOffset": 43.796,
  "TimeLength": 0.204,
  "Fret": 5,
  "String": 1,
  "Techniques": "Slide",
  "HandFret": 5,
  "SlideFret": 7
}
```
"TimeOffset" is the start time of the note in seconds. "TimeLength" is the duration. "Fret" and "String" are the fingering. "HandFret" is the hand anchor position. "Techniques" is a comma-separated list of note modifiers. In this case, we have a slide from from 5 to fret 7 on the second string.

Currently, these are the possible techniques:

```
HammerOn 
PullOff 
Accent 
PalmMute 
FretHandMute 
Slide 
Bend 
Tremolo 
Vibrato 
Harmonic 
PinchHarmonic 
Tap 
Slap 
Pop 
Chord 
ChordNote 
Continued 
```

These should be pretty self-explanatory, with a few exceptions:
- **Chord** indicates the note is a chord, **ChordID** will be an index into the list of Chords
- **ChordNote** if present with **Chord**, indicates that individual notes for the chord follow with their own separate information
- **ChordNote** if *not* present with **Chord**, indicates the chord is a simple chord
- **Continued** indicates the note is a continuation of a previous note (so a new note head should not be drawn)

## vocals.json
Vocal parts have a different structure. They are simply a list of timed vocal events:

```
{
  "Vocal": "Lala\n"
  "TimeOffset":16.159
}
```

Newline characters ("\n") indicate a line break. Dashes ("-") indicate word breaks. Any vocal that doesn't end with "-" should be considered a complete word.
