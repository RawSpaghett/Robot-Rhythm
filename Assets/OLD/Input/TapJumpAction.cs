using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TapJumpAction : ButtonActionBase
{
    [Header("Normal Jump Request")]
    [SerializeField] private UnityEvent onJumpRequested = new UnityEvent();

    public override bool Matches(GestureData gesture) => gesture.IsTap;

    public override void Execute(GestureData gesture)
    {
        onJumpRequested.Invoke();
    }
}
