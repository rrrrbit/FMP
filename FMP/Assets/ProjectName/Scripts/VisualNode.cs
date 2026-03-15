using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
public class VisualNode : MonoBehaviour
{
	public Vector3 v;
	public Node node;
	public GameMaths gameMaths;
	public float outdegree;
	public List<float> connections;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnDrawGizmosSelected()
	{
		Vector3 weightedCentroid = Vector3.zero;
		foreach (Node j in gameMaths.nodes)
		{
			weightedCentroid += j.visual.transform.position * j.outdegree * gameMaths.nn[node][j] / gameMaths.nodes.Sum(x => x.outdegree * gameMaths.nn[node][j]);
		}
		Gizmos.DrawSphere(weightedCentroid, 0.1f);
	}
}
