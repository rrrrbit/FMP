using RBitUtils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Visual_NodeSocial : Visual_Node
{
    [SerializeField] float[] niEdges;
    [SerializeField] float[] inEdges;
    [SerializeField] float[] nnEdgesTo;
    [SerializeField] MGR_mtx.NodeStats dstats; 

    void Update()
    {
        int strongestId = 0;
        float strongestWeight = 0;
        niEdges = MGR_game.mtx.NI.AllFrom(id);

		for (int i = 0; i < niEdges.Length; i++)
		{
            if (niEdges[i] > strongestWeight)
            {
                strongestId = i;
                strongestWeight = niEdges[i];
            }
		}

		inEdges = MGR_game.mtx.IN.AllTo(id);
        nnEdgesTo = MGR_game.mtx.NN.AllTo(id);

        dstats = MGR_game.mtx.nodeStatsDelta[id];
        sr.color = MGR_game.visuals.ideaColours[strongestId];
        transform.localScale = 2 * r * Vector3.one;
    }

    public override void Select()
    {
        MGR_game.levelUI.TryFocusNodeViewer(this);
    }

    private void FixedUpdate()
	{
        if (MGR_game.visuals.showNodes)
        {
            sr.enabled = true;
            rb.simulated = true;
        }
        else
        {
            sr.enabled = false;
            rb.simulated = false;
            return;
        }

        Vector2 totalForce = Vector2.zero;

		r = MGR_game.visuals.sizeByIndegree.Evaluate(MGR_game.mtx.NN.Indegree(id)) * (MGR_game.visuals.useScale ? 1 : 0);

		totalForce += CenteringForce(MGR_game.visuals.centeringStrength);
		totalForce += DragForce(MGR_game.visuals.dragStrength);

		if (MGR_game.visuals.showNodes) totalForce += NodesForces(MGR_game.visuals.applyNN, MGR_game.visuals.visualNodes, MGR_game.mtx.NN, MGR_game.mtx.NN);
		if (MGR_game.visuals.showIdeas) totalForce += NodesForces(MGR_game.visuals.applyNI, MGR_game.visuals.visualIdeas, MGR_game.mtx.NI, MGR_game.mtx.IN);

        if (dragging)
        {
            Vector2 dMouse = (MGR_game.input.worldPointer.pos - transform.position);
            totalForce += dMouse.sqrMagnitude * dMouse.normalized * MGR_game.visuals.mouseStrength;
        }

        totalForce = totalForce.ClampLength(MGR_game.visuals.maxVel);
		rb.AddForce(totalForce);
	}
}
