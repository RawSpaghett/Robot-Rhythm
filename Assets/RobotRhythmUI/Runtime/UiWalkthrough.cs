using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class UiWalkthrough : MonoBehaviour
    {
        [SerializeField]
        private UiController ui;
        private string output;
        private Camera capture;
        private RenderTexture target;
        private Texture2D pixels;
        private Image contact;
        private RectTransform contactRect;
        private int frame;
        private int homeFrame;
        private float touchAt = -10, touchDuration = .5f;
        private Vector2 touchFrom, touchTo;
        private const int Fps = 30, Seconds = 50;
        private const int CaptureWidth = 2560, CaptureHeight = 1440;
        private Button FindButton(string name)
        {
            foreach (var button in ui.GetComponentsInChildren<Button>(true))
            {
                if (button.name == name)
                    return button;
            }

            throw new InvalidOperationException("Menu button not found: " + name);
        }

        private IEnumerator Start()
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                yield break;
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-uiVideo");
            if (index < 0)
            {
                enabled = false;
                yield break;
            }

            output = args[index + 1];
            Directory.CreateDirectory(output);
            yield return null;
            yield return null;
            foreach (var logo in ui.GetComponentsInChildren<LogoMotion>(true))
                logo.enabled = false;
            ui.ShowPage(UiPage.Routes);
            ui.ShowPage(UiPage.Home);
            ui.GetComponentInChildren<Slider>(true).value = .5f;
            if (ui.ReducedMotion)
                FindButton("Motion").onClick.Invoke();
            var canvas = ui.GetComponent<Canvas>();
            capture = new GameObject("CaptureCamera").AddComponent<Camera>();
            capture.enabled = false;
            capture.clearFlags = CameraClearFlags.SolidColor;
            capture.backgroundColor = new Color32(23, 49, 60, 255);
            capture.orthographic = true;
            capture.transform.position = new Vector3(0, 0, -1000);
            target = new RenderTexture(CaptureWidth, CaptureHeight, 24);
            target.Create();
            capture.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = capture;
            canvas.planeDistance = 10;
            canvas.GetComponent<CanvasScaler>().enabled = false;
            canvas.scaleFactor = CaptureWidth / 1280f;
            var safeArea = canvas.GetComponent<LandscapeSafeArea>();
            safeArea.enabled = false;
            safeArea.Apply(new Rect(0, 0, CaptureWidth, CaptureHeight), new Vector2(CaptureWidth, CaptureHeight));
            pixels = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            var indicator = new GameObject("TouchContact", typeof(RectTransform), typeof(Image));
            indicator.transform.SetParent(ui.GetComponent<LandscapeSafeArea>().Frame, false);
            contact = indicator.GetComponent<Image>();
            contact.raycastTarget = false;
            contactRect = (RectTransform)indicator.transform;
            contactRect.anchorMin = contactRect.anchorMax = contactRect.pivot = new Vector2(0, 1);
            contactRect.sizeDelta = new Vector2(38, 38);
            var dot = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float radius = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    dot.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(29 - radius) * Mathf.Clamp01(radius - 23)));
                }

            dot.Apply();
            contact.sprite = Sprite.Create(dot, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f));
            contact.color = Color.clear;
            for (frame = 0; frame < Seconds * Fps; frame++)
            {
                Timeline(frame);
                foreach (var logo in ui.GetComponentsInChildren<LogoMotion>())
                    logo.Render((frame - homeFrame) / (float)Fps);
                float phase = (frame / (float)Fps - touchAt) / touchDuration;
                contact.color = phase >= 0 && phase < 1 ? new Color(1f, .39f, .20f, 1 - phase) : Color.clear;
                contactRect.anchoredPosition = Vector2.Lerp(touchFrom, touchTo, Mathf.Clamp01(phase));
                Canvas.ForceUpdateCanvases();
                yield return null;
                Canvas.ForceUpdateCanvases();
                capture.Render();
                var old = RenderTexture.active;
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                pixels.Apply();
                RenderTexture.active = old;
                File.WriteAllBytes(Path.Combine(output, frame.ToString("D5") + ".png"), pixels.EncodeToPNG());
            }

            File.WriteAllText(Path.Combine(output, "complete.txt"), "1500 frames at 30 fps; 50 seconds.\n");
            Application.Quit();
        }

        private void Tap(string name)
        {
            var button = FindButton(name);
            var rect = (RectTransform)button.transform;
            Vector3 local = ui.GetComponent<LandscapeSafeArea>().Frame.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
            Pulse(new Vector2(local.x + 640, local.y - 360), Vector2.zero, .55f);
            button.onClick.Invoke();
            if (ui.Page == UiPage.Home)
                homeFrame = frame;
        }

        private void Pulse(Vector2 from, Vector2 delta, float duration)
        {
            touchFrom = from - new Vector2(19, -19);
            touchTo = touchFrom + delta;
            touchAt = frame / (float)Fps;
            touchDuration = duration;
        }

        private void Timeline(int value)
        {
            switch (value)
            {
                case 180:
                    Tap("Play");
                    break;
                case 270:
                    Tap("RouteTutorial");
                    break;
                case 330:
                    Tap("RouteStandard");
                    break;
                case 390:
                    Tap("PackageHeavy");
                    break;
                case 450:
                    Tap("PackageLight");
                    break;
                case 510:
                    Tap("SelectRoute");
                    break;
                case 630:
                    Tap("PlaceholderRoutes");
                    break;
                case 690:
                    Tap("RoutesBack");
                    break;
                case 750:
                    Tap("HowTo");
                    break;
                case 900:
                    Tap("ControlsDone");
                    break;
                case 960:
                    Tap("HomeSettings");
                    break;
                case 1050:
                    ui.GetComponentInChildren<Slider>(true).value = .7f;
                    Pulse(new Vector2(634, -355), Vector2.right * 150, .6f);
                    break;
                case 1110:
                    Tap("Motion");
                    break;
                case 1170:
                    Tap("SettingsBack");
                    break;
                case 1260:
                    Tap("HomeSettings");
                    break;
                case 1290:
                    Tap("Motion");
                    break;
                case 1320:
                    Tap("SettingsBack");
                    break;
            }
        }
    }
}
