using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using RobotRhythm.UI;

// Plays once at startup. The small reveal maps store when each ink pixel appears.
public class StudioSplash : MonoBehaviour
{
    public static bool Playing { get; private set; }
    public static void Prepare() { Playing = true; }
    private const float FadeIn = .95f;
    private const float LaughAt = 7.035167f;
    private const float PresentsAt = 8.935167f;
    private const float MenuAt = 11.985167f;
    private readonly Object[] loaded = new Object[7];
    private Material logoMaterial, scribbleMaterial;
    private GameObject content;
    private AudioSource sound;

    public IEnumerator Play(UiController preferences, StartupSunburst black)
    {
        Playing = true;
        string[] names = { "Logo", "LogoReveal", "Crossout", "CrossoutReveal", "Presents", "Startup", "InkReveal" };
        for (int i = 0; i < names.Length; i++)
        {
            var request = Resources.LoadAsync("StudioSplash/" + names[i]);
            yield return request;
            loaded[i] = request.asset;
            if (!loaded[i]) { Debug.LogError("Missing studio splash asset: " + names[i]); Release(); yield break; }
        }

        var parent = (RectTransform)transform;
        content = new GameObject("Studio splash", typeof(RectTransform), typeof(CanvasRenderer), typeof(StartupSunburst), typeof(Mask));
        var maskRect = (RectTransform)content.transform;
        maskRect.SetParent(parent, false); Stretch(maskRect);
        maskRect.SetAsFirstSibling();
        var opening = content.GetComponent<StartupSunburst>();
        content.GetComponent<Mask>().showMaskGraphic = false;
        var backdrop = Create<Image>("Navy", maskRect);
        backdrop.color = new Color32(35, 64, 75, 255);
        var existingPattern = FindFirstObjectByType<ScrollingRobotPattern>(FindObjectsInactive.Include);
        if (existingPattern)
        {
            var pattern = Create<ScrollingRobotPattern>("Robots", maskRect);
            pattern.robot = existingPattern.robot; pattern.color = existingPattern.color; pattern.preferences = preferences;
            // Use the same design-space scale as the menu, including on small windows.
            pattern.rectTransform.anchorMin = pattern.rectTransform.anchorMax = new Vector2(.5f,.5f);
            pattern.rectTransform.sizeDelta = new Vector2(1920,1080);
        }
        Create<MenuVignette>("Vignette", maskRect);
        var stage = new GameObject("Artwork", typeof(RectTransform)).GetComponent<RectTransform>();
        stage.SetParent(maskRect, false);stage.sizeDelta = new Vector2(1448,815);
        logoMaterial = new Material((Shader)loaded[6]);logoMaterial.SetTexture("_RevealTex", (Texture)loaded[1]);
        scribbleMaterial = new Material((Shader)loaded[6]);scribbleMaterial.SetTexture("_RevealTex", (Texture)loaded[3]);
        var logo = Artwork("Misfits Studios", stage, (Texture)loaded[0], new Vector2(970,610));logo.material = logoMaterial;
        var scribble = Artwork("Orange scribbles", stage, (Texture)loaded[2], new Vector2(970,610));scribble.material = scribbleMaterial;
        var presents = Artwork("Presents", stage, (Texture)loaded[4], new Vector2(1100,691.3f));presents.gameObject.SetActive(false);
        var blackGroup = black.GetComponent<CanvasGroup>();
        if (!blackGroup) blackGroup = black.gameObject.AddComponent<CanvasGroup>();
        blackGroup.alpha = 1;
        sound = gameObject.AddComponent<AudioSource>();sound.playOnAwake = false;sound.ignoreListenerPause = true;sound.ignoreListenerVolume = true;
        sound.clip = (AudioClip)loaded[5];sound.volume = preferences.UiVolume * preferences.EffectsVolume;
        double start = AudioSettings.dspTime + .08;
        sound.PlayScheduled(start);
        bool reduced = preferences.ReducedMotion;
        while (true)
        {
            float t = Mathf.Max(0,(float)(AudioSettings.dspTime-start));
            if (t >= MenuAt + 1.25f) break;
            float scale = Mathf.Min(parent.rect.width/1448f,parent.rect.height/815f);
            stage.localScale = Vector3.one*scale;
            var pattern = content.GetComponentInChildren<ScrollingRobotPattern>();
            if (pattern) pattern.rectTransform.localScale = Vector3.one * Mathf.Max(parent.rect.width/1920f,parent.rect.height/1080f);
            blackGroup.alpha = 1-Mathf.SmoothStep(0,1,t/.8f);
            float writing = t-FadeIn;
            logo.materialForRendering.SetFloat("_Elapsed", reduced ? (writing >= 0 ? 16 : -1) : writing);
            scribble.materialForRendering.SetFloat("_Elapsed", reduced ? (writing >= LaughAt ? 16 : -1) : writing-LaughAt);
            bool showPresents = writing >= PresentsAt;
            logo.gameObject.SetActive(!showPresents);scribble.gameObject.SetActive(!showPresents);presents.gameObject.SetActive(showPresents);
            if (showPresents && !reduced)
            {
                float u = writing-PresentsAt;
                presents.rectTransform.localScale = Vector3.one*(1+.13f*Mathf.Exp(-u*19)*Mathf.Cos(u*27));
                presents.rectTransform.localRotation = Quaternion.Euler(0,0,.018f*Mathf.Rad2Deg*Mathf.Exp(-u*18));
            }
            float reveal = Mathf.Clamp01((t-MenuAt)/(reduced ? .2f : 1.25f));
            opening.Progress = Mathf.SmoothStep(0,1,reveal);opening.SetVerticesDirty();
            sound.volume = preferences.UiVolume*preferences.EffectsVolume;
            yield return null;
        }
        blackGroup.alpha = 1;
        Release();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;rect.anchorMax = Vector2.one;rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    private static T Create<T>(string label, Transform parent) where T : Graphic
    {
        var item = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<T>();
        item.transform.SetParent(parent,false);Stretch(item.rectTransform);item.raycastTarget = false;return item;
    }
    private static RawImage Artwork(string label, Transform parent, Texture texture, Vector2 size)
    {
        var image = Create<RawImage>(label,parent);image.texture = texture;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(.5f,.5f);
        image.rectTransform.sizeDelta = size;return image;
    }
    private void Release()
    {
        Playing = false;
        if (sound) { sound.Stop();sound.clip = null;Destroy(sound); }
        if (content) Destroy(content);
        if (logoMaterial) Destroy(logoMaterial);
        if (scribbleMaterial) Destroy(scribbleMaterial);
        // The menu and credits also use the studio logo. Release only the splash-only assets.
        for (int i=0;i<loaded.Length;i++) { if (i != 0 && loaded[i] && !(loaded[i] is Shader)) Resources.UnloadAsset(loaded[i]);loaded[i]=null; }
    }
    private void OnDestroy() { Release(); }
}
