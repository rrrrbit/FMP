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
        if (graphView.showIdeas)
        {
            sr.enabled = true;
            text.renderer.enabled = true;
        }
        else
        {
            sr.enabled = false;
            text.renderer.enabled = false;
            return;
        }

        Vector2 totalForce = Vector2.zero;

		r = graphView.sizeByIndegree.Evaluate(gameMaths.NN.Indegree(id)) * (graphView.useScale ? 1 : 0);

		totalForce += CenteringForce(graphView.centeringStrength);
		totalForce += DragForce(graphView.dragStrength);

		if (graphView.showNodes) totalForce += NodesForces(graphView.applyNN, graphView.visualNodes, gameMaths.NN, gameMaths.NN);
		if (graphView.showIdeas) totalForce += NodesForces(graphView.applyNI, graphView.visualIdeas, gameMaths.NI, gameMaths.IN);

		totalForce = totalForce.ClampLength(graphView.maxVel);
		rb.AddForce(totalForce);
	}
}
