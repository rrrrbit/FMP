using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UI_eventSystem : MonoBehaviour, IPointerMoveHandler
{
    [SerializeField] EventSystem es;

    [SerializeField] List<RaycastResult> results;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerMove(PointerEventData pointerEventData)
    {
        print("test");
    }

    
}
