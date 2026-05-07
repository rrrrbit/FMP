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
	public float minColourEdge = -10;
	public float maxColourEdge = 10;
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
	[SerializeField] int debug_calculatedPairs;
	[SerializeField] int totalPairs;
	public VisualNode visualNodePrefab;
	public int nodeCount;
	MGR_gameMaths gameMaths;
	AdjacencyMtx graph;
	VisualNode[] obj;

	public struct VisualNodeProperties
	{
		public Vector2 p, v, a;
		public float r;
	}
	public VisualNodeProperties[] vn;

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
		graph = gameMaths.NN;
		nodeCount = graph.nodes.Count;
		obj = new VisualNode[nodeCount];
		vn = new VisualNodeProperties[nodeCount];

        for (int i = 0; i < obj.Length; i++)
        {
			obj[i] = Instantiate(visualNodePrefab);
			obj[i].id = i;
			obj[i].gameObject.name = "Node "+obj[i].id.ToString();

			vn[i].p = Random.insideUnitCircle * 1;
			vn[i].v = Vector2.zero;
            vn[i].a = Vector2.zero;
			vn[i].r = sizeByIndegree.Evaluate(graph.GetIndegree(i));

        }

		totalPairs = nodeCount * nodeCount;
	}

	void UpdateView(float dt)
	{
		debug_calculatedPairs = 0;
		for (int i = 0; i < nodeCount; i++) // nodewise forces and some calcs
        {
            vn[i].r = sizeByIndegree.Evaluate(graph.GetIndegree(i));
            vn[i].a = NodewiseForce(i);
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

                Vector2 d = vn[j].p - vn[i].p;
                Vector2 dn = d.normalized;

                vn[i].a += PairwiseForce(i, j, d, w, padding);
				debug_calculatedPairs += 1;

                //debug edge visualisation
                Vector2 offs = new(dn.y, -dn.x);

				Vector2 startPoint = vn[i].p + offs * lineGap + dn * vn[i].r;
				Vector2 endPoint = vn[j].p + offs * lineGap - dn * vn[j].r;
				Debug.DrawLine(startPoint, endPoint, edgeColourGradient.Evaluate((ij - minColourEdge) / (maxColourEdge - minColourEdge)));
				Debug.DrawLine(endPoint, endPoint + (offs-dn), edgeColourGradient.Evaluate((ij - minColourEdge) / (maxColourEdge - minColourEdge)));
			}

		}
        for (int i = 0; i < nodeCount; i++) // integrate and update transform
        {
			Vector2 p = vn[i].p;
			Vector2 v = vn[i].v;
			Vector2 a = vn[i].a;

            if (!float.IsFinite(a.sqrMagnitude))
            {
                print("Caught infinite force");
                a = a.WithMag(1000);
            }

            v += a.ClampLength(1000) * dt;
            if (!float.IsFinite(v.sqrMagnitude))
            {
                print("Caught infinite velocity");
                v = v.WithMag(1000);
            }

			p += v.ClampLength(1000) * dt;
            if (!float.IsFinite(p.sqrMagnitude))
            {
                print("Caught infinite position");
                p = Vector3.zero;
            }

			obj[i].transform.position = p;
			obj[i].transform.localScale = vn[i].r * 2 * Vector3.one;

			vn[i].p = p;
			vn[i].v = v;
			vn[i].a = a;
        }
	}

	Vector2 PairwiseForce(int i, int j, Vector3 dv, float weight, float padding)
	{
		float radii = vn[i].r + vn[j].r;
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
			globalCentroid += vn[j].p / (graph.nodes.Count - 1);
			weightedCentroid += vn[j].p * graph.GetIndegree(j) / graph.sumAbsWeight;
        }
		Vector2 centeringForce = centeringStrength * globalCentroid.sqrMagnitude * -globalCentroid.normalized;
		Vector2 weightedCentroidD = (weightedCentroid - vn[i].p);
		Vector2 clusterForce = clusterStrength * weightedCentroidD;

		Vector2 drag = dragStrength * vn[i].v.sqrMagnitude * - vn[i].v.normalized;
		return centeringForce + drag + clusterForce;
	}
}