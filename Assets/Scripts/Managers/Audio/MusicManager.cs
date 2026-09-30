using UnityEngine;

public class MusicManager: MonoBehaviour
{
    private AudioSource speakers;

    void Awake()
    {
        speakers = GetComponent<AudioSource>();
    }

    
}
