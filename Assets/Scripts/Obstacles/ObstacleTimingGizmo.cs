using UnityEngine;

// Add to the same root GameObject as a concrete obstacle component.
// This only displays timing. It does not evaluate input or change the schedule.
public class ObstacleTimingGizmo : MonoBehaviour
{
    [Tooltip("Found on this GameObject automatically when left empty.")]
    [SerializeField] private ObstacleBase obstacle;
    [Tooltip("Leave empty on prefabs to use RhythmManager.Instance at runtime.")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Min(0.1f)] private float width = 3f;
    [Tooltip("How early the moving dot starts crossing the bar.")]
    [SerializeField, Min(0.1f)] private float approachSeconds = 1f;

    private void Reset() { obstacle = GetComponent<ObstacleBase>(); }

    private void OnDrawGizmos()
    {
        if (!enabled) return;
        if (obstacle == null) obstacle = GetComponent<ObstacleBase>();
        if (obstacle == null) return;

        Vector3 center = transform.TransformPoint(localOffset);
        float lead = Mathf.Max(0.1f, Mathf.Max(approachSeconds, obstacle.EarlyWindow));
        float total = lead + obstacle.LateWindow;
        Vector3 left = center - Vector3.right * Mathf.Max(0.1f, width) * 0.5f;
        Vector3 right = center + Vector3.right * Mathf.Max(0.1f, width) * 0.5f;
        Vector3 target = Vector3.Lerp(left, right, lead / total);
        Vector3 early = Vector3.Lerp(left, right, (lead - obstacle.EarlyWindow) / total);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(left, right);
        Gizmos.color = Color.green;
        Gizmos.DrawCube((early + right) * 0.5f,
            new Vector3(Vector3.Distance(early, right), 0.07f, 0.02f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(target - Vector3.up * 0.2f, target + Vector3.up * 0.2f);

        string label = "Waiting for spawner";
        RhythmManager rhythm = rhythmManager != null ? rhythmManager : RhythmManager.Instance;
        if (Application.isPlaying && rhythm != null && rhythm.isPlaying &&
            (obstacle.IsActive || obstacle.IsResolved))
        {
            double remaining = obstacle.TargetTime - rhythm.SongTime;
            if (obstacle.IsResolved)
            {
                label = $"{obstacle.LastResult} | Accuracy {obstacle.LastAccuracy:P0}";
                Gizmos.color = obstacle.WasCleared ? Color.green : Color.red;
                Gizmos.DrawWireCube(center, new Vector3(Mathf.Max(0.1f, width), 0.4f, 0.02f));
            }
            else
            {
                float progress = Mathf.Clamp01((float)((lead - remaining) / total));
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(Vector3.Lerp(left, right, progress), 0.09f);
                label = $"Target {obstacle.TargetTime:F2}s | Input in {remaining:F2}s";
            }
        }

        // Excluding UnityEditor from builds keeps this safe in a runtime folder.
#if UNITY_EDITOR
        UnityEditor.Handles.Label(center + Vector3.up * 0.35f, label);
        UnityEditor.Handles.Label(target + Vector3.down * 0.3f, "INPUT");
#endif
    }
}
