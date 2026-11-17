using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Center short lists and incomplete rows; keep larger lists readable in the scroll view.
public class ModifierCardLayout : LayoutGroup
{
    [Min(200)] public float minimumCardWidth = 360;
    [Min(200)] public float singleCardWidth = 640;
    [Min(100)] public float cardHeight = 170;
    [Min(0)] public float spacing = 20;
    public Text browseHint;

    private int columns = 1;
    private readonly List<float> rowHeights = new List<float>();
    private float rowsHeight;
    private float ViewportHeight => ((RectTransform)transform.parent).rect.height;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        columns = rectChildren.Count > 1 && rectTransform.rect.width >= minimumCardWidth * 2 + spacing ? 2 : 1;
        SetLayoutInputForAxis(0, 0, 1, 0);
    }

    public override void SetLayoutHorizontal()
    {
        float width = (rectTransform.rect.width - spacing * (columns - 1)) / columns;
        if (columns == 1) width = Mathf.Min(width, singleCardWidth);
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int rowCount = Mathf.Min(columns, rectChildren.Count - (i / columns) * columns);
            float left = (rectTransform.rect.width - rowCount * width - (rowCount - 1) * spacing) * .5f;
            SetChildAlongAxis(rectChildren[i], 0, left + (i % columns) * (width + spacing), width);
            var label = rectChildren[i].GetComponentInChildren<Text>();
            if (label != null)
            {
                // The paper's torn edges scale with its width, so its text inset must too.
                float inset = Mathf.Max(55, width * .16f);
                label.rectTransform.offsetMin = new Vector2(inset, 20);
                label.rectTransform.offsetMax = new Vector2(-inset, -20);
            }
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        rowHeights.Clear();
        rowsHeight = 0;
        float minimumHeight = rectChildren.Count == 1 ? 220 : rectChildren.Count == 2 ? 190 : cardHeight;
        for (int i = 0; i < rectChildren.Count; i += columns)
        {
            float height = minimumHeight;
            for (int j = i; j < Mathf.Min(i + columns, rectChildren.Count); j++)
            {
                var label = rectChildren[j].GetComponentInChildren<Text>();
                if (label != null) height = Mathf.Max(height, label.preferredHeight + 56);
            }
            rowHeights.Add(height);
            rowsHeight += height;
        }
        rowsHeight += Mathf.Max(0, rowHeights.Count - 1) * spacing;
        SetLayoutInputForAxis(Mathf.Max(ViewportHeight, rowsHeight), Mathf.Max(ViewportHeight, rowsHeight), 0, 1);
        if (browseHint != null)
            browseHint.text = rectChildren.Count == 0 ? "NO MODIFIERS AVAILABLE" : rowsHeight > ViewportHeight + 1
                ? "SWIPE TO SEE MORE" : rectChildren.Count + (rectChildren.Count == 1 ? " MODIFIER AVAILABLE" : " MODIFIERS AVAILABLE");
    }

    public override void SetLayoutVertical()
    {
        float top = Mathf.Max(0, (ViewportHeight - rowsHeight) * .5f);
        for (int row = 0; row < rowHeights.Count; row++)
        {
            for (int i = row * columns; i < Mathf.Min((row + 1) * columns, rectChildren.Count); i++)
                SetChildAlongAxis(rectChildren[i], 1, top, rowHeights[row]);
            top += rowHeights[row] + spacing;
        }
    }
}
