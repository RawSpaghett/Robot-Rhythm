using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class RollingNumber : MonoBehaviour
{
    [SerializeField] private bool percentage;
    [SerializeField, Range(1, 4)] private int fullTurns = 2;
    [SerializeField, Range(0f, .05f)] private float digitDelay = .035f;
    private Text label;
    private RectTransform row;
    private readonly List<Text> currentDigits = new List<Text>();
    private readonly List<Text> nextDigits = new List<Text>();
    private string targetText;
    private float digitHeight;

    public void SetProgress(int value, float progress)
    {
        if (label == null) label = GetComponent<Text>();
        string text = value.ToString("N0", CultureInfo.InvariantCulture) + (percentage ? "%" : "");
        if (row == null || targetText != text) BuildDigits(text);
        row.gameObject.SetActive(true);
        label.text = text;
        label.enabled = false;
        for (int i = 0; i < text.Length; i++)
        {
            currentDigits[i].color = nextDigits[i].color = label.color;
            if (!char.IsDigit(text[i])) continue;
            int finalDigit = text[i] - '0';
            float delay = Mathf.Min(i * digitDelay, .8f);
            float travel = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - delay) / (1f - delay)));
            float number = (fullTurns * 10 + finalDigit) * travel;
            int whole = Mathf.FloorToInt(number);
            float offset = (number - whole) * digitHeight;
            currentDigits[i].text = (whole % 10).ToString();
            nextDigits[i].text = ((whole + 1) % 10).ToString();
            currentDigits[i].rectTransform.anchoredPosition = Vector2.up * offset;
            nextDigits[i].rectTransform.anchoredPosition = Vector2.up * (offset - digitHeight);
        }
    }

    public void Clear()
    {
        if (row != null) row.gameObject.SetActive(false);
        if (label == null) label = GetComponent<Text>();
        label.enabled = true;
    }

    private void BuildDigits(string text)
    {
        if (row != null) Destroy(row.gameObject);
        currentDigits.Clear();
        nextDigits.Clear();
        targetText = text;
        row = new GameObject("Rolling digits", typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(transform, false);
        row.anchorMin = Vector2.zero;
        row.anchorMax = Vector2.one;
        row.offsetMin = row.offsetMax = Vector2.zero;
        float width = label.rectTransform.rect.width;
        digitHeight = label.rectTransform.rect.height;
        int size = Mathf.Min(label.fontSize, Mathf.FloorToInt(width / (text.Length * .65f)));
        float totalWidth = 0f;
        foreach (char character in text) totalWidth += size * (character == '%' ? .95f : .64f);
        float cursor = -totalWidth * .5f;
        for (int i = 0; i < text.Length; i++)
        {
            float cellWidth = size * (text[i] == '%' ? .95f : .64f);
            var cell = new GameObject("Digit " + i, typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
            cell.SetParent(row, false);
            cell.anchorMin = cell.anchorMax = cell.pivot = new Vector2(.5f, .5f);
            cell.sizeDelta = new Vector2(cellWidth, digitHeight);
            cell.anchoredPosition = new Vector2(cursor + cellWidth * .5f, 0f);
            cursor += cellWidth;
            currentDigits.Add(CreateDigit(cell, size, text[i].ToString()));
            nextDigits.Add(CreateDigit(cell, size, char.IsDigit(text[i]) ? "0" : ""));
        }
    }

    private Text CreateDigit(RectTransform parent, int size, string text)
    {
        var digit = new GameObject("Face", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
        digit.transform.SetParent(parent, false);
        digit.rectTransform.sizeDelta = parent.sizeDelta;
        digit.font = label.font;
        digit.fontSize = size;
        digit.fontStyle = label.fontStyle;
        digit.color = label.color;
        var finish = label.GetComponent<NumberFinish>();
        if (finish != null)
            digit.gameObject.AddComponent<NumberFinish>().CopyStyleFrom(finish);
        digit.alignment = TextAnchor.MiddleCenter;
        digit.raycastTarget = false;
        digit.text = text;
        return digit;
    }
}
