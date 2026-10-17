using System;
using System.Collections;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelCarousel : MonoBehaviour, IPointerDownHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Serializable]
    public class Level
    {
        public string title;
        public string scenePath;
        public Texture2D[] coverLayers;
        public AudioClip song;
    }

    [Header("Levels")]
    public Level[] levels;
    public UIManager ui;
    public UiController preferences;
    public MenuMusicPreview music;

    [Header("Card")]
    public RectTransform card;
    public CanvasGroup cardFade;
    public RawImage[] coverImages;
    public Text title;
    public Text pageNumber;
    public Button previousButton;
    public Button nextButton;
    [SerializeField, Min(20)] private float swipeDistance = 70;

    public int SelectedIndex { get; private set; }
    private Vector2 pointerStart;
    private bool dragged;
    private bool changing;
    private Vector2 cardPosition;

    private void Awake()
    {
        cardPosition = card.anchoredPosition;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        changing = false;
        card.anchoredPosition = cardPosition;
        cardFade.alpha = 1;
        music.RestoreMenuSong();
    }

    private void Refresh()
    {
        if (levels == null || levels.Length == 0) return;
        SelectedIndex = Mathf.Clamp(SelectedIndex, 0, levels.Length - 1);
        Level level = levels[SelectedIndex];
        for (int i = 0; i < coverImages.Length; i++)
        {
            bool visible = level.coverLayers != null && i < level.coverLayers.Length;
            coverImages[i].gameObject.SetActive(visible);
            if (visible) coverImages[i].texture = level.coverLayers[i];
        }
        title.text = level.title;
        pageNumber.text = (SelectedIndex + 1) + " / " + levels.Length;
        previousButton.gameObject.SetActive(SelectedIndex > 0);
        nextButton.gameObject.SetActive(SelectedIndex < levels.Length - 1);
        music.PreviewSong(level.song);
    }

    public void Previous() { ChangeLevel(-1); }
    public void Next() { ChangeLevel(1); }

    private void ChangeLevel(int direction)
    {
        if (changing || ui.IsTransitioning || levels == null) return;
        int next = SelectedIndex + direction;
        if (next < 0 || next >= levels.Length) return;
        if (direction > 0) preferences.PlayClick();
        else preferences.PlayBackClick();
        StartCoroutine(Slide(next, direction));
    }

    private IEnumerator Slide(int next, int direction)
    {
        changing = true;
        float duration = preferences.ReducedMotion ? 0 : .14f;
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0, 1, time / duration);
            card.anchoredPosition = cardPosition + Vector2.left * direction * 90 * progress;
            cardFade.alpha = 1 - progress;
            yield return null;
        }
        SelectedIndex = next;
        Refresh();
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0, 1, time / duration);
            card.anchoredPosition = cardPosition + Vector2.right * direction * 90 * (1 - progress);
            cardFade.alpha = progress;
            yield return null;
        }
        card.anchoredPosition = cardPosition;
        cardFade.alpha = 1;
        changing = false;
    }

    public void PlaySelected()
    {
        if (changing || ui.IsTransitioning || levels == null || levels.Length == 0) return;
        preferences.PlayClick();
        ui.StartLevel(levels[SelectedIndex].scenePath);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        pointerStart = eventData.position;
        dragged = false;
    }

    public void OnBeginDrag(PointerEventData eventData) { dragged = true; }
    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Vector2 delta = eventData.position - pointerStart;
        // Keep the gesture threshold consistent when the canvas scales for a phone.
        float scale = GetComponentInParent<Canvas>().scaleFactor;
        if (Mathf.Abs(delta.x) >= swipeDistance * scale && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            ChangeLevel(delta.x < 0 ? 1 : -1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !dragged)
            PlaySelected();
    }
}
