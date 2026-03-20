using RBitUtils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class MGR_graphView : MonoBehaviour, IGraphView
{
	[Header("Misc")]
	public float lineGap = 0;
	public AnimationCurve sizeByOutdegree;
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
	[Header("Runtime & Refs")]
	public AdjacencyDictionary graph;
	HashSet<VisualNode> visualNodes;
	public VisualNode visualNodePrefab;
	MGR_gameMaths gameMaths;
	public Dictionary<Node, float> outdegrees;
	public Dictionary<Node, Vector3> pos;

	private void Awake()
	{
		gameMaths = Managers.Get<MGR_gameMaths>();
		gameMaths.OnReadyForVisualisation += InstantiateVisualNodes;
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
		UpdateView(Time.fixedDeltaTime);
		
	}

	void InstantiateVisualNodes()
	{
		visualNodes = new HashSet<VisualNode>();
		graph = gameMaths.nn;
		foreach (Node i in graph.nodes)
		{
			i.visual = Instantiate(visualNodePrefab);
			i.visual.node = i;
			i.visual.transform.position = Random.insideUnitCircle * 1;
			print("created new visualNode");
		}
		outdegrees = new();
		visualNodes = graph.nodes.Select(x => x.visual).ToHashSet();
	}

	void UpdateView(float dt)
	{
		float minWeight = graph.minWeight;
		float maxWeight = graph.maxWeight;
		float maxAbsWeight = graph.maxAbsWeight;
		float sumAbsWeight = graph.sumAbsWeight;

		outdegrees = graph.nodes.ToDictionary(x => x, x => graph.GetOutdegree(x));
		pos = graph.nodes.ToDictionary(x => x, x => x.visual.transform.position);
		foreach (VisualNode i in visualNodes) // calculate forces
		{
			i.radius = sizeByOutdegree.Evaluate(outdegrees[i]);

			i.a = Vector3.zero;

			foreach (VisualNode j in visualNodes)
			{
				float ij = graph.dict[i][j];
				float ji = graph.dict[j][i];
				Vector3 d = (pos[j] - pos[i]);
				Vector3 dn = d.normalized;

				if (i == j) continue;
				i.a += PairwiseForce(i, j, d, ij, ji, padding, maxAbsWeight);

				//debug edge visualisation
				Vector3 offs = new(dn.y, -dn.x, 0);
				Debug.DrawLine(i.transform.position + offs * lineGap + dn * i.radius, pos[j] + offs * lineGap - dn * j.radius, edgeColourGradient.Evaluate((ij - minWeight) / (maxWeight - minWeight)));
			}

			i.a += NodewiseForce(i, sumAbsWeight);
		}

		foreach (VisualNode i in visualNodes)// integrate
		{
			if (!float.IsFinite(i.a.sqrMagnitude))
			{
				print("Caught infinite force");
				i.a = i.a.WithMag(1000);
			}

			i.v += i.a.ClampLength(1000) * dt;
			if (!float.IsFinite(i.v.sqrMagnitude))
			{
				print("Caught infinite velocity");
				i.v = i.v.WithMag(1000);
			}

			i.transform.position += i.v.ClampLength(1000) * dt;
			if (!float.IsFinite(i.transform.position.sqrMagnitude))
			{
				print("Caught infinite position");
				i.transform.position = Vector3.zero;
			}
		}
	}

	Vector3 PairwiseForce(VisualNode i, VisualNode j, Vector3 dv, float ij, float ji, float padding, float maxAbsWeight)
	{
		float w;
		
		if (symmetriseWeights)
		{
			w = (Mathf.Abs(ij) + Mathf.Abs(ji)) / 2;
		}
		else
		{
			w = Mathf.Abs(ij);
		}

		if (normaliseWeights)
		{
			w /= maxAbsWeight;
		}

		float radii = i.radius + j.radius;
		float d = Mathf.Max(dv.magnitude - padding - radii * (useScale ? 1 : 0), 0.01f);

		return (
			RawAttraction(d) * attractionStrength * attractionByWeight.Evaluate(w) -
			RawRepulsion(d) * repulsionStrength
			) * dv.normalized;
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

	Vector3 NodewiseForce(VisualNode i, float sumAbsWeight)
	{
		Vector3 globalCentroid = Vector3.zero;
		Vector3 weightedCentroid = Vector3.zero;
		foreach (VisualNode j in visualNodes)
		{
			if(i == j) continue;
			globalCentroid += j.transform.position / (graph.nodes.Count - 1);
			weightedCentroid += j.transform.position * outdegrees[j] / sumAbsWeight;
		}
		Vector3 centeringForce = centeringStrength * globalCentroid.sqrMagnitude * -globalCentroid.normalized;
		Vector3 weightedCentroidD = (weightedCentroid - i.transform.position);
		Vector3 clusterForce = clusterStrength * weightedCentroidD;

		Vector3 drag = dragStrength * i.v.sqrMagnitude * -i.v.normalized;
		return centeringForce + drag + clusterForce;
	}
}