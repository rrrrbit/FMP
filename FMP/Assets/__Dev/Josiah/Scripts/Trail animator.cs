using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class Trailanimator : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float circleRadius = 2.5f;
    public Color StartColor;
    public Color endColor;
    private TrailRenderer trail;
    private float angle = 0f;
    private float VFX;
    public bool emitting;


    void Start()
    {
        trail = GetComponent<TrailRenderer>();

        trail.time = 1.5f;
        trail.startWidth = 0.5f;
        trail.endWidth = 0f;


        Gradient gradient = new Gradient();
        
      
    }

  
    void Update()
    {
        angle += moveSpeed * Time.deltaTime; 
        float x = math.cos(angle) * circleRadius;
        float z = Mathf.Sin(angle) * circleRadius;
        transform.position = new Vector3(x, 0, z);
    }
}
