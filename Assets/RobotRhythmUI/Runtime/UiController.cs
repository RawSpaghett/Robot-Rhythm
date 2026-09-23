using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class UiController : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private GameObject[] pages;

        [Header("Route Placeholder")]
        [SerializeField] private Text routeNumber;
        [SerializeField] private Text routeTitle;
        [SerializeField] private Text routeDetail;
        [SerializeField] private Graphic[] routeOptions;

        [Header("Package Placeholder")]
        [SerializeField] private Text packageDescription;
        [SerializeField] private Graphic[] packageOptions;

        [Header("Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeValue;
        [SerializeField] private Text motionLabel;

        [Header("Appearance and Audio")]
        [SerializeField] private UiTheme theme;
        [SerializeField] private AudioClip clickSound;

        private const string VolumeKey = "rr.ui.v1.volume";
        private const string MotionKey = "rr.ui.v1.motion";
        private AudioSource audioSource;
        private AudioClip defaultClick;

        public UiPage Page { get; private set; }
        public bool ReducedMotion { get; private set; }
        public int Route { get; private set; } = 1;
        public int Package { get; private set; }
        public float UiVolume { get; private set; }
        public int PageCount => pages.Length;

        private void Awake()
        {
            if (theme == null || pages == null || pages.Length != 5 ||
                routeNumber == null || routeTitle == null || routeDetail == null ||
                packageDescription == null || volumeSlider == null ||
                volumeValue == null || motionLabel == null ||
                routeOptions == null || routeOptions.Length != 2 ||
                packageOptions == null || packageOptions.Length != 2)
            {
                Debug.LogError("UiController needs its menu references assigned.", this);
                enabled = false;
                return;
            }

            UiVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.5f));
            ReducedMotion = PlayerPrefs.GetInt(MotionKey, 0) == 1;
            volumeSlider.SetValueWithoutNotify(UiVolume);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            if (clickSound == null)
                defaultClick = CreateDefaultClick();

            SelectRoute(Route);
            SelectPackage(Package);
            UpdateSettings();
            ShowPage(UiPage.Home);
        }

        // Button events use the page order shown in the Navigation array.
        public void OpenPage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pages.Length)
                return;

            ShowPage((UiPage)pageIndex);
        }

        public void ShowPage(UiPage page)
        {
            Page = page;
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == (int)page);
        }

        public void CycleRoute()
        {
            SelectRoute(1 - Route);
        }

        public void SelectRoute(int value)
        {
            // These two entries are menu examples, not a gameplay route source.
            Route = Mathf.Clamp(value, 0, 1);
            routeNumber.text = Route == 0 ? "00 / TRAINING" : "01 / CITY ROUTE";
            routeTitle.text = Route == 0 ? "FIRST SHIFT" : "SPECIAL DELIVERY";
            routeDetail.text = Route == 0 ? "TUTORIAL" : "STANDARD";
            UpdateSelection(routeOptions, Route);
        }

        public void SelectPackage(int value)
        {
            Package = Mathf.Clamp(value, 0, 1);
            packageDescription.text = Package == 0 ? "Small parcel" : "Heavy parcel";
            UpdateSelection(packageOptions, Package);
        }

        private void UpdateSelection(Graphic[] options, int selectedIndex)
        {
            for (int i = 0; i < options.Length; i++)
                options[i].color = i == selectedIndex ? theme.yellow : theme.cream;
        }

        public void SetVolume(float value)
        {
            UiVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, UiVolume);
            UpdateSettings();
        }

        public void ToggleReducedMotion()
        {
            ReducedMotion = !ReducedMotion;
            PlayerPrefs.SetInt(MotionKey, ReducedMotion ? 1 : 0);
            PlayerPrefs.Save();
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            volumeValue.text = Mathf.RoundToInt(UiVolume * 100f) + "%";
            motionLabel.text = ReducedMotion ? "REDUCED MOTION: ON" : "REDUCED MOTION: OFF";
        }

        public void PlayClick()
        {
            if (audioSource != null)
                audioSource.PlayOneShot(clickSound != null ? clickSound : defaultClick, UiVolume);
        }

        private AudioClip CreateDefaultClick()
        {
            // Preserve the short menu tone until a UI sound is assigned.
            const int sampleRate = 44100;
            var samples = new float[1800];
            for (int i = 0; i < samples.Length; i++)
            {
                float fade = 1f - (float)i / samples.Length;
                samples[i] = Mathf.Sin(i * 2f * Mathf.PI * 740f / sampleRate) * 0.15f * fade;
            }

            var clip = AudioClip.Create("UI click", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                PlayerPrefs.Save();
        }

        private void OnDisable()
        {
            // Flush volume changes once when leaving the UI, not on every drag step.
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (defaultClick != null)
                Destroy(defaultClick);
        }
    }
}
