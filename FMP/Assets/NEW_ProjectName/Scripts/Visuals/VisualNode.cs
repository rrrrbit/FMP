using System.Collections.Generic;
using UnityEngine;
using static MGR_graphView;

public class VisualNode : MonoBehaviour
{
	public int id;
	public bool onScreen = true;
	public float r;
	public MGR_graphView graphView;
	public MGR_gameMaths gameMaths;
	public Rigidbody2D rb;
	public SpriteRenderer sr;

	// will handle clicks

	private void OnBecameVisible()
    {
        onScreen = true;
    }

    private void OnBecameInvisible()
    {
        onScreen = false;
    }

    protected Vector2 NodesForces(bool applyAttraction, List<VisualNode> otherList, float[,] fromMtx, float[,] toMtx)
    {
        Vector2 totalForce = Vector2.zero;
        for (int otherId = 0; otherId < otherList.Count; otherId++)
        {
            if (fromMtx.Equals(toMtx) && otherId == id) continue;
            VisualNode other = otherList[otherId];

            Vector2 d = other.transform.position - transform.position;

            totalForce += Repulsion(other, d, graphView.padding);

            if (applyAttraction)
            {
                float fromThis = fromMtx[id, otherId];
                float toThis = toMtx[otherId, id];

                float w;
                if (graphView.symmetriseWeights) w = (Mathf.Abs(fromThis) + Mathf.Abs(toThis)) / 2;
                else w = Mathf.Abs(fromThis);

                float max = graphView.symmetriseWeights ? Mathf.Max(gameMaths.mtxStats[fromMtx].maxAbs, gameMaths.mtxStats[toMtx].maxAbs) : gameMaths.mtxStats[fromMtx].maxAbs;
                if (graphView.normaliseWeights) w /= max;

                if (w < graphView.pairwiseForceThreshold) continue;


                totalForce += Attraction(other, d, w, graphView.padding);
            }
        }
        return totalForce;
    }
    protected Vector2 Attraction(VisualNode other, Vector3 dv, float weight, float padding)
    {
        float radii = r + other.r;
        float d = Mathf.Max(dv.magnitude - padding - radii * (graphView.useScale ? 1 : 0), 0.01f);

        Vector2 force = RawAttraction(d, graphView.attractionType) * graphView.attractionStrength * graphView.attractionByWeight.Evaluate(weight) * dv.normalized;
        return force;
    }

    protected Vector2 Repulsion(VisualNode other, Vector3 dv, float padding)
    {
        float radii = r + other.r;
        float d = Mathf.Max(dv.magnitude - padding - radii * (graphView.useScale ? 1 : 0), 0.01f);

        Vector2 force = -RawRepulsion(d, graphView.repulsionType) * graphView.repulsionStrength * dv.normalized;
        return force;
    }

    protected float RawAttraction(float d, AttractionTypes attractionType)
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

    protected float RawRepulsion(float d, RepulsionTypes repulsionType)
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

    protected Vector2 CenteringForce(float strength)
    {
        return strength * transform.position.sqrMagnitude * -transform.position.normalized; ;
    }

    protected Vector2 DragForce(float strength)
    {
        return strength * rb.linearVelocity.sqrMagnitude * -rb.linearVelocity.normalized;
    }
}
