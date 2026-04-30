using UnityEngine;
using UnityEngine.Analytics;
using TMPro;
using NUnit.Framework;
 using System.Linq;
using UnityEngine.Rendering;
using NUnit.Framework.Constraints;
using UnityEngine.SceneManagement;
using System;




public class Fundamental : MonoBehaviour
{   
    public bool isNode;
    GameObject FundamentalNode;
    public bool NodeAmputation;

    private void Start()
    {
        if (FundamentalNode == null)
        {
            Debug.Log("Node is activated");
            isNode = true;
        }



    }

    [Header("Clarification")]
    [Header("Consolidation")]
    [Header("Amplifier")]
    public VisualNode[] FormulatedNode;
    public Vector3 Nodes;
    public Node Node;
    public float Outdegree;
    public float Indegree;
    public float Node_physics;

  

    public enum node_influence
    {
        

    }

    void Node_Amplifier()
    {
        if (NodeAmputation == true)
        {
            print("Amputation is working");
        }
        else
        {
            print("Amputation is not working");
        }
    }










































}





































































