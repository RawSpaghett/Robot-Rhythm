using UnityEngine;
using UnityEngine.EventSystems;

namespace RobotRhythm.Feature5
{
    public class SkinSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public SkinMenu menu;
        public float swipeDistance = 60;
        private Vector2 start;
        public void OnBeginDrag(PointerEventData data) { start = data.pressPosition; }
        public void OnDrag(PointerEventData data) { }
        public void OnEndDrag(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            Vector2 delta = data.position - start;
            float scale = GetComponentInParent<Canvas>().scaleFactor;
            if (Mathf.Abs(delta.x) < swipeDistance * scale || Mathf.Abs(delta.x) <= Mathf.Abs(delta.y)) return;
            if (delta.x > 0) menu.Next(); else menu.Previous();
        }
    }
}
