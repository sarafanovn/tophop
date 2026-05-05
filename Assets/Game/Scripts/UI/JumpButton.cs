using UnityEngine;
using UnityEngine.EventSystems;

public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _pressed;
    public bool  IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData e) { _pressed = true;  IsHeld = true; }
    public void OnPointerUp(PointerEventData e)   { IsHeld = false; }

    public bool ConsumePress()
    {
        if (!_pressed) return false;
        _pressed = false;
        return true;
    }
}
