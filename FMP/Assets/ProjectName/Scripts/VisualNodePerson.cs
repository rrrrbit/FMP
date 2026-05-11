using RBitUtils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static MGR_graphView;

public class VisualNodePerson : VisualNode
{
	[SerializeField] TextMeshPro text;
    [SerializeField] float[] niEdges;
    [SerializeField] float[] inEdges;
    [SerializeField] float[] nnEdgesTo;
    [SerializeField] MGR_gameMaths.NodeStats stats;
    [SerializeField] MGR_gameMaths.NodeStats dstats;
    [SerializeField] bool mouseOver;
    [SerializeField] bool dragging;
    [SerializeField] Vector2 toMouse;

    private void Start()
    {
        gameMaths = Managers.Get<MGR_gameMaths>();
    }

    void Update()
    {
        int strongestId = 0;
        float strongestWeight = 0;
        niEdges = gameMaths.NI.AllFrom(id);

		for (int i = 0; i < niEdges.Length; i++)
		{
            if (niEdges[i] > strongestWeight)
            {
                strongestId = i;
                strongestWeight = niEdges[i];
            }
		}

        text.text = strongestId.ToString();

		inEdges = gameMaths.IN.AllTo(id);
        nnEdgesTo = gameMaths.NN.AllTo(id);

        stats = gameMaths.nodeStats[id];
        dstats = gameMaths.nodeStatsDelta[id];

		transform.localScale = 2 * r * Vector3.one;
    }

	private void FixedUpdate()
	{
		//for (int i = 0; i < nodeCount; i++) // nodewise forces and some calcs
		//{
		//	vn[i].r = sizeByIndegree.Evaluate(graph.Indegree(i));
		//	vn[i].a = NodewiseForce(i);
		//}

		if (graphView.showNodes)
		{
			sr.enabled = true;
		}
		else
		{
			sr.enabled = false;
			return;
		}

		r = graphView.sizeByIndegree.Evaluate(gameMaths.NN.Indegree(id)) * (graphView.useScale ? 1 : 0);

		rb.AddForce(NodewiseForce(id));

		for (int j = 0; j < gameMaths.nodesCount; j++)
		{
			if (j == id) continue;
			VisualNodePerson other = graphView.visualNodes[j];

			Vector2 d = other.transform.position - transform.position;

			rb.AddForce(Repulsion(other, d, graphView.padding));

			if (graphView.applyNN)
			{
				float ij = gameMaths.NN[id, j];
				float ji = gameMaths.NN[j, id];

				float w;
				if (graphView.symmetriseWeights) w = (Mathf.Abs(ij) + Mathf.Abs(ji)) / 2;
				else w = Mathf.Abs(ij);
				if (graphView.normaliseWeights) w /= gameMaths.statsNN.maxAbs;

				if (w < graphView.pairwiseForceThreshold) continue;


				rb.AddForce(Attraction(other, d, w, graphView.padding));
			}
		}
	}

	Vector2 Attraction(VisualNodePerson other, Vector3 dv, float weight, float padding)
	{
		float radii = r + other.r;
		float d = Mathf.Max(dv.magnitude - padding - radii * (graphView.useScale ? 1 : 0), 0.01f);

		Vector2 force = RawAttraction(d, graphView.attractionType) * graphView.attractionStrength * graphView.attractionByWeight.Evaluate(weight) * dv.normalized;

		if (!float.IsFinite(force.sqrMagnitude))
		{
			print("Caught infinite pairwise force between " + id + " and " + other.id);
			force = force.WithMag(1000);
		}

		return force;
	}

	Vector2 Repulsion(VisualNodePerson other, Vector3 dv, float padding)
	{
		float radii = r + other.r;
		float d = Mathf.Max(dv.magnitude - padding - radii * (graphView.useScale ? 1 : 0), 0.01f);

		Vector2 force = -RawRepulsion(d, graphView.repulsionType) * graphView.repulsionStrength * dv.normalized;

		if (!float.IsFinite(force.sqrMagnitude))
		{
			print("Caught infinite pairwise force between " + id + " and " + other.id);
			force = force.WithMag(1000);
		}

		return force;
	}

	float RawAttraction(float d, AttractionTypes attractionType)
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

	float RawRepulsion(float d, RepulsionTypes repulsionType)
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
		Vector3 globalCentroid = Vector3.zero;
		Vector2 weightedCentroid = Vector3.zero;

		for (int j = 0; j < gameMaths.nodesCount; j++)
		{
			if (i == j) continue;
			globalCentroid += graphView.visualNodes[j].transform.position / (gameMaths.nodesCount - 1);
			//weightedCentroid += graphView.visualNodes[j].p * graph.Indegree(j) / gameMaths.statsNN.sumAbs;
		}
		Vector2 centeringForce = graphView.centeringStrength * transform.position.sqrMagnitude * -transform.position.normalized;
		//Vector2 weightedCentroidD = (weightedCentroid - graphView.visualNodes[i].p);
		//Vector2 clusterForce = graphView.clusterStrength * weightedCentroidD;

		//Vector2 drag = dragStrength * vn[i].v.sqrMagnitude * -vn[i].v.normalized;

		Vector2 totalForce = centeringForce;// + drag + clusterForce;



		if (!float.IsFinite(totalForce.sqrMagnitude))
		{
			print("Caught infinite nodewise force for node " + i);
			totalForce = totalForce.WithMag(1000);
		}

		return totalForce;
	}
}
