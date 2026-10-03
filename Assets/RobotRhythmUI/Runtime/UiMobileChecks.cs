using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class UiMobileChecks : MonoBehaviour
    {
        [SerializeField]
        private UiController ui;
        private Touchscreen touch;
        private string output;
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
            int index = Array.IndexOf(args, "-uiQA");
            if (index < 0)
                yield break;
            output = args[index + 1];
            Directory.CreateDirectory(output);
            var stack = new Stack<IEnumerator>();
            stack.Push(Run());
            while (stack.Count > 0)
            {
                object next;
                try
                {
                    if (!stack.Peek().MoveNext())
                    {
                        stack.Pop();
                        continue;
                    }

                    next = stack.Peek().Current;
                }
                catch (Exception error)
                {
                    File.WriteAllText(Path.Combine(output, "result.txt"), "FAIL\n" + error);
                    Application.Quit(1);
                    yield break;
                }

                if (next is IEnumerator nested)
                    stack.Push(nested);
                else
                    yield return next;
            }

            InputSystem.RemoveDevice(touch);
            File.WriteAllText(Path.Combine(output, "result.txt"), "PASS\nTouch-only menus, route/package selection, static placeholder, logo animation, reduced motion, settings, safe area, and text rendering.\n");
            Application.Quit();
        }

        private static void Require(bool value, string message)
        {
            if (!value)
                throw new Exception(message);
        }

        private IEnumerator Run()
        {
            yield return null;
            yield return null;
            var module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            Require(module.point != null && module.leftClick != null, "Touch references");
            foreach (var binding in module.actionsAsset.bindings)
                Require(binding.path.StartsWith("<Touchscreen>/"), "Touch bindings only");
            Require(module.move == null && module.submit == null && module.cancel == null, "Touch navigation only");
            Require(ui.PageCount == 5, "Five menu and placeholder pages");
            Canvas.ForceUpdateCanvases();
            foreach (var text in ui.GetComponentsInChildren<Text>(true))
                Require(text.pixelsPerUnit + .01f >= text.rectTransform.lossyScale.x, "Text must render at screen resolution: " + text.name);
            foreach (var component in ui.GetComponentsInChildren<MonoBehaviour>(true))
                Require(component.GetType().Name != "GesturePad" && component.GetType().Name != "UiPreviewDriver", "No gameplay components");
            touch = InputSystem.AddDevice<Touchscreen>();
            yield return null;
            if (ui.ReducedMotion)
                FindButton("Motion").onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.1f);
            yield return Capture("01-Home");
            var logo = ui.GetComponentInChildren<LogoMotion>(true);
            Vector2 before = logo.TopLayer.anchoredPosition;
            yield return new WaitForSecondsRealtime(.4f);
            Require(Vector2.Distance(before, logo.TopLayer.anchoredPosition) > .1f, "Logo animates");
            yield return Tap("Play");
            Require(ui.Page == UiPage.Routes, "Touch route navigation");
            yield return Tap("RouteTutorial");
            Require(ui.Route == 0, "Tutorial selection");
            yield return Tap("RouteStandard");
            yield return Tap("PackageHeavy");
            Require(ui.Package == 1, "Package selection");
            yield return Tap("PackageLight");
            yield return Capture("02-Routes");
            yield return Tap("SelectRoute");
            Require(ui.Page == UiPage.Placeholder, "Selection opens only placeholder");
            yield return Capture("04-Placeholder");
            yield return new WaitForSecondsRealtime(.4f);
            Require(ui.Page == UiPage.Placeholder, "Placeholder remains static");
            yield return Tap("PlaceholderRoutes");
            yield return Tap("RoutesBack");
            yield return Tap("HowTo");
            yield return Capture("03-Controls");
            yield return Tap("ControlsDone");
            yield return Tap("HomeSettings");
            yield return Capture("05-Settings");
            bool motion = ui.ReducedMotion;
            yield return Tap("Motion");
            Require(motion != ui.ReducedMotion, "Motion setting");
            if (!ui.ReducedMotion)
                yield return Tap("Motion");
            yield return Tap("SettingsBack");
            yield return null;
            Vector2 reduced = logo.TopLayer.anchoredPosition;
            yield return new WaitForSecondsRealtime(.2f);
            Require(logo.TopLayer.anchoredPosition == reduced, "Reduced motion stops logo animation");
            yield return Tap("HomeSettings");
            yield return Tap("Motion");
            yield return Tap("SettingsBack");
            yield return Tap("Play");
            var safe = ui.GetComponent<LandscapeSafeArea>();
            safe.enabled = false;
            var inset = new Rect(70, 32, Screen.width - 100, Screen.height - 64);
            safe.Apply(inset, new Vector2(Screen.width, Screen.height));
            var corners = new Vector3[4];
            safe.Frame.GetWorldCorners(corners);
            Require(corners[0].x >= inset.xMin - 1 && corners[2].x <= inset.xMax + 1 && corners[0].y >= inset.yMin - 1 && corners[2].y <= inset.yMax + 1, "Safe area");
            yield return Capture("06-SafeArea");
        }

        private IEnumerator Tap(string name)
        {
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)FindButton(name).transform;
            var point = rect.TransformPoint(rect.rect.center);
            yield return Gesture(point, point, false);
        }

        private IEnumerator Gesture(Vector2 start, Vector2 end, bool hold)
        {
            InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, position = start, phase = UnityEngine.InputSystem.TouchPhase.Began });
            yield return null;
            yield return null;
            if (hold)
                yield return new WaitForSecondsRealtime(.35f);
            if (start != end)
            {
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, position = end, phase = UnityEngine.InputSystem.TouchPhase.Moved });
                yield return null;
                yield return null;
            }

            InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, position = end, phase = UnityEngine.InputSystem.TouchPhase.Ended });
            yield return null;
            yield return null;
        }

        private IEnumerator Capture(string name)
        {
            yield return null;
            var canvas = ui.GetComponent<Canvas>();
            var go = new GameObject("Capture");
            var camera = go.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(23, 49, 60, 255);
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -1000);
            var target = new RenderTexture(Screen.width, Screen.height, 24);
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var old = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
            foreach (var text in ui.GetComponentsInChildren<Text>())
            {
                string value = text.text.ToLowerInvariant();
                foreach (var forbidden in new[]
                {
                    "keyboard",
                    "mouse",
                    "press space",
                    "click to"
                }

                )
                    Require(!value.Contains(forbidden), "Desktop instructions: " + text.text);
                Require(string.IsNullOrWhiteSpace(value) || text.cachedTextGenerator.vertexCount > 0, "Missing text: " + text.name);
                Require(text.preferredHeight <= text.rectTransform.rect.height + 2, "Text overflow: " + text.name);
            }

            RenderTexture.active = old;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            camera.targetTexture = null;
            target.Release();
            Destroy(target);
            Destroy(texture);
            Destroy(go);
            Canvas.ForceUpdateCanvases();
            yield return null;
        }
    }
}
