using UnityEngine;

// Inherit from this for each action; do not attach this abstract class.
public abstract class ButtonActionBase : MonoBehaviour
{
    public abstract bool Matches(GestureData gesture);
    public abstract void Execute(GestureData gesture);

    // Actions without charging keep this default implementation.
    public virtual bool TryGetCharge(GestureData gesture, out float charge)
    {
        charge = 0f;
        return false;
    }
}
