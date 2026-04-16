using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Reflection.Emit;

public class Idea : MonoBehaviour
{
    [Header("Analytics")]
    public Dictionary<Node, Dictionary<Node, float>> jj;
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
    public float Implement;
    public float zenify;
    [Header("Go and Influence")]
    [Header("Implement node effects")]
    public float[,] formulate;

    public Dictionary<Node, Dictionary<Node, float[]>> influences;
    public Node[] nodes;
    public TextMeshPro debugText;
    public TextMeshProUGUI text;
    public VisualNode visualnodePrefab;
    public Gradient gradient;
    public float Indigree;

    public int StartingNumber;
    public float desiredDistance = 10f;
    private object sizebyIndegree;

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

        jj = new Dictionary<Node, Dictionary<Node, float>>();
        foreach (Node i in nodes)
        {
            Dictionary<Node, float> newRow = new Dictionary<Node, float>();
            foreach (Node j in nodes)
            {
                var x = Random.value * 2 - 1;
                newRow.Add(j, Mathf.Pow(x, 5));

            }


            jj.Add(i, newRow);





        }


    }

    private void Update()
    {
        //Unidentified// DebugText.text = string.Join("\n", jj.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString())));









    }

    private void FixedUpdate()
    {
        foreach (Node i in nodes)
        {
            i.outdegree = jj[i].Values.Sum(x => Mathf.Abs(x));
            //Unidentified//.visual.transform.localScale = sizebyIndegree.(i.outdegree) * Vector3.one;
            i.visual.outdegree = i.outdegree;
            i.visual.connections = jj[i].Values.ToList();
            //Ignore this// i.influence.connections = jj [i].Values.ToList();




        }













    }


    public void latharlatharising()
    {




    }

    public void sentinel()
    {
       



    }

    public void Node_positioning()
    {

    }











































}












