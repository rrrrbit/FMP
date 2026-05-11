using RBitUtils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VisualNodeIdea : VisualNode
{
    public TextMeshPro text;

    void Update()
    {
        text.text = id.ToString();

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

        if (graphView.showNodes) totalForce += NodesForces(graphView.applyIN, graphView.visualNodes, gameMaths.IN, gameMaths.NI);
        if (graphView.showIdeas) totalForce += NodesForces(graphView.applyII, graphView.visualIdeas, gameMaths.II, gameMaths.II);

        totalForce = totalForce.ClampLength(graphView.maxVel);
        rb.AddForce(totalForce);
    }
}
