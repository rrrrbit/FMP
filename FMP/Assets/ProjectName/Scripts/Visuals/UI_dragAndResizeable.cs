using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class UI_dragAndResizeable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public bool doing;
    public Vector2Int action;
    public RectTransform targetTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        print("test");
        doing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print("untest");
        doing = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!doing) return;
        if (action == Vector2Int.zero)
        {
            print(eventData.delta);
            targetTransform.localPosition += (Vector3)eventData.delta;
        }
    }
}
