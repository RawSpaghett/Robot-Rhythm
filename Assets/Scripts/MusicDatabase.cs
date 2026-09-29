using UnityEngine;
using System.Collections.Generic;

//Stores music data
//Justin

public class MusicDatabase: MonoBehaviour
{
    public static MusicDatabase Instance {get; private set;}//singleton
    public AudioClip[] audioLibrary;
    public Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        IntializeInstance();
        LoadDatabase();
    }

    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void LoadDatabase()
    {
        foreach (AudioClip clip in audioLibrary)
        {
            if (clip != null && !musicLibrary.ContainsKey(clip.name))
            {
                musicLibrary.Add(clip.name, clip);
            }
        }
    }


}
