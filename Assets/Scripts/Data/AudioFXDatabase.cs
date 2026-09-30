using UnityEngine;
using System.Collections.Generic;

//Stores music data
//Justin

public class AudioFXDatabase: MonoBehaviour
{
    public static AudioFXDatabase Instance {get; private set;}//singleton
    public AudioClip[] audioLoader;
    public Dictionary<string, AudioClip> fxLibrary = new Dictionary<string, AudioClip>();

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
        foreach (AudioClip clip in audioLoader)
        {
            if (clip != null && !fxLibrary.ContainsKey(clip.name))
            {
                fxLibrary.Add(clip.name, clip);
            }
        }
    }

    public AudioClip GetClip(string fxID)
    {
        if(fxLibrary.TryGetValue(fxID, out AudioClip clip))
        {
            return clip;
        }
        else
        {
            Debug.Log($"{fxID} not found.");
            return null;
        }
    }


}
