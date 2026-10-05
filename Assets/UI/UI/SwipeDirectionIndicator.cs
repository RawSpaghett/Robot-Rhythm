using UnityEngine;

// Keep exactly one declaration of this enum in the project.
public enum SwipeDirection { None, Up, Down, Left, Right }

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SwipeDirectionIndicator : MonoBehaviour
{
    [SerializeField, Min(0f)] private float distanceFromButton = 150f;
    private RectTransform arrowRect;

    private void Awake() => arrowRect = GetComponent<RectTransform>();

    public void ShowDirection(SwipeDirection direction)
    {
        if (direction == SwipeDirection.None) { Hide(); return; }
        if (arrowRect == null) arrowRect = GetComponent<RectTransform>();

        Vector2 position;
        float rotation;
        switch (direction)
        {
            case SwipeDirection.Up:
                position = new Vector2(0f, distanceFromButton);
                rotation = 0f;
                break;
            case SwipeDirection.Down:
                position = new Vector2(0f, -distanceFromButton);
                rotation = 180f;
                break;
            case SwipeDirection.Right:
                position = new Vector2(distanceFromButton, 0f);
                rotation = -90f;
                break;
            case SwipeDirection.Left:
                position = new Vector2(-distanceFromButton, 0f);
                rotation = 90f;
                break;
            default:
                Hide();
                return;
        }
        arrowRect.anchoredPosition3D = new Vector3(position.x, position.y, 0f);
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
