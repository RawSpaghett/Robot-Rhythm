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
        if (musicManager == null) musicManager = MusicManager.Instance;
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
        #region  Debug
        if (beatMap == null || beatMap.SongID == null || beatMap.Bpm <= 0f || musicManager == null)if (beatMap == null)
        {
            Debug.LogError("beatMap is missing (null).");
            return;
        }

        if (beatMap.SongID == null)
        {
            Debug.LogError("beatMap.SongID is missing (null).");
            return;
        }

        if (beatMap.Bpm <= 0f)
        {
            Debug.LogError($"beatMap.Bpm is invalid ({beatMap.Bpm}). Must be greater than 0.");
            return;
        }

        if (musicManager == null)
        {
            Debug.LogError("musicManager is missing (null).");
            return;
        }
        #endregion
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
        
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

}
