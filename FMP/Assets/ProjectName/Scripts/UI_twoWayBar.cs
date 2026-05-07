using UnityEngine;

[ExecuteAlways]
public class UI_twoWayBar : MonoBehaviour
{
    RectTransform fill;
    RectTransform parent;
    [Range(-1, 1)] public float value;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        parent = GetComponent<RectTransform>();   
    }

    // Update is called once per frame
    void Update()
    {
        fill = transform.GetChild(0).GetComponent<RectTransform>();
        if (value >= 0)
        {
            fill.offsetMin = new Vector2 (0,parent.sizeDelta.y/2);
            fill.offsetMax = new Vector2(0, -Mathf.Lerp(parent.sizeDelta.y / 2, 0, value));
        }
        else
        {
            fill.offsetMax = new Vector2(0, -parent.sizeDelta.y / 2);
            fill.offsetMin = new Vector2(0, Mathf.Lerp(parent.sizeDelta.y / 2, 0, - value));
        }
    }
}
