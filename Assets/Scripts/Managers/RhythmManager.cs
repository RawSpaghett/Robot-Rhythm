using UnityEngine;

//Aligns music and gameplay
//Justin

//AudioSettings.dspTime;
//https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

public class RhythmManager: MonoBehaviour
{
    // gives the audio system a small delay before the song actually starts
    private const double startDelay = 0.2;
    public float songPosition {get; private set;}    //Current song position, in seconds
    public float songPositionInBeats {get; private set;}  //Current song position, in beats
    public bool isPlaying {get; private set;}
    private double songStartDspTime;

    public BeatMap beatMap;
    public MusicManager musicManager;
    public static RhythmManager Instance {get; private set;}//singleton

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
        if (beatMap == null || beatMap.SongID == null || beatMap.Bpm <= 0f || musicManager == null)
        {
            Debug.Log("beatmap, song or bpm are missing");
            return;
        }
        songStartDspTime = AudioSettings.dspTime + startDelay;
        musicManager.StartPlay(beatMap.SongID,songStartDspTime);

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

    private void SetObstacleTime()
    {
        foreach (ObstaclePlacement placement in beatMap.Obstacles)
        {
            placement.TargetTime = songStartDspTime + (beatMap.SecondsPerBeat * placement);
        }
    }

}
