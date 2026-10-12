using UnityEngine;
using UnityEngine.EventSystems;
namespace RobotRhythm.UI
{
    public sealed class EffectsVolumeFeedback : MonoBehaviour, IPointerUpHandler, IMoveHandler
    {
        public UiController preferences;
        public void OnPointerUp(PointerEventData eventData) => preferences.PlayClick();
        public void OnMove(AxisEventData eventData) => preferences.PlayClick();
    }
}
