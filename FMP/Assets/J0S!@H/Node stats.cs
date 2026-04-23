using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Analytics;
public class Nodestats : MonoBehaviour
{
    float Influence_time = 55f;
    float Consolidation = 33f;
    [SerializeField] Node Amplifier;
    [SerializeField] Node stats;
    [SerializeField] Node Node;
    [SerializeField] Node statistics;
    [SerializeField] Node experimentation;
    
    public class Stats
    {
        public int stat1;
        public int stat2;
        public int stat3;
        public int stat4;
        public int stat5;
        Vector2 Influence;
        Vector2 Consolidate;
        Vector2 Correlation;
        Vector2 Node_size;
        Vector2 Chamar;


    }

    public bool isNodeAnomalyDetected;

    private void Start()
    {
        while (Consolidation > 0)
        {
            Debug.Log("Need more consolidation");
            Consolidation--;

        }
    }

   
    public enum Stat
    {
        Physics2D,
        Clarification,
        Consildation,
        NodeAmplifier,
        Influences,
        maxIndegree,
        minIndegree,
        Outdegree,

    }

    public void cancellation()
    {



    }

    
    public void Node_Sound_Amplitude()
    {


    }

    public void LIGHT_YAGAMI()
    {



    }

    public void Node_Amplifier()
    {


    }

    public void Chamar()
    {
        if (isNodeAnomalyDetected)
        {
            print("Node Error");

        }
        else if (isNodeAnomalyDetected)
        {
            print("Node Unknown");
        }
        else
        {
            print("Node is validated");

        }

    }

    public void JEM_THE_GOAT()
    {



    }

    public void ALEX_O()
    {



    }

    




























































}
