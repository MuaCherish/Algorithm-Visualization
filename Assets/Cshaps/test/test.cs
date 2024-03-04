using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class test : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private bool isDragging = false;
    private Vector2 pointerOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Debug.Log(rectTransform.anchoredPosition);

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        pointerOffset = rectTransform.anchoredPosition - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition = eventData.position + pointerOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
