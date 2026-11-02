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

    private double visualSongTime;
    private int visualFrame = -1;
    private bool visualClockReady;

    // The audio clock moves in buffer-sized steps. Visuals advance each frame
    // and gently correct toward it; input judgement still uses SongTime.
    public double VisualSongTime
    {
        get
        {
            if (visualFrame == Time.frameCount) return visualSongTime;
            visualFrame = Time.frameCount;
            double audioTime = SongTime;
            if (!visualClockReady)
            {
                visualSongTime = audioTime;
                visualClockReady = true;
            }
            else if (isPlaying && Time.timeScale > 0f && !AudioListener.pause)
            {
                visualSongTime += Time.deltaTime;
                double error = audioTime - visualSongTime;
                if (System.Math.Abs(error) > 0.25) visualSongTime = audioTime;
                else visualSongTime += Mathf.Clamp((float)error * Time.deltaTime * 3f,
                    -Time.deltaTime * 0.1f, Time.deltaTime * 0.1f);
            }
            return visualSongTime;
        }
    }

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
        visualClockReady = false;
        visualFrame = -1;
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
