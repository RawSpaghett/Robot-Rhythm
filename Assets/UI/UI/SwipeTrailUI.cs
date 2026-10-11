using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Draws pointer movement only. InputManager still decides which action to perform.
public class SwipeTrailUI : MaskableGraphic
{
    private struct Mark
    {
        public Vector2 position;
        public float angle;
        public float scale;
    }

    private readonly List<Mark> dots = new List<Mark>();
    private int markNumber;

    // Outline of the dash in BubbleCircuit-Regular, centered and scaled to width 1.
    private static readonly Vector2[] dashOutline =
    {
        new Vector2(-0.400971f, 0.109021f),
        new Vector2(-0.424001f, 0.101417f),
        new Vector2(-0.445259f, 0.089354f),
        new Vector2(-0.464001f, 0.073518f),
        new Vector2(-0.479485f, 0.054595f),
        new Vector2(-0.490966f, 0.033271f),
        new Vector2(-0.497703f, 0.010231f),
        new Vector2(-0.499970f, -0.009578f),
        new Vector2(-0.498694f, -0.029559f),
        new Vector2(-0.494101f, -0.049197f),
        new Vector2(-0.486421f, -0.067977f),
        new Vector2(-0.475883f, -0.085385f),
        new Vector2(-0.462715f, -0.100907f),
        new Vector2(-0.447193f, -0.113923f),
        new Vector2(-0.429785f, -0.124080f),
        new Vector2(-0.411005f, -0.131264f),
        new Vector2(-0.391367f, -0.135361f),
        new Vector2(-0.371386f, -0.136257f),
        new Vector2(-0.351577f, -0.133837f),
        new Vector2(-0.313502f, -0.127500f),
        new Vector2(-0.267194f, -0.122936f),
        new Vector2(-0.212654f, -0.120202f),
        new Vector2(-0.149881f, -0.119354f),
        new Vector2(-0.078877f, -0.120449f),
        new Vector2(0.000361f, -0.123546f),
        new Vector2(0.088698f, -0.126624f),
        new Vector2(0.168516f, -0.127586f),
        new Vector2(0.239874f, -0.126376f),
        new Vector2(0.302827f, -0.122936f),
        new Vector2(0.357434f, -0.117210f),
        new Vector2(0.403751f, -0.109139f),
        new Vector2(0.422884f, -0.102498f),
        new Vector2(0.440873f, -0.093056f),
        new Vector2(0.457262f, -0.081098f),
        new Vector2(0.471593f, -0.066910f),
        new Vector2(0.483408f, -0.050779f),
        new Vector2(0.492250f, -0.032989f),
        new Vector2(0.497757f, -0.013161f),
        new Vector2(0.499949f, 0.006953f),
        new Vector2(0.498939f, 0.026953f),
        new Vector2(0.494842f, 0.046439f),
        new Vector2(0.487772f, 0.065009f),
        new Vector2(0.477843f, 0.082265f),
        new Vector2(0.462388f, 0.101084f),
        new Vector2(0.443389f, 0.116186f),
        new Vector2(0.421760f, 0.127286f),
        new Vector2(0.398415f, 0.134099f),
        new Vector2(0.374271f, 0.136338f),
        new Vector2(0.350240f, 0.133718f),
        new Vector2(0.314233f, 0.127382f),
        new Vector2(0.270050f, 0.122818f),
        new Vector2(0.217749f, 0.120083f),
        new Vector2(0.157387f, 0.119235f),
        new Vector2(0.089022f, 0.120331f),
        new Vector2(0.012710f, 0.123427f),
        new Vector2(-0.076818f, 0.126505f),
        new Vector2(-0.158114f, 0.127467f),
        new Vector2(-0.231177f, 0.126257f),
        new Vector2(-0.296008f, 0.122818f),
        new Vector2(-0.352606f, 0.117091f),
    };
    private InputManager input;
    private float markSize = 22f;
    private float spacing = 30f;
    private float fadeDuration = 0.2f;
    private float fade;
    private bool pressed;
    private bool ignorePress;

    public void Bind(InputManager source, float size, float gap, float duration)
    {
        input = source;
        markSize = Mathf.Max(1f, size);
        spacing = Mathf.Max(1f, gap);
        fadeDuration = Mathf.Max(0.01f, duration);
        raycastTarget = false;
        Clear();
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (input == null || !input.isActiveAndEnabled || pointer == null || Time.timeScale == 0f)
        {
            Clear();
            return;
        }
        bool down = pointer.press.isPressed;
        // A press that began on a menu should not leave a trail after resuming.
        if (ignorePress)
        {
            if (!down) ignorePress = false;
            return;
        }
        if (down)
        {
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointer.position.ReadValue(), camera, out Vector2 point);
            if (!pressed)
            {
                dots.Clear();
                markNumber = 0;
                AddMark(point, 0f);
            }
            Vector2 last = dots[dots.Count - 1].position;
            int count = Mathf.Min(64, Mathf.FloorToInt(Vector2.Distance(last, point) / spacing));
            Vector2 direction = (point - last).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (count > 0 && dots.Count == 1)
            {
                Mark first = dots[0];
                first.angle = angle;
                dots[0] = first;
            }
            for (int i = 1; i <= count; i++) AddMark(last + direction * spacing * i, angle);
            if (dots.Count > 64) dots.RemoveRange(0, dots.Count - 64);
            fade = 1f;
            pressed = true;
            SetVerticesDirty();
        }
        else if (fade > 0f)
        {
            pressed = false;
            fade = Mathf.Max(0f, fade - Time.unscaledDeltaTime / fadeDuration);
            SetVerticesDirty();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
        ignorePress = Pointer.current != null && Pointer.current.press.isPressed;
    }

    protected override void OnDisable()
    {
        Clear();
        base.OnDisable();
    }

    private void Clear()
    {
        dots.Clear();
        pressed = false;
        fade = 0f;
        SetVerticesDirty();
    }

    private void AddMark(Vector2 point, float angle)
    {
        // Small fixed variations, so marks do not wobble while the finger is still.
        dots.Add(new Mark
        {
            position = point,
            angle = angle + (markNumber % 5 - 2) * 2f,
            scale = 0.94f + (markNumber % 4) * 0.04f
        });
        markNumber++;
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Color fill = color;
        fill.a *= fade;
        foreach (Mark mark in dots)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(mark.position, fill, Vector2.zero);
            Quaternion rotation = Quaternion.Euler(0f, 0f, mark.angle);
            foreach (Vector2 edge in dashOutline)
            {
                Vector3 offset = rotation * (Vector3)(edge * markSize * mark.scale);
                mesh.AddVert((Vector3)mark.position + offset, fill, Vector2.zero);
            }
            for (int i = 0; i < dashOutline.Length; i++)
                mesh.AddTriangle(first, first + 1 + i, first + 1 + (i + 1) % dashOutline.Length);
        }
    }
}
