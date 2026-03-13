
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Utilities;

public class GameMaths : MonoBehaviour
{
    public Dictionary<Node, Dictionary<Node, float>> nn;
	public float centeringStrength;
	public float dragStrength;
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxIndegree;
	public float minIndegree;


	public Node[] nodes;
    public TextMeshProUGUI debugText;

	public VisualNode visualNodePrefab;

	public Gradient gradient;

	public int startingNumber;

	public float desiredDistance = 10f;
	private void Start()
    {
        //initialise list of all nodes
		nodes = new Node[startingNumber];
		for (int i = 0; i < nodes.Length; i++)
		{
			var thisNode = new Node();
			nodes[i] = thisNode;
			thisNode.visual = Instantiate(visualNodePrefab, Random.insideUnitCircle, Quaternion.identity);
		}

		//initialise nn matrix with random weights
		nn = new Dictionary<Node, Dictionary<Node, float>>();
		foreach (Node i in nodes)
        {
            Dictionary<Node, float> newRow = new Dictionary<Node, float>();
			foreach (Node j in nodes)
            {
				var x = Random.value * 2 - 1;
				newRow.Add(j, Mathf.Pow(x, 5));
			}

            nn.Add(i, newRow);
        }
    }

    private void FixedUpdate()
    {
		
		foreach (Node i in nodes)
		{
			i.indegree = nn[i].Values.Sum(x => Mathf.Abs(x));
			i.visual.transform.localScale = (1 + 4 * (i.indegree - minIndegree) / (maxIndegree - minIndegree)) * 0.25f * Vector3.one;
			i.visual.indegree = i.indegree;
			i.visual.connections = nn[i].Values.ToList();

			foreach (Node j in nodes)
			{
				AccumulateForces(i, j, desiredDistance/* * (1 + 4 * (i.indegree - minIndegree) / (maxIndegree - minIndegree))*/, Time.fixedDeltaTime);

				float gap = 0.01f;

				Vector3 d = (j.visual.transform.position - i.visual.transform.position).normalized;
				Vector3 offs = new(d.y, -d.x, 0);

				

				Debug.DrawLine(i.visual.transform.position + offs * gap, j.visual.transform.position + offs * gap, gradient.Evaluate( (nn[i][j] - min) / (max - min) ));
			}
			if(i.visual.v.Equals(new(float.NaN, float.NaN, float.NaN))) i.visual.v = Vector3.zero;
			i.visual.transform.position += i.visual.v * Time.fixedDeltaTime;
		}
	}

	private void Update()
	{
		debugText.text = string.Join("\n", nn.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString()))));
		max = nn.Values.Max(x => x.Values.Max());
		min = nn.Values.Min(x => x.Values.Min());
		maxAbs = nn.Values.Max(x => x.Values.Max(y => Mathf.Abs(y)));
		sumAbs = nn.Values.Sum(x => x.Values.Sum(y => Mathf.Abs(y)));
		maxIndegree = nodes.Max(i => nn[i].Values.Sum(x => Mathf.Abs(x)));
		minIndegree = nodes.Min(i => nn[i].Values.Sum(x => Mathf.Abs(x)));
	}

	void AccumulateForces(Node i, Node j, float desiredDistance, float dt)
	{
		if(i == j) return;

		var symmetricWeight = Mathf.Abs(nn[i][j]) / maxAbs / 2;

		Vector3 d = j.visual.transform.position - i.visual.transform.position;

		Vector3 repulsion = desiredDistance * desiredDistance / (d.magnitude + 0.01f) * -d.normalized;
		Vector3 attraction = d.sqrMagnitude / desiredDistance * d.normalized * symmetricWeight;
		Vector3 p = -i.visual.transform.position;
		Vector3 centeringForce = centeringStrength * p.sqrMagnitude * p.normalized; // centering force
		Vector3 drag = dragStrength * -i.visual.v.normalized * i.visual.v.sqrMagnitude; // drag

		Vector3 force = 
			attraction + 
			repulsion + 
			centeringForce +
			drag
		;

		Vector3 newV = i.visual.v + force * dt;
		i.visual.v = Mathf.Min(newV.magnitude, 100) * newV.normalized;
		//i.visual.v += force * dt;

		
	}
}

[System.Serializable]
public class Node
{
	public VisualNode visual;
	public float indegree;
}