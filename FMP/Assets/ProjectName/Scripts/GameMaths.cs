
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
			Node thisNode = new Node();
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
				float x = Random.value * 2 - 1;
				newRow.Add(j, Mathf.Pow(x, 7)*10);
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

			Vector3 force = Vector3.zero;
			foreach (Node j in nodes)
			{
				force += PairwiseForce(i, j, desiredDistance + i.visual.transform.localScale.x - 0.25f);

				//debug edge visualisation
				float gap = 0.01f;
				Vector3 d = (j.visual.transform.position - i.visual.transform.position).normalized;
				Vector3 offs = new(d.y, -d.x, 0);
				Debug.DrawLine(i.visual.transform.position + offs * gap, j.visual.transform.position + offs * gap, gradient.Evaluate((nn[i][j] - min) / (max - min)));
			}
			force += NodewiseForce(i);

			if(!float.IsFinite(force.sqrMagnitude)) force = Vector3.zero;

			i.visual.v += force * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.v.sqrMagnitude)) i.visual.v = Vector3.zero;

			i.visual.transform.position += i.visual.v * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.transform.position.sqrMagnitude)) i.visual.transform.position = Vector3.zero;
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

	Vector3 PairwiseForce(Node i, Node j, float desiredDistance)
	{
		if (i == j) return Vector3.zero;

		float symmetricWeight = Mathf.Abs(nn[i][j]) / maxAbs / 2;

		Vector3 d = j.visual.transform.position - i.visual.transform.position;

		Vector3 repulsion = desiredDistance * desiredDistance / (d.magnitude + 0.01f) * -d.normalized;
		Vector3 attraction = d.sqrMagnitude / desiredDistance * d.normalized * symmetricWeight;

		return attraction + repulsion;
	}

	Vector3 NodewiseForce(Node i)
	{
		Vector3 p = -i.visual.transform.position;
		Vector3 centeringForce = centeringStrength * p.sqrMagnitude * p.normalized;
		Vector3 drag = dragStrength * -i.visual.v.normalized * i.visual.v.sqrMagnitude;
		return centeringForce + drag;
	}
}

[System.Serializable]
public class Node
{
	public VisualNode visual;
	public float indegree;
}