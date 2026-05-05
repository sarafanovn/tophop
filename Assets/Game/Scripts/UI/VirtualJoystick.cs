using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField] private float         maxRadius = 60f;

    public Vector2 Direction { get; private set; }

    private RectTransform _bg;
    private Vector2       _originPos;
    private Canvas        _canvas;

    private void Awake()
    {
        _bg     = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData e)
    {
        _originPos = ScreenToLocal(e.position);
        MoveHandle(e.position);
    }

    public void OnDrag(PointerEventData e) => MoveHandle(e.position);

    public void OnPointerUp(PointerEventData e)
    {
        Direction                = Vector2.zero;
        handle.anchoredPosition  = Vector2.zero;
    }

    private void MoveHandle(Vector2 screenPos)
    {
        Vector2 local   = ScreenToLocal(screenPos);
        Vector2 delta   = local - _originPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, maxRadius);

        handle.anchoredPosition = clamped;
        Direction = clamped / maxRadius;
    }

    private Vector2 ScreenToLocal(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _bg, screenPos, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out Vector2 local);
        return local;
    }
}
