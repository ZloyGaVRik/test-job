using UnityEngine;
using UnityEngine.EventSystems;

public class SwipePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _swipePanel;

    private Vector3 _startPosition;
    private float swipeThreshold = -30f;
    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = _swipePanel.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.delta.x;
        Vector2 newPos = _swipePanel.anchoredPosition;
        newPos.x += deltaX;
        newPos.x = Mathf.Clamp(newPos.x, -50, 0);
        _swipePanel.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_swipePanel.anchoredPosition.x < swipeThreshold)
            Open();
        else
            Close();
    }

    private void Open()
    {
        _swipePanel.anchoredPosition = new Vector2(-50, _swipePanel.anchoredPosition.y);
    }

    private void Close()
    {
        _swipePanel.anchoredPosition = new Vector2(0, _swipePanel.anchoredPosition.y);
    }
}
