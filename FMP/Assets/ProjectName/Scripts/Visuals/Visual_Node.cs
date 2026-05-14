using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static MGR_visuals;

public class Visual_Node : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	public int id;
	public bool onScreen = true;
	public float r;
	public Rigidbody2D rb;
	public SpriteRenderer sr;
    protected bool dragging;

    // will handle clicks

    void OnBecameVisible()
    {
        onScreen = true;
    }

    void OnBecameInvisible()
    {
        onScreen = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Select();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        print("asdkfjg");
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }


    public virtual void Select() { }

    protected Vector2 NodesForces(bool applyAttraction, List<Visual_Node> otherList, float[,] fromMtx, float[,] toMtx)
    {
        Vector2 totalForce = Vector2.zero;
        for (int otherId = 0; otherId < otherList.Count; otherId++)
        {
            if (fromMtx.Equals(toMtx) && otherId == id) continue;
            Visual_Node other = otherList[otherId];

            Vector2 d = other.transform.position - transform.position;

            totalForce += Repulsion(other, d, MGR_game.visuals.padding);

            if (applyAttraction)
            {
                float fromThis = fromMtx[id, otherId];
                float toThis = toMtx[otherId, id];

                float w;
                if (MGR_game.visuals.symmetriseWeights) w = (Mathf.Abs(fromThis) + Mathf.Abs(toThis)) / 2;
                else w = Mathf.Abs(fromThis);

                float max = MGR_game.visuals.symmetriseWeights ? Mathf.Max(MGR_game.mtx.mtxStats[fromMtx].maxAbs, MGR_game.mtx.mtxStats[toMtx].maxAbs) : MGR_game.mtx.mtxStats[fromMtx].maxAbs;
                if (MGR_game.visuals.normaliseWeights) w /= max;

                if (w < MGR_game.visuals.pairForceWeightThreshold) continue;


                totalForce += Attraction(other, d, w, MGR_game.visuals.padding);
            }
        }
        return totalForce;
    }
    protected Vector2 Attraction(Visual_Node other, Vector3 dv, float weight, float padding)
    {
        float radii = r + other.r;
        float d = Mathf.Max(dv.magnitude - padding - radii * (MGR_game.visuals.useScale ? 1 : 0), 0.01f);

        Vector2 force = RawAttraction(d, MGR_game.visuals.attractionType) * MGR_game.visuals.attractionStrength * MGR_game.visuals.attractionByWeight.Evaluate(weight) * dv.normalized;
        return force;
    }

    protected Vector2 Repulsion(Visual_Node other, Vector3 dv, float padding)
    {
        float radii = r + other.r;
        float d = Mathf.Max(dv.magnitude - padding - radii * (MGR_game.visuals.useScale ? 1 : 0), 0.01f);

        Vector2 force = -RawRepulsion(d, MGR_game.visuals.repulsionType) * MGR_game.visuals.repulsionStrength * dv.normalized;
        return force;
    }

    protected float RawAttraction(float d, AttractionTypes attractionType)
    {
        return attractionType switch
        {
            AttractionTypes.Linear => d,
            AttractionTypes.Log => Mathf.Log(d + 1),
            AttractionTypes.Quadratic => d * d,
            _ => 0,
        };
    }

    protected float RawRepulsion(float d, RepulsionTypes repulsionType)
    {
        return repulsionType switch
        {
            RepulsionTypes.Reciprocal => 1 / d,
            RepulsionTypes.InverseSqr => 1 / (d * d),
            _ => 0,
        };
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
