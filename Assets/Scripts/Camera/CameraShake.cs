using UnityEngine;
using System.Collections;

public class CameraShake: MonoBehaviour
{

    public static CameraShake Instance {get; private set;}//singleton
    private Camera MainCamera;
    private void Awake()
    {
        MainCamera = Camera.main;
        IntializeInstance();
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Debug.Log("SHAKEEE");
        Vector3 OriginalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f,1f) * magnitude;
            float y = Random.Range(-1f,1f) * magnitude;

            transform.localPosition = new Vector3(OriginalPosition.x + x, OriginalPosition.y + y, OriginalPosition.z);
            elapsed+= Time.deltaTime;
            yield return null;
        }

        transform.localPosition = OriginalPosition;

    }

    public void ShakeCameraStart(float duration, float magnitude)
    {
        Debug.Log("SHAKEEE");
        StartCoroutine(ShakeCamera(duration,magnitude));
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

}
