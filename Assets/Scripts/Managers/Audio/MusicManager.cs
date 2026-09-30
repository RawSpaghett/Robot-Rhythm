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

    public void StartPlay()
    {}
    public void EndPlay()
    {}
    public void ResumePlay() 
    {}


    
}
