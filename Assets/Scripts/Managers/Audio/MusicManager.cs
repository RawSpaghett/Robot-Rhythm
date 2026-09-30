using UnityEngine;

//Justin
//Plays music for level

public class MusicManager: MonoBehaviour
{
    private AudioSource speakers;
    private RhythmManager rhythmManager;

    void Awake()
    {
        rhythmManager = RhythmManager.Instance;
        speakers = GetComponent<AudioSource>();
    }

    public void StartPlay(string musicID)
    {
        var song = MusicDatabase.Instance.GetClip(musicID);
        if (song != null)
        {
            speakers.clip = song;
            speakers.Play();
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


    
}
