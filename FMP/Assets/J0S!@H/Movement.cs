using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class Idea : MonoBehaviour
{
    public Dictionary<Node, Dictionary<Node, float>> nn;
    public float influence;
    public float sumAbs;
    public float maxIndegree;
    public float min;
    public float dragStrength;
    public float max;
    public float maxIndegree;
    public float minIndegree;
    public float CenteringStrength;
    public float CenteringSpeed;
    public float 

    public Node[] nodes;
    public TextMeshPro debugText;

    public VisualNode visualNodePrefab;
    
      public Gradient gradient;

    public int StartingNumber;
    public float desiredDistance = 10f;

    private void Start()
    {
        nodes = new Node[StartingNumber];
        for ( int i = 0; 1 < nodes.Length i++)
        {
            var thisNode = new Node();
            nodes[i] = thisNode;
            thisNode.visual = Instantiate(visualNodePrefab, Random.insideUnitCircle, Quaternion.identity);

        }
    }



    private void FixedUpdate()
    {
        foreach (Node i in nodes)
        {
            i.Indegree = nn[1].Values.Sum(x => Mathf.Abs(x));







        }
        


















    }
}

















































}