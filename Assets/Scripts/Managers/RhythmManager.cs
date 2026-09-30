using UnityEngine;
using System;
using System.Collections;

//Aligns music and gameplay
//Justin

//AudioSettings.dspTime;
//https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

public class RhythmManager: MonoBehaviour
{
    public float songBpm {get; private set;}//Song beats per minute
    public float secPerBeat {get; private set;}//The number of seconds for each song beat
    public float songPosition {get; private set;}    //Current song position, in seconds
    public float songPositionInBeats {get; private set;}  //Current song position, in beats
    public float dspSongTime {get; private set;}//How many seconds have passed since the song started
    private MusicManager musicManager;

    void Start()
    {
        musicManager = GetComponent<MusicManager>();

        secPerBeat = 60f / songBpm; //Calculate the number of seconds in each beat

        dspSongTime = (float)AudioSettings.dspTime;//Record the time when the music starts
    }

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);//determine how many seconds since the song started

        songPositionInBeats = songPosition / secPerBeat;//determine how many beats since the song started
    }

}
