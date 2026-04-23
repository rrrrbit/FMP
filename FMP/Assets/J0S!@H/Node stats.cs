using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
public class Nodestats : MonoBehaviour
{    
    [SerializeField] Node Amplifier;
    [SerializeField] Node stats;
    [SerializeField] Node Node;
    [SerializeField] Node Consolidation;
    [SerializeField] Node statistics;
    [SerializeField] Node experimentation;
    private float elapsedTime = 0f;
    private float score = 0;
    private float scoreMultiplier = 10f;
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
        Vector3 Node_physics;
        Vector3 Node_velocity;


    }
    
    public bool isNodeAnomalyDetected;
    public bool Node_Capacity = true;

    void Update()
    {

        elapsedTime += Time.deltaTime;
        Debug.Log("Score: " + score);
        score = elapsedTime * scoreMultiplier;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier); 



    }
    public enum Statistics
    {
        Physics2D,
        Clarification,
        Consildation,
        NodeAmplifier,
        Influences,
        maxIndegree,
        minIndegree,
        Outdegree,
        Node_velocity,
        Node_physics,

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
        if (isNodeAnomalyDetected)
        {
            print("Node Error");

        }
        else if (isNodeAnomalyDetected)
        {
            print("Node is unknown");
        }
        else
        {
            print("Node is validated");

        }

    }

    public void HEISENBERG()
    {
        if (Node_Capacity == true)
            print("Size is adequate");
        else if (Node_Capacity == false)
        {
            print("Size is inadequate");
        }

    }





























































}
