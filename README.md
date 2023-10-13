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
      "InstrumentType": 2,
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
      "InstrumentType": 1,
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
      "InstrumentType": 3
    }
  ]
}
```

Notes: "**A440CentsOffset**" is the numbers of cents (100th of a semitone) the song is out of tune. This is in addition to the individual instrument tunings described by the "**Tuning**" for each part. "**StringSemitoneOffsets**" specifies how many semitones each string is detuned from E standard tuning. So, for this song example, all of the guitar parts are tuned a half-step down to Eb. But the song is also 30 cents sharp.

## arrangement.json
The arrangment file currently just has a "**Beats**" section, which is a list of time-indexed measures/beats.

## <part>.json
The individual instrument parts have a "**Sections**"" list and a "**Notes**"" list.

A section is a division of an instrument part into logical sections. It looks like this:

```
{
  "Name": "intro",
  "StartTime": 4.18,
  "EndTime":19.737
}
```

A note is an individual note event. It is the most complex structure.
