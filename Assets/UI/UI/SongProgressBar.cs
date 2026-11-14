using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SongProgressBar : MonoBehaviour
{
    private Material instance;
    public float Progress { get; private set; }

    public void SetProgress(float value)
    {
        if(instance==null)
        {
            var image=GetComponent<Image>();
            instance=new Material(image.material);
            image.material=instance;
        }
        Progress=Mathf.Clamp01(value);
        instance.SetFloat("_Progress",Progress);
    }

    private void OnDestroy() { if(instance!=null) Destroy(instance); }
}
