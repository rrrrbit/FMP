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
    [Header("Node Analytics")]
    // This is just a title for the Node data
    [SerializeField] Node Amplifier;
    [SerializeField] Node stats;
    [SerializeField] Node Node;
    [SerializeField] Node Consolidation;
    [SerializeField] Node statistics;
    [SerializeField] Node experimentation;
    private float elapsedTime = 0f;
    private float score = 0;
    // It can also be a public float but I prefer to keep this confidential
    private float scoreMultiplier = 10f;
     // Your score will multiply if you continue to influence other nodes

    private bool Light_Yagami_is_Kira;
    // This is to detect if the code implemented will reiterate
   
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
    public bool Node_disc = false;
    public bool Node_demonstration = true;
    void Update()
    {

        elapsedTime += Time.deltaTime;
        Debug.Log("Score: " + score);
        score = elapsedTime * scoreMultiplier;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier); 
        // This is me inputting a score system



    }
    public enum Statistics
    {
        Physics2D,
        // Physics2D is a specialized simulation engine within game development (specifically Unity) used to managed two-dimensional Physical interactions. There's more to explain, but it will overcomplicate things
        Clarification,
        // To give the node an understanding on how it's supposed to operate 
        Consildation,
        // More nodes will unite and conquer
        NodeAmplifier,
        // Node strength will increase if they either acquire a power-up or if they influence other nodes
        Influences,
        // Used to make sure if there are positve/negative influences which either attract or repel
        maxIndegree,
        minIndegree,
        Outdegree,
        Node_velocity,
        Node_physics,
        // How fast/slow paced the node will move when linked to other nodes
        Node_Analytics,
        // This is only for data purposes
    }


    public void cancellation()
    {
        do
        {


        } while (enabled);
        // I need more explained in this bit of code, but it explains how nodes will repel each other if they share similarities


    }

    
    public void Node_Sound_Amplitude()
    {
        Vector3[] targets = { Vector3.zero, Vector2.one, Vector3.up };
        // This is for node sound volume and for how it travels
        foreach (Vector3 target in targets)
        {
            //Input specific iteration 
            // Still me experimenting
        }
    }

    public void LIGHT_YAGAMI()
    {
        while (gameObject.activeSelf)
        {
            // Code will continue to reiterate
        }


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
        // These bits of code will ingrain this into the node which could improve their energy levels
    }

    public void HEISENBERG()
    {
        if (Node_Capacity == true)
            print("Size is adequate");
        else if (Node_Capacity == false)
        {
            print("Size is inadequate");
        }
        else
        {
            print("Size is overextending");

        } // There is a limit depending on what size the node reaches


    }

    private void Node_disc_congregation()
    {
        if (Node_disc == true)
        {
            print("Disc is working");


        }
        else if (Node_disc == false)
        {
            print("Disc is not working");
        }
        // This is just me experimenting
    }


    public void Node_visuals()
    {
        if (Node_demonstration == true)
            print("Demonstration is accurate");
        else if (Node_demonstration == false)
            print("Demonstration is inaccurate");
        else
        {
            print("Demonstration is moderate");
        }
       // This is how I will 
    }
    


























































}
