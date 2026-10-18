using UnityEngine;

//Justin
//Plays music for level

public class MusicManager: MonoBehaviour
{
    [SerializeField]private AudioSource speakers;
    [SerializeField]private RhythmManager rhythmManager;
    private double fadeStart;
    private const float FadeSeconds = .8f;
    public static MusicManager Instance {get; private set;}
    void Update()
    {
        if (speakers)
        {
            float fade = Mathf.Clamp01((float)(AudioSettings.dspTime - fadeStart) / FadeSeconds);
            speakers.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("rr.music", 1f)) * Mathf.SmoothStep(0, 1, fade);
        }
    }

    void Awake()
    {
        IntializeInstance();
        speakers = GetComponent<AudioSource>();
        speakers.playOnAwake = false;
        speakers.Stop();
    }

    public void StartPlay(string musicID, double dsp)
    {
        var song = MusicDatabase.Instance.GetClip(musicID);
        if (song != null)
        {
            speakers.clip = song;
            speakers.volume=0;
            fadeStart=dsp;
            speakers.PlayScheduled(dsp);
        }
    }

    public void EndPlay()
    {
        speakers.Stop();
    }

    public void ResumePlay() 
    {
        speakers.UnPause();
    }

    public void PausePlay()
    {
        speakers.Pause();
    }
    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // The level owns this player, so unloading it also stops its song.
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
