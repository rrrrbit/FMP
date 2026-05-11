using RBitUtils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
		if (graphView.showNodes)
		{
			sr.enabled = true;
		}
		else
		{
			sr.enabled = false;
			return;
		}

		Vector2 totalForce = Vector2.zero;

		r = graphView.sizeByIndegree.Evaluate(gameMaths.NN.Indegree(id)) * (graphView.useScale ? 1 : 0);

		totalForce += CenteringForce(graphView.centeringStrength);
		totalForce += DragForce(graphView.dragStrength);

		if (graphView.showNodes) totalForce += NodesForces();
		if (graphView.showIdeas) totalForce += IdeasForces();

		totalForce = totalForce.ClampLength(graphView.maxVel);
		rb.AddForce(totalForce);
	}

	Vector2 NodesForces()
	{
		Vector2 totalForce = Vector2.zero;
		for (int otherId = 0; otherId < gameMaths.nodesCount; otherId++)
        {
            if (otherId == id) continue;
            VisualNodePerson other = graphView.visualNodes[otherId];

            Vector2 d = other.transform.position - transform.position;

            totalForce += Repulsion(other, d, graphView.padding);

            if (graphView.applyNN)
            {
                float fromThis = gameMaths.NN[id, otherId];
                float toThis = gameMaths.NN[otherId, id];

                float w;
                if (graphView.symmetriseWeights) w = (Mathf.Abs(fromThis) + Mathf.Abs(toThis)) / 2;
                else w = Mathf.Abs(fromThis);
                if (graphView.normaliseWeights) w /= gameMaths.statsNN.maxAbs;

                if (w < graphView.pairwiseForceThreshold) continue;


                totalForce += Attraction(other, d, w, graphView.padding);
            }
        }
		return totalForce;
    }
	Vector2 IdeasForces()
	{
        Vector2 totalForce = Vector2.zero;
        for (int otherId = 0; otherId < gameMaths.ideasCount; otherId++)
        {
            if (otherId == id) continue;
            VisualNode other = graphView.visualIdeas[otherId];

            Vector2 d = other.transform.position - transform.position;

            totalForce += Repulsion(other, d, graphView.padding);

            if (graphView.applyNI)
            {
                float fromThis = gameMaths.NI[id, otherId];
                float toThis = gameMaths.IN[otherId, id];

                float w;
                if (graphView.symmetriseWeights) w = (Mathf.Abs(fromThis) + Mathf.Abs(toThis)) / 2;
                else w = Mathf.Abs(fromThis);



                if (graphView.normaliseWeights) w /= Mathf.Max(gameMaths.statsNI.maxAbs, gameMaths.statsIN.maxAbs);

                if (w < graphView.pairwiseForceThreshold) continue;


                totalForce += Attraction(other, d, w, graphView.padding);
            }
        }
		return totalForce;
    }
}
