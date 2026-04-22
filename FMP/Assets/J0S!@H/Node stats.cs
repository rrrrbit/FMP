using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class Nodestats : MonoBehaviour
{    
    [SerializeField] Node Amplifier;
    [SerializeField] Node stats;
    [SerializeField] Node Node;
    [SerializeField] Node Consolidation;
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

    public void Chamar()
    {


    }































































}
