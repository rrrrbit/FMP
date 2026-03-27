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
	public float pairwiseForceThreshold = 0.01f;
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
	[Header("Runtime & Refs")]
	[SerializeField] int calculatedPairs;
	[SerializeField] int totalPairs;
	public VisualNode visualNodePrefab;
	public int nodeCount;
	MGR_gameMaths gameMaths;
	AdjacencyMtx graph;
	VisualNode[] obj;
	Vector2[] p, v, a;
	float[] r;

	private void Awake()
	{
		gameMaths = Managers.Get<MGR_gameMaths>();
		gameMaths.OnReadyForVisualisation += Init;
	}

	private void Update()
	{
        
    }

	private void FixedUpdate()
	{

        UpdateView(Time.fixedDeltaTime);
    }

	void Init()
	{
		graph = gameMaths.nnMtx;
		nodeCount = graph.nodes.Count;
		obj = new VisualNode[nodeCount];
		p = new Vector2[nodeCount];
		v = new Vector2[nodeCount];
		a = new Vector2[nodeCount];
		r = new float[nodeCount];

        for (int i = 0; i < obj.Length; i++)
        {
			obj[i] = Instantiate(visualNodePrefab);
			obj[i].id = i;
			p[i] = Random.insideUnitCircle * 1;
			v[i] = Vector2.zero;
			a[i] = Vector2.zero;
			r[i] = sizeByIndegree.Evaluate(graph.GetIndegree(i));
        }

		totalPairs = nodeCount * nodeCount;
	}

	void UpdateView(float dt)
	{
		calculatedPairs = 0;
		for (int i = 0; i < nodeCount; i++) // nodewise forces and some calcs
        {
            r[i] = sizeByIndegree.Evaluate(graph.GetIndegree(i));
            a[i] = Vector3.zero;
            a[i] += NodewiseForce(i);
        }

        for (int i = 0; i < graph.mtx.Rows(); i++) // pairwise forces
        {
			
            for (int j = 0; j < graph.mtx.Cols(); j++)
            {
                if (i == j) continue;
                float ij = graph.mtx[i, j];
                float ji = graph.mtx[j, i];

                float w;
                if (symmetriseWeights) w = (Mathf.Abs(ij) + Mathf.Abs(ji)) / 2;
                else w = Mathf.Abs(ij);
                if (normaliseWeights) w /= graph.maxAbsWeight;

				if (w < pairwiseForceThreshold) continue;

                Vector2 d = p[j] - p[i];
                Vector2 dn = d.normalized;

                a[i] += PairwiseForce(i, j, d, w, padding);
				calculatedPairs += 1;

                //debug edge visualisation
                Vector2 offs = new(dn.y, -dn.x);
				Debug.DrawLine(p[i] + offs * lineGap + dn * r[i], p[j] + offs * lineGap - dn * r[j], edgeColourGradient.Evaluate((ij - graph.minWeight) / (graph.maxWeight - graph.minWeight)));
            }
			
        }
        for (int i = 0; i < nodeCount; i++) // integrate
        {
            if (!float.IsFinite(a[i].sqrMagnitude))
            {
                print("Caught infinite force");
                a[i] = a[i].WithMag(1000);
            }

            v[i] += a[i].ClampLength(1000) * dt;
            if (!float.IsFinite(v[i].sqrMagnitude))
            {
                print("Caught infinite velocity");
                v[i] = v[i].WithMag(1000);
            }

			p[i] += v[i].ClampLength(1000) * dt;
            if (!float.IsFinite(p[i].sqrMagnitude))
            {
                print("Caught infinite position");
                p[i] = Vector3.zero;
            }

			obj[i].transform.position = p[i];
			obj[i].transform.localScale = r[i] * 2 * Vector3.one;
        }
	}

	Vector2 PairwiseForce(int i, int j, Vector3 dv, float weight, float padding)
	{
		float radii = r[i] + r[j];
		float d = Mathf.Max(dv.magnitude - padding - radii * (useScale ? 1 : 0), 0.01f);

		return (
			RawAttraction(d) * attractionStrength * attractionByWeight.Evaluate(weight) -
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

	Vector2 NodewiseForce(int i)
	{
		Vector2 globalCentroid = Vector3.zero;
		Vector2 weightedCentroid = Vector3.zero;

        for (int j = 0; j < nodeCount; j++)
        {
			if(i == j) continue;
			globalCentroid += p[j] / (graph.nodes.Count - 1);
			weightedCentroid += p[j] * graph.GetIndegree(j) / graph.sumAbsWeight;
        }
		Vector2 centeringForce = centeringStrength * globalCentroid.sqrMagnitude * -globalCentroid.normalized;
		Vector2 weightedCentroidD = (weightedCentroid - p[i]);
		Vector2 clusterForce = clusterStrength * weightedCentroidD;

		Vector2 drag = dragStrength * v[i].sqrMagnitude * -v[i].normalized;
		return centeringForce + drag + clusterForce;
	}
}