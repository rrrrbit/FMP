
using RBitUtils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class GameMaths : MonoBehaviour
{
	[Header("Misc")]
	public int startingNumber;
	public AnimationCurve sizeByIndegree;
	public Gradient edgeColourGradient;
	[Header("Forces")]
	public float padding = 10f;
	public bool useScale = true;
	public bool normaliseWeights = true;
	public bool symmetriseWeights = true;
	[Space]
	public float centeringStrength;
	public float dragStrength;
	public float clusterStrength = 1;
	[Space]
	public float attractionStrength = 2;
	public AnimationCurve attractionByWeight;
	public enum AttractionTypes 
	{ 
		Linear,
		Log,
		Quadratic, 
	}
	public AttractionTypes attractionType;
	[Space]
	public float repulsionStrength = 100;
	public enum RepulsionTypes 
	{
		Reciprocal,
		InverseSqr,
	}
	public RepulsionTypes repulsionType;

	[Header("Statistics")]
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxOutdegree;
	[Header("Runtime & Refs")]

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
		foreach (Node i in nodes)// calculate forces
		{
			i.outdegree = nn.Values.Sum(x => Mathf.Abs(x[i]));
			i.visual.transform.localScale = sizeByIndegree.Evaluate(i.outdegree) * Vector3.one;
			i.visual.outdegree = i.outdegree;
			i.visual.connections = nn[i].Values.ToList();

			i.visual.a = Vector3.zero;
			foreach (Node j in nodes)
			{
				i.visual.a += PairwiseForce(i, j, padding);

				//debug edge visualisation
				float gap = 0.01f;
				Vector3 d = (j.visual.transform.position - i.visual.transform.position).normalized;
				Vector3 offs = new(d.y, -d.x, 0);
				Debug.DrawLine(i.visual.transform.position + offs * gap + d * i.visual.transform.localScale.x/2, j.visual.transform.position + offs * gap - d * j.visual.transform.localScale.x / 2, edgeColourGradient.Evaluate((nn[i][j] - min) / (max - min)));
			}
			i.visual.a += NodewiseForce(i);
		}

		foreach (Node i in nodes)// integrate
		{
			if (!float.IsFinite(i.visual.a.sqrMagnitude))
			{
				print("Caught infinite force");
				i.visual.a = i.visual.a.WithMag(1000);
			}

			i.visual.v += i.visual.a.ClampLength(1000) * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.v.sqrMagnitude))
			{
				print("Caught infinite velocity");
				i.visual.v = i.visual.v.WithMag(1000);
			}

			i.visual.transform.position += i.visual.v.ClampLength(1000) * Time.fixedDeltaTime;
			if (!float.IsFinite(i.visual.transform.position.sqrMagnitude))
			{
				print("Caught infinite position");
				i.visual.transform.position = Vector3.zero;
			}
		}
	}


	Vector3 PairwiseForce(Node i, Node j, float padding)
	{
		if (i == j) return Vector3.zero;

		float w;
		if (symmetriseWeights)
		{
			w = (Mathf.Abs(nn[i][j]) + Mathf.Abs(nn[j][i])) / 2;
		}
		else
		{
			w = Mathf.Abs(nn[i][j]);
		}

		if(normaliseWeights)
		{
			w /= maxAbs;
		}

		Vector3 dv = (j.visual.transform.position - i.visual.transform.position).normalized;
		float distance = (j.visual.transform.position - i.visual.transform.position).magnitude;
		float radii = (i.visual.transform.localScale.x + j.visual.transform.localScale.x) / 2;
		float d = Mathf.Max(distance - padding - radii * (useScale ? 1 : 0), 0.01f);

		return (
			RawAttraction(d) * attractionStrength * attractionByWeight.Evaluate(w) - 
			RawRepulsion(d) * repulsionStrength
			) * dv;
	}

	float RawAttraction(float d)
	{
		switch (attractionType)
		{
			case AttractionTypes.Linear:
				return d;
			case AttractionTypes.Log:
				return Mathf.Log(d + 1);
			case AttractionTypes.Quadratic:
				return d * d;
			default:
				return 0;
		}
	}

	float RawRepulsion(float d)
	{
		switch (repulsionType)
		{
			case RepulsionTypes.Reciprocal:
				return 1 / d;
			case RepulsionTypes.InverseSqr:
				return 1 / (d * d);
			default:
				return 0;
		}
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