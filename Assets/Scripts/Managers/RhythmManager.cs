using UnityEngine;

//Aligns music and gameplay
//Justin

//AudioSettings.dspTime;
//https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

public class RhythmManager: MonoBehaviour
{
    // gives the audio system a small delay before the song actually starts
    private const double startDelay = 0.2;
    public static RhythmManager Instance {get; private set;}//singleton
    public float songPosition {get; private set;}    //Current song position, in seconds
    public float songPositionInBeats {get; private set;}  //Current song position, in beats
    public bool isPlaying {get; private set;}
    public BeatMap beatMap;
    public AudioSource musicSource;
    private double songStartDspTime;

    // Targets and player input use seconds from the same music clock.
    public double SongTime => AudioSettings.dspTime - songStartDspTime -
        (beatMap != null ? beatMap.FirstBeatOffset : 0f);

    void Awake()
    {
        IntializeInstance();
    }

    void Start()
    {
        StartSong();
    }

    void Update()
    {
        if (!isPlaying)
        {
            return;
        }
        songPosition = (float)SongTime;

        songPositionInBeats = songPosition /beatMap.SecondsPerBeat;//determine how many beats since the song started
    }

    public void StartSong()
    {
        if (beatMap == null || beatMap.Song == null || beatMap.Bpm <= 0f || musicSource == null)
        {
            Debug.Log("beatmap, song or bpm are missing");
            return;
        }

        musicSource.clip = beatMap.Song;

        songStartDspTime = AudioSettings.dspTime + startDelay;
        musicSource.PlayScheduled(songStartDspTime);

        isPlaying = true;
    }

    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // The level owns its song. Reloading the level starts a fresh clock.
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

}
