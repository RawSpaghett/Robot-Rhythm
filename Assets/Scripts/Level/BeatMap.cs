using System.Collections.Generic;
using UnityEngine;

// each one made is one level and holds the song, tempo and the obstacles placed, this is what the editor will write
[CreateAssetMenu(fileName = "BeatMap", menuName = "Robot Rhythm/Beat Map")]
public class BeatMap : ScriptableObject
{
    // link to the audio file, does not hold the song it just points to it
    public string SongID;
    public float SongLength; //TEMPORARY FIX
    public float Bpm = 120f;
    public float FirstBeatOffset;
    public int BeatsPerMeasure = 4;

    // This is where it holds obstacleplacement so it knows what obstaacles are in the level and at which beat each one is
    public List<ObstaclePlacement> Obstacles = new List<ObstaclePlacement>();

    // Seconds between beats
    public float SecondsPerBeat
    {
        get { return 60f / Bpm; }
    }
}