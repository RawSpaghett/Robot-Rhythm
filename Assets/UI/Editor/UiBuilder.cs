using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RobotRhythm.UI.Editor
{
    public static class UiBuilder
    {
        private const string Root = "Assets/RobotRhythmUI";
        private static UiTheme theme;
        private static GameObject buttonTemplate;
        private static Color Ink => theme.ink;
        private static Color Cream => theme.cream;
        private static Color Teal => theme.teal;
        private static Color Orange => theme.orange;
        private static Color Yellow => theme.yellow;

        private static Sprite logoSprite, logoTop, logoBottom;
        [MenuItem("Tools/Robot Rhythm UI/Open preview")]
        public static void OpenPreview()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(Root + "/Scenes/RobotRhythmUIPreview.unity");
        }

        [MenuItem("Tools/Robot Rhythm UI/Rebuild authored preview")]
        public static void RebuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!EditorUtility.DisplayDialog("Rebuild UI preview?", "This replaces the UI prefabs and preview scene. Edit the existing prefab for normal layout changes. Back up any custom edits before rebuilding.", "Rebuild", "Cancel"))
                return;
            Generate();
        }

        public static void Generate()
        {
            foreach (string dir in new[]
            {
                "Scenes",
                "Prefabs",
                "Art",
                "Documentation"
            }

            )
                Directory.CreateDirectory(Root + "/" + dir);
            AssetDatabase.Refresh();
            LoadTheme();
            logoSprite = ImportSprite("RobotRhythmLogo");
            logoTop = ImportSprite("LogoTop");
            logoBottom = ImportSprite("LogoBottom");
            CreateButtonTemplate();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("PreviewCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.cullingMask = 0;
            camera.gameObject.AddComponent<AudioListener>();
            var canvas = new GameObject("RobotRhythmUI", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().pixelPerfect = true;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var outer = Box(canvas.transform, "Letterbox", 0, 0, 1280, 720, Ink, false);
            Stretch(outer.rectTransform);
            var frame = Rect(canvas.transform, "SafeAreaFrame", 0, 0, 1280, 720);
            frame.anchorMin = frame.anchorMax = new Vector2(.5f, .5f);
            frame.pivot = new Vector2(.5f, .5f);
            frame.anchoredPosition = Vector2.zero;
            var safe = canvas.AddComponent<LandscapeSafeArea>();
            SetReference(safe, "frame", frame);
            var ui = canvas.AddComponent<UiController>();
            var pages = new GameObject[5];
            for (int i = 0; i < pages.Length; i++)
            {
                var page = Rect(frame, ((UiPage)i).ToString(), 0, 0, 1280, 720);
                pages[i] = page.gameObject;
                Box(page, "Paper", 0, 0, 1280, 720, Cream, false);
            }

            Home(pages[0].transform, ui);
            Routes(pages[1].transform);
            Controls(pages[2].transform);
            Placeholder(pages[3].transform);
            Settings(pages[4].transform);
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == 0);
            SetReferences(ui, "pages", pages);
            ConfigureReferences(ui);
            var prefabPath = Root + "/Prefabs/RobotRhythmUI.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvas, prefabPath, InteractionMode.AutomatedAction);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<EventSystem>().sendNavigationEvents = false;
            var savedInput = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root + "/TouchUI.inputactions");
            if (savedInput == null)
                throw new InvalidOperationException("Assign the existing TouchUI input asset before rebuilding.");
            string inputPath = AssetDatabase.GetAssetPath(savedInput);
            var module = events.GetComponent<InputSystemUIInputModule>();
            module.actionsAsset = savedInput;
            var references = AssetDatabase.LoadAllAssetsAtPath(inputPath);
            module.point = Array.Find(references, item => item is InputActionReference reference && reference.action.name == "Point") as InputActionReference;
            module.leftClick = Array.Find(references, item => item is InputActionReference reference && reference.action.name == "Press") as InputActionReference;
            module.move = null;
            module.submit = null;
            module.cancel = null;
            module.rightClick = null;
            module.middleClick = null;
            module.scrollWheel = null;
            module.trackedDevicePosition = null;
            module.trackedDeviceOrientation = null;
            var review = new GameObject("UIReview");
            SetReference(review.AddComponent<UiMobileChecks>(), "ui", ui);
            SetReference(review.AddComponent<UiWalkthrough>(), "ui", ui);
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == 0);
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/RobotRhythmUIPreview.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("ROBOT_RHYTHM_UI_GENERATED");
        }

        private static void LoadTheme()
        {
            theme = AssetDatabase.LoadAssetAtPath<UiTheme>(Root + "/UiTheme.asset");
            if (theme == null)
                throw new InvalidOperationException("The UI theme asset is missing.");
        }

        public static void ConfigureReferences(UiController ui)
        {
            var texts = ui.GetComponentsInChildren<Text>(true);
            var buttons = ui.GetComponentsInChildren<Button>(true);
            foreach (string name in new[]
            {
                "RouteNumber",
                "RouteTitle",
                "RouteDetail",
                "PackageDescription",
                "VolumeValue",
                "MotionLabel"
            }

            )
            {
                string field = char.ToLowerInvariant(name[0]) + name.Substring(1);
                SetReference(ui, field, Array.Find(texts, text => text.name == name));
            }

            SetReference(ui, "theme", AssetDatabase.LoadAssetAtPath<UiTheme>(Root + "/UiTheme.asset"));
            var slider = ui.GetComponentInChildren<Slider>(true);
            SetReference(ui, "volumeSlider", slider);
            slider.onValueChanged = new Slider.SliderEvent();
            UnityEventTools.AddPersistentListener(slider.onValueChanged, ui.SetVolume);
            Button FindButton(string name) => Array.Find(buttons, button => button.name == name);
            Graphic Surface(string name) => FindButton(name).transform.Find("Surface").GetComponent<Graphic>();
            SetReferences(ui, "routeOptions", new UnityEngine.Object[] { Surface("RouteTutorial"), Surface("RouteStandard") });
            SetReferences(ui, "packageOptions", new UnityEngine.Object[] { Surface("PackageLight"), Surface("PackageHeavy") });
            foreach (var button in buttons)
            {
                button.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(button.onClick, ui.PlayClick);
            }

            foreach (string name in new[]
            {
                "RoutesBack",
                "ControlsBack",
                "ControlsDone",
                "SettingsBack"
            }

            )
                UnityEventTools.AddIntPersistentListener(FindButton(name).onClick, ui.OpenPage, (int)UiPage.Home);
            foreach (string name in new[]
            {
                "Play",
                "PlaceholderBack",
                "PlaceholderRoutes"
            }

            )
                UnityEventTools.AddIntPersistentListener(FindButton(name).onClick, ui.OpenPage, (int)UiPage.Routes);
            UnityEventTools.AddIntPersistentListener(FindButton("HowTo").onClick, ui.OpenPage, (int)UiPage.Controls);
            UnityEventTools.AddIntPersistentListener(FindButton("HomeSettings").onClick, ui.OpenPage, (int)UiPage.Settings);
            UnityEventTools.AddIntPersistentListener(FindButton("SelectRoute").onClick, ui.OpenPage, (int)UiPage.Placeholder);
            UnityEventTools.AddIntPersistentListener(FindButton("RouteTutorial").onClick, ui.SelectRoute, 0);
            UnityEventTools.AddIntPersistentListener(FindButton("RouteStandard").onClick, ui.SelectRoute, 1);
            UnityEventTools.AddIntPersistentListener(FindButton("PackageLight").onClick, ui.SelectPackage, 0);
            UnityEventTools.AddIntPersistentListener(FindButton("PackageHeavy").onClick, ui.SelectPackage, 1);
            UnityEventTools.AddPersistentListener(FindButton("PreviousRoute").onClick, ui.CycleRoute);
            UnityEventTools.AddPersistentListener(FindButton("NextRoute").onClick, ui.CycleRoute);
            UnityEventTools.AddPersistentListener(FindButton("Motion").onClick, ui.ToggleReducedMotion);
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReferences(UnityEngine.Object target, string field, UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(target);
            var array = serialized.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)obj.transform;
            r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            return r;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static Image Box(Transform parent, string name, float x, float y, float w, float h, Color color, bool round = true)
        {
            var r = Rect(parent, name, x, y, w, h);
            var image = r.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (round)
            {
                image.sprite = theme.rounded;
                image.type = Image.Type.Sliced;
            }
            else
                image.sprite = theme.solid;
            return image;
        }

        private static CutPanel Cut(Transform p, string name, float x, float y, float w, float h, Color color, float corner = 18)
        {
            var graphic = Rect(p, name, x, y, w, h).gameObject.AddComponent<CutPanel>();
            graphic.color = color;
            graphic.cut = corner;
            graphic.raycastTarget = false;
            return graphic;
        }

        private static RectTransform Panel(Transform p, string name, float x, float y, float w, float h, Color color)
        {
            Cut(p, name + "Offset", x + 7, y + 8, w, h, Ink);
            var edge = Cut(p, name, x, y, w, h, Ink);
            Cut(edge.transform, "Surface", 3, 3, w - 6, h - 6, color, 15);
            return edge.rectTransform;
        }

        private static void Rule(Transform p, string name, float x, float y, float w, Color color)
        {
            Box(p, name, x, y, w, 2, color, false);
        }

        private static void Diagonal(Transform p, string name, float x, float y, float w, float h, Color color)
        {
            var line = Box(p, name, x, y, Mathf.Sqrt(w * w + h * h), 2, color, false).rectTransform;
            line.localEulerAngles = new Vector3(0, 0, -Mathf.Atan2(h, w) * Mathf.Rad2Deg);
        }

        private static Text Label(Transform p, string name, string value, float x, float y, float w, float h, int size, Color color, bool display = false, TextAnchor align = TextAnchor.UpperLeft)
        {
            var text = Rect(p, name, x, y, w, h).gameObject.AddComponent<Text>();
            text.font = display ? theme.displayFont : theme.boldFont;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.raycastTarget = false;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void CreateButtonTemplate()
        {
            var root = new GameObject("ActionButton", typeof(RectTransform));
            ((RectTransform)root.transform).sizeDelta = new Vector2(300, 80);
            var offset = Cut(root.transform, "Offset", 4, 6, 300, 80, Ink);
            Stretch(offset.rectTransform);
            offset.rectTransform.anchoredPosition = new Vector2(4, -6);
            var edge = Cut(root.transform, "Border", 0, 0, 300, 80, Ink);
            Stretch(edge.rectTransform);
            var surface = Cut(root.transform, "Surface", 3, 3, 294, 74, Cream, 14);
            Stretch(surface.rectTransform);
            surface.rectTransform.offsetMin = new Vector2(3, 3);
            surface.rectTransform.offsetMax = new Vector2(-3, -3);
            surface.raycastTarget = true;
            var label = Label(root.transform, "Label", "BUTTON", 8, 8, 284, 64, 29, Ink, true, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(15, 8);
            label.rectTransform.offsetMax = new Vector2(-15, -8);
            var button = root.AddComponent<Button>();
            button.targetGraphic = surface;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(.86f, .84f, .77f);
            colors.fadeDuration = .08f;
            button.colors = colors;
            buttonTemplate = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/ActionButton.prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static Button Button(Transform p, string name, string value, float x, float y, float w, float h, Color color, int size = 28)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(buttonTemplate, p);
            obj.name = name;
            var r = (RectTransform)obj.transform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            obj.transform.Find("Surface").GetComponent<Graphic>().color = color;
            var label = obj.transform.Find("Label").GetComponent<Text>();
            label.name = name + "Label";
            label.text = value;
            label.fontSize = size;
            var hit = Box(obj.transform, "TouchTarget", (w - Mathf.Max(88, w)) / 2, (h - Mathf.Max(88, h)) / 2, Mathf.Max(88, w), Mathf.Max(88, h), Color.clear, false);
            hit.raycastTarget = true;
            return obj.GetComponent<Button>();
        }

        private static void Header(Transform p, string eyebrow, string title, string backName)
        {
            Button(p, backName, "<", 40, 41, 76, 76, Orange, 34);
            Label(p, "Eyebrow", eyebrow, 150, 27, 820, 30, 17, Ink);
            Label(p, "PageTitle", title, 147, 56, 865, 78, 53, Ink, true);
            Rule(p, "HeaderRule", 42, 148, 1196, Ink);
            Box(p, "HeaderAccent", 42, 148, 180, 6, Orange, false);
            var brand = Cut(p, "BrandPlate", 1052, 31, 192, 93, Teal, 14);
            Picture(brand.transform, "BrandMark", logoSprite, 9, 10, 171, 71);
        }

        private static Image Picture(Transform p, string name, Sprite sprite, float x, float y, float w, float h, bool preserve = true)
        {
            var image = Box(p, name, x, y, w, h, Color.white, false);
            image.sprite = sprite;
            image.preserveAspect = preserve;
            return image;
        }

        private static Sprite ImportSprite(string name)
        {
            string path = Root + "/Art/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void Home(Transform p, UiController ui)
        {
            Box(p, "TopRail", 0, 0, 1280, 17, Ink, false);
            Box(p, "TopAccent", 0, 17, 226, 8, Orange, false);
            Box(p, "LowerRail", 0, 701, 1280, 19, Ink, false);
            Box(p, "LowerAccent", 1082, 693, 198, 8, Yellow, false);
            var plate = Panel(p, "LogoPlate", 52, 193, 742, 348, Teal);
            plate.localEulerAngles = new Vector3(0, 0, 2);
            p.Find("LogoPlateOffset").localEulerAngles = new Vector3(0, 0, 2);
            var top = Picture(plate, "LogoTop", logoTop, 24, 31, 691, 284);
            var bottom = Picture(plate, "LogoBottom", logoBottom, 24, 31, 691, 284);
            var motion = plate.gameObject.AddComponent<LogoMotion>();
            SetReference(motion, "ui", ui);
            SetReference(motion, "top", top.rectTransform);
            SetReference(motion, "bottom", bottom.rectTransform);
            Cut(p, "BrandTab", 70, 549, 248, 18, Orange, 7);
            Cut(p, "BrandTabShort", 332, 549, 105, 18, Yellow, 7);
            Label(p, "MenuCaption", "MAIN MENU", 869, 139, 346, 35, 19, Ink);
            Rule(p, "MenuRule", 869, 180, 330, Ink);
            var play = Button(p, "Play", "PLAY", 862, 215, 346, 91, Orange, 36);
            var how = Button(p, "HowTo", "CONTROLS", 862, 342, 346, 83, Cream, 29);
            var settings = Button(p, "HomeSettings", "SETTINGS", 862, 461, 346, 83, Yellow, 29);
            foreach (var button in new[]
            {
                play,
                how,
                settings
            }

            )
            {
                var label = button.GetComponentInChildren<Text>();
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(31, 8);
                Label(button.transform, "Arrow", ">", 294, 24, 26, 48, 29, Ink, true, TextAnchor.MiddleCenter);
            }
        }

        private static void Routes(Transform p)
        {
            Header(p, "SELECT", "ROUTE", "RoutesBack");
            var card = Panel(p, "RouteCard", 118, 192, 667, 397, Ink);
            Label(card, "RouteNumber", "01 / CITY ROUTE", 30, 26, 610, 32, 20, Yellow);
            Label(card, "RouteTitle", "SPECIAL DELIVERY", 28, 73, 611, 65, 42, Cream, true);
            var artwork = Cut(card, "ArtworkPlaceholder", 30, 160, 607, 155, new Color32(47, 71, 77, 255), 10);
            Diagonal(artwork.transform, "CrossA", 12, 13, 582, 129, new Color32(65, 94, 98, 255));
            Diagonal(artwork.transform, "CrossB", 12, 142, 582, -129, new Color32(65, 94, 98, 255));
            Box(artwork.transform, "LabelSurface", 200, 57, 209, 44, new Color32(47, 71, 77, 255), false);
            Label(artwork.transform, "ArtworkCaption", "ROUTE ART", 207, 65, 193, 30, 18, new Color32(186, 202, 192, 255), false, TextAnchor.MiddleCenter);
            Label(card, "RouteDetail", "STANDARD", 32, 347, 598, 32, 21, Cream);
            Button(p, "PreviousRoute", "<", 21, 334, 70, 84, Yellow, 39);
            Button(p, "NextRoute", ">", 811, 334, 70, 84, Yellow, 39);
            Button(p, "RouteTutorial", "00  FIRST SHIFT", 120, 629, 312, 53, Cream, 21);
            Button(p, "RouteStandard", "01  CITY ROUTE", 463, 629, 321, 53, Yellow, 21);
            Label(p, "PackageEyebrow", "PACKAGE", 930, 201, 303, 35, 22, Ink);
            Rule(p, "PackageRule", 931, 252, 303, Ink);
            Button(p, "PackageLight", "LIGHT", 925, 287, 303, 78, Yellow, 28);
            Button(p, "PackageHeavy", "HEAVY", 925, 399, 303, 78, Cream, 28);
            Label(p, "PackageDescription", "Small parcel", 933, 506, 296, 40, 24, Ink).font = theme.bodyFont;
            Button(p, "SelectRoute", "SELECT", 925, 583, 303, 91, Orange, 30);
        }

        private static void Controls(Transform p)
        {
            Header(p, "TOUCH", "CONTROLS", "ControlsBack");
            string[] actions =
            {
                "JUMP",
                "BRAKE",
                "ACCELERATE",
                "DUCK",
                "LONG JUMP"
            };
            for (int i = 0; i < 5; i++)
            {
                var card = Panel(p, "Control" + i, 44 + i * 241, 207, 219, 290, Cream);
                Cut(card, "NumberTab", 14, 14, 47, 35, Ink, 7);
                Label(card, "Order", (i + 1).ToString("D2"), 17, 20, 40, 26, 17, Cream, false, TextAnchor.MiddleCenter);
                Label(card, "ControlAction", actions[i], 10, 77, 199, 53, i == 2 ? 29 : 32, Ink, true, TextAnchor.MiddleCenter);
                Rule(card, "GestureRule", 19, 141, 180, Ink);
                if (i == 0)
                    GestureBadge(card, "Tap", "TAP", 183, 56, Orange);
                else if (i < 4)
                    GestureBadge(card, "Swipe", "SWIPE", 183, 56, Teal, i == 1 ? 180 : i == 2 ? 0 : 270);
                else
                {
                    GestureBadge(card, "Hold", "HOLD", 157, 43, Yellow);
                    Label(card, "Sequence", "then", 20, 201, 179, 25, 16, Ink, false, TextAnchor.MiddleCenter);
                    GestureBadge(card, "Swipe", "SWIPE", 228, 43, Teal, 90);
                }
            }

            Label(p, "ControlInstructions", "Tap or swipe on the control pad.\nHold before swiping up for a long jump.", 49, 559, 800, 86, 26, Ink).font = theme.bodyFont;
            Button(p, "ControlsDone", "DONE", 963, 576, 271, 82, Orange, 30);
        }

        private static void GestureBadge(Transform p, string name, string value, float y, float height, Color color, float? direction = null)
        {
            var badge = Cut(p, name + "Badge", 20, y, 179, height, color, 9);
            Label(badge.transform, name + "Label", value, direction.HasValue ? 11 : 0, 0, direction.HasValue ? 116 : 179, height, 23, Ink, true, TextAnchor.MiddleCenter);
            if (direction.HasValue)
            {
                var rect = Rect(badge.transform, "Direction", 145, height * .5f, 27, 27);
                rect.pivot = new Vector2(.5f, .5f);
                rect.localEulerAngles = new Vector3(0, 0, direction.Value);
                var arrow = rect.gameObject.AddComponent<DirectionArrow>();
                arrow.color = Ink;
                arrow.raycastTarget = false;
            }
        }

        private static void Placeholder(Transform p)
        {
            Header(p, "LAYOUT", "PLACEHOLDER", "PlaceholderBack");
            var panel = Panel(p, "ReservedView", 120, 207, 1040, 346, new Color32(226, 222, 207, 255));
            Diagonal(panel, "CrossA", 19, 19, 1000, 305, new Color32(187, 188, 174, 255));
            Diagonal(panel, "CrossB", 19, 326, 1000, -305, new Color32(187, 188, 174, 255));
            Cut(panel, "PlaceholderTag", 322, 119, 397, 96, Cream, 17);
            Label(panel, "ReservedLabel", "LEVEL PLACEHOLDER", 339, 150, 365, 43, 29, Ink, true, TextAnchor.MiddleCenter);
            Button(p, "PlaceholderRoutes", "BACK TO ROUTES", 441, 604, 399, 75, Orange, 26);
        }

        private static void Settings(Transform p)
        {
            Header(p, "PREFERENCES", "SETTINGS", "SettingsBack");
            var panel = Panel(p, "SettingsPanel", 155, 209, 970, 412, Cream);
            Cut(panel, "SettingsStripe", 3, 3, 18, 386, Teal, 0);
            Label(panel, "VolumeLabel", "MENU VOLUME", 53, 33, 680, 49, 31, Ink, true);
            Label(panel, "VolumeValue", "50%", 802, 34, 108, 49, 32, Ink, true, TextAnchor.UpperRight);
            var root = Rect(panel, "UISoundSlider", 54, 112, 852, 68);
            var track = Box(root, "Track", 0, 28, 852, 13, new Color32(206, 208, 189, 255), false);
            track.raycastTarget = true;
            var area = Rect(root, "HandleArea", 24, 0, 804, 68);
            var handle = Cut(area, "Handle", 0, 10, 48, 48, Orange, 9);
            handle.raycastTarget = true;
            handle.rectTransform.pivot = new Vector2(.5f, .5f);
            handle.rectTransform.anchorMin = Vector2.zero;
            handle.rectTransform.anchorMax = Vector2.up;
            handle.rectTransform.sizeDelta = new Vector2(48, -20);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            var slider = root.gameObject.AddComponent<Slider>();
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };
            Rule(panel, "SettingsRule", 53, 208, 854, Ink);
            Button(panel, "Motion", "REDUCED MOTION: OFF", 53, 244, 854, 70, Yellow, 27);
            Label(panel, "MotionCopy", "Limits menu and logo animation.", 54, 350, 856, 43, 23, Ink).font = theme.bodyFont;
        }

        public static void Build()
        {
            AssetDatabase.SaveAssets();
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-uiOutput");
            if (index < 0 || index + 1 >= args.Length)
                throw new ArgumentException("-uiOutput is required");
            string path = args[index + 1];
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var result = BuildPipeline.BuildPlayer(new[] { Root + "/Scenes/RobotRhythmUIPreview.unity" }, path, BuildTarget.StandaloneWindows64, BuildOptions.Development);
            if (result.summary.result != BuildResult.Succeeded)
                throw new Exception("UI preview build failed");
            int packageIndex = Array.IndexOf(args, "-uiPackage");
            if (packageIndex >= 0)
                AssetDatabase.ExportPackage(Root, args[packageIndex + 1], ExportPackageOptions.Recurse);
            Debug.Log("ROBOT_RHYTHM_UI_BUILD_PASS");
            EditorApplication.Exit(0);
        }

        public static void ExportReviewPackage()
        {
            AssetDatabase.Refresh();
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-uiPackage");
            if (index < 0 || index + 1 >= args.Length)
                throw new ArgumentException("-uiPackage is required");
            AssetDatabase.ExportPackage(Root, args[index + 1], ExportPackageOptions.Recurse);
            Debug.Log("ROBOT_RHYTHM_UI_PACKAGE_PASS");
            EditorApplication.Exit(0);
        }
    }
}
