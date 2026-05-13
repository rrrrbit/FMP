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
        if (MGR_game.visuals.showIdeas)
        {
            sr.enabled = true;
            text.renderer.enabled = true;
            rb.simulated = true;
        }
        else
        {
            sr.enabled = false;
            text.renderer.enabled = false;
            rb.simulated = false;
            return;
        }

        Vector2 totalForce = Vector2.zero;

        r = MGR_game.visuals.sizeByIndegree.Evaluate(MGR_game.mtx.NN.Indegree(id)) * (MGR_game.visuals.useScale ? 1 : 0);

        totalForce += CenteringForce(MGR_game.visuals.centeringStrength);
        totalForce += DragForce(MGR_game.visuals.dragStrength);

        if (MGR_game.visuals.showNodes) totalForce += NodesForces(MGR_game.visuals.applyIN, MGR_game.visuals.visualNodes, MGR_game.mtx.IN, MGR_game.mtx.NI);
        if (MGR_game.visuals.showIdeas) totalForce += NodesForces(MGR_game.visuals.applyII, MGR_game.visuals.visualIdeas, MGR_game.mtx.II, MGR_game.mtx.II);

        totalForce = totalForce.ClampLength(MGR_game.visuals.maxVel);
        rb.AddForce(totalForce);
    }
}
