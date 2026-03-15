
using RBitUtils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class GameMaths : MonoBehaviour
{
	public int startingNumber;
	public float desiredDistance = 10f;
	public float centeringStrength;
	public float dragStrength;
	public AnimationCurve sizeOverIndegree;
	public Gradient edgeColourGradient;
	public float attractionForce = 2;
	public float repulsionForce = 100;
	public float clusterStrength = 1;

	[Space]
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxOutdegree;
	[Space]

	public Dictionary<Node, Dictionary<Node, float>> nn;
	public Node[] nodes;
	public TextMeshProUGUI debugText;
	public VisualNode visualNodePrefab;

	private void Start()
	{
		//initialise list of all nodes
		nodes = new Node[startingNumber];
		for (int i = 0; i < nodes.Length; i++)
		{
			Node thisNode = new Node();
			nodes[i] = thisNode;
			thisNode.visual = Instantiate(visualNodePrefab, Random.insideUnitCircle, Quaternion.identity);
			thisNode.visual.node = thisNode;
			thisNode.visual.gameMaths = this;
		}

		//initialise nn matrix with random weights
		nn = new Dictionary<Node, Dictionary<Node, float>>();
		foreach (Node i in nodes)
		{
			Dictionary<Node, float> newRow = new Dictionary<Node, float>();
			foreach (Node j in nodes)
			{
				float x = Random.value * 2 - 1;
				newRow.Add(j, Mathf.Pow(x, 11)*10);
			}
			nn.Add(i, newRow);
			nn[i][i] = 0;
		}
	}

	private void Update()
	{
		debugText.text = string.Join("\n", nn.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString()))));
		max = nn.Values.Max(x => x.Values.Max());
		min = nn.Values.Min(x => x.Values.Min());
		maxAbs = nn.Values.Max(x => x.Values.Max(y => Mathf.Abs(y)));
		sumAbs = nn.Values.Sum(x => x.Values.Sum(y => Mathf.Abs(y)));
		maxOutdegree = nodes.Max(i => nn.Values.Sum(x => Mathf.Abs(x[i])));
	}
	private void FixedUpdate()
	{
		foreach (Node i in nodes)
		{
			i.outdegree = nn.Values.Sum(x => Mathf.Abs(x[i]));
			i.visual.transform.localScale = sizeOverIndegree.Evaluate(i.outdegree / maxOutdegree) * Vector3.one;
			i.visual.outdegree = i.outdegree;
			i.visual.connections = nn[i].Values.ToList();

			Vector3 force = Vector3.zero;
			foreach (Node j in nodes)
			{
				force += PairwiseForce(i, j, desiredDistance);

				//debug edge visualisation
				float gap = 0.01f;
				Vector3 d = (j.visual.transform.position - i.visual.transform.position).normalized;
				Vector3 offs = new(d.y, -d.x, 0);
				Debug.DrawLine(i.visual.transform.position + offs * gap + d * i.visual.transform.localScale.x/2, j.visual.transform.position + offs * gap - d * j.visual.transform.localScale.x / 2, edgeColourGradient.Evaluate((nn[i][j] - min) / (max - min)));
			}
			force += NodewiseForce(i);

			if(!float.IsFinite(force.sqrMagnitude)) force = Vector3.zero;

			i.visual.v += force.ClampLength(1000) * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.v.sqrMagnitude)) i.visual.v = Vector3.zero;

			i.visual.transform.position += i.visual.v.ClampLength(1000) * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.transform.position.sqrMagnitude)) i.visual.transform.position = Vector3.zero;
		}
	}


	Vector3 PairwiseForce(Node i, Node j, float idealLength)
	{
		if (i == j) return Vector3.zero;

		float w = Mathf.Pow((Mathf.Abs(nn[i][j])) / 2 / maxAbs, 2);


		Vector3 dv = (j.visual.transform.position - i.visual.transform.position).normalized;
		float d = (j.visual.transform.position - i.visual.transform.position).magnitude;
		float contactDistance = (i.visual.transform.localScale.x + j.visual.transform.localScale.x) / 2;

		Vector3 attraction = (attractionForce * w * (d - contactDistance - idealLength)) * dv;
		Vector3 repulsion = repulsionForce / Mathf.Max(d - contactDistance, 0.01f) * -dv;

		return attraction + repulsion;
	}

	Vector3 NodewiseForce(Node i)
	{
		Vector3 globalCentroid = Vector3.zero;
		foreach (Node j in nodes)
		{
			globalCentroid += j.visual.transform.position / (nodes.Length - 1);
		}
		Vector3 centeringForce = centeringStrength * globalCentroid.sqrMagnitude * -globalCentroid.normalized;

		Vector3 weightedCentroid = Vector3.zero;
		foreach (Node j in nodes)
		{
			weightedCentroid += j.visual.transform.position * j.outdegree / nodes.Sum(x => x.outdegree);
		}
		Vector3 weightedCentroidD = (weightedCentroid - i.visual.transform.position);
		Vector3 clusterForce = clusterStrength * weightedCentroidD;

		Vector3 drag = dragStrength * i.visual.v.sqrMagnitude * -i.visual.v.normalized;
		return centeringForce + drag + clusterForce;
	}
}

[System.Serializable]
public class Node
{
	public VisualNode visual;
	public float outdegree;
}