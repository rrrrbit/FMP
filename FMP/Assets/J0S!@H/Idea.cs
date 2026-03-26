using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;


public class Idea : MonoBehaviour
{
    [Header("Analytics")]
    public Dictionary<Node, Dictionary<Node, float>> nn;
    public float influence;
    public float sumAbs;
    public float min;
    public float dragStrength;
    public float max;
    public float maxIndegree;
    public float minIndegree;
    public float CenteringStrength;
    public float CenteringSpeed;
    public float maxOutDegree;
    public float indegree;
    [Header("Go and Influence")]

    public Dictionary<Node, Dictionary<Node, float[]>> influences;
    public Node[] nodes;
    public TextMeshPro debugText;
    public TextMeshProUGUI text;
    public VisualNode visualnodePrefab;
    public Gradient gradient;
    public float Indigree;

    public int StartingNumber;
    public float desiredDistance = 10f;

    public object DebugText { get; private set; }

    private void Start()
    {
        nodes = new Node[StartingNumber];
        for (int i = 0; 1 < nodes.Length; i++)
        {
            var thisNode = new Node();
            nodes[i] = thisNode;
            // thisNode.visual = Instantiate(VisualNode, Random.insideUnitCircle, Quaternion.identity);

        }
        
        nn = new Dictionary<Node, Dictionary<Node, float>>();

        foreach (Node i in nodes)
        {
            Dictionary<Node, float> newRow = new Dictionary<Node, float>();
            foreach (Node j in nodes)
            {
                float x = UnityEngine.Random.value * 2 - 1;
                newRow.Add(j, Mathf.Pow(x, 5));

            }


            nn.Add(i, newRow);
            //Using Add Method
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "One");


            
        }





    }












}




        
        












        






































































    







































    






























































































































