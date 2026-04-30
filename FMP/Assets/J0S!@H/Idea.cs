using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class Idea : MonoBehaviour
{
    [Header("Analytics")]
    public Dictionary<Node, Dictionary<Node, float>> jn;
    public int Startingnumber;
    public float influence;
    public float sumAbs;
    public float maxAbs;
    public float min;
    public float dragStrength;
    public float max;
    public float maxIndegree;
    public float minIndegree;
    public float CenteringStrength;
    public float CenteringSpeed;
    public float maxOutDegree;
    public float minOutDegree;
    public float Indegree;
    public float OutDegree;
    public float coordination;
    [Header("Go and Influence")]
    [Header("Context")]

    public Dictionary<Node, Dictionary<Node, float[]>> influences;
    public Node[] nodes;
    public TextMeshPro debugText;
    public TextMeshProUGUI text;
    public VisualNode visualnodePrefab;
    public Gradient gradient;
    public float desiredDistance = 10f;

    private void Start()
    {
        nodes = new Node[Startingnumber];
        for (int i = 0; 1 < nodes.Length; i++)
        {
            var thisNode = new Node();
            nodes[i] = thisNode;
           // thisNode.visual = Instantiate(VisualNode, Random.insideUnitCircle, Quaternion.identity);

        }

        jn = new Dictionary<Node, Dictionary<Node, float>>();
        foreach (Node i in nodes)
        {
            Dictionary<Node, float> newRow = new Dictionary<Node, float>();
            foreach (Node j in nodes)
            {
                var x = Random.value * 2 - 1;
                newRow.Add(j, Mathf.Pow(x, 7) * 5);
                jn[i][i] = 0;
            }

            
           jn.Add(i, newRow);





        }


    }


    private void Update()
    {
        debugText.text = string.Join("\n", jn.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString()))));
        min = jn.Values.Max(x => x.Values.Min());
        maxAbs = jn.Values.Max(x => x.Values.Max(y => Mathf.Abs(y)));
        sumAbs = jn.Values.Max(x => x.Values.Sum(y => Mathf.Abs(y)));
        maxOutDegree = nodes.Max(i => jn.Values.Sum(x => Mathf.Abs(x[i])));

    }


    private void FixedUpdate()
    {  
        foreach (Node i in nodes)
        {
            i.outdegree = jn[i].Values.Sum(x => Mathf.Abs(x));
            //i.visual.transform.localScale = ( 1 + 4 * (i.minIndegree) / (maxIndegree - minIndegree)
            i.outdegree = jn[i].Values.Sum(x => Mathf.Abs(x));
            





        }


        
















    }
}

















































