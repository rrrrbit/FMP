using System.Collections;
using UnityEngine;

public class Nodecounter : MonoBehaviour
{
    [Header("Node Counter system")]
    [Header("Time registrated")]
    [Header("Time managed")]
    public static int tarvel_time = 0;
    public int tarvel_speed = 0;
    public static int Node_travel_time;
    [SerializeField] Node time_capture;
    [SerializeField] Node analytics;
    [SerializeField] private GameObject Time_Manager;
    public bool Is_time_accurate;
    public bool isInitiated;
    public bool Is_time_set;
    public bool isInitiated_set;
    public bool planned_time;
    public bool planned_analytics;
    [SerializeField] Time instantiated;

    void Start()
    { 
        isInitiated = true;
        Is_time_set = false;
        Is_time_set = true;
        isInitiated_set = false;
        Invoke("Time activated", 0);
    }

   
    void Update()
    {
        
    }

    IEnumerator Time_accuracy()
    {
        yield return Time.deltaTime;
        Is_time_set &= true;
        Is_time_set |= true;
    
    }
    

}
