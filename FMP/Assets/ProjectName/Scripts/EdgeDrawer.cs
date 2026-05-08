using RBitUtils;
using Unity.Burst;
using UnityEngine;

public class EdgeDrawer : MonoBehaviour
{
    MGR_graphView view;

	float[,] mtx;
    
    MeshFilter mf;
    MeshRenderer mr;

    Vector3[] verts;
    int[] tris;

    [SerializeField] AnimationCurve widthByWeight;
    
    Vector2Int[] edgePairs;

	public float offCenter = 0.1f;
	public float arrowHeadSize = 3f;
	class Edge
    {
        public Vector3 from, to;
        public float width;
        public Color colour;
    }
    Edge[] edges;

    void Start()
    {
        view = Managers.Get<MGR_graphView>();
		mtx = view.graph.mtx;
        mf = GetComponent<MeshFilter>();
        edgePairs  = new Vector2Int[mtx.Length];
		edges = new Edge[edgePairs.Length];
		for (int i = 0; i < edges.Length; i++)
		{
			edges[i] = new Edge();
		}

		verts = new Vector3[edgePairs.Length * 5];
		tris = new int[edgePairs.Length * 3 * 3];

		mf = GetComponent<MeshFilter>();
		mf.mesh = new Mesh();

		int edgeIndex = 0;
        for (int from = 0; from < mtx.Rows(); from++)
        {
            for (int to = 0; to < mtx.Cols(); to++)
            {
                edgePairs[edgeIndex++] = new(from, to);
            }
        }

		UpdateEdges();
		UpdateMesh();
	}
    void UpdateEdges()
    {
		for (int pair = 0; pair < edgePairs.Length; pair++)
		{
			MGR_graphView.VisualNodeProperties from = view.vn[edgePairs[pair].x];
			MGR_graphView.VisualNodeProperties to = view.vn[edgePairs[pair].y];

			float weight = mtx[edgePairs[pair].x, edgePairs[pair].y];
			Vector2 dir = (to.p - from.p).normalized;

			edges[pair].from = from.p + dir * from.r; // r could be removed
			edges[pair].to = to.p - dir * to.r;
			edges[pair].width = widthByWeight.Evaluate(Mathf.Abs(weight) / view.graph.maxAbsWeight);

			if (weight >= 0)
			{
				edges[pair].colour = Color.green;
			}
			else
			{
				edges[pair].colour = Color.red;
			}
		}
	}

	void UpdateMesh()
	{
		int vert = 0;
		foreach(Edge edge in edges)
		{
			Vector3 dir = (edge.to - edge.from).normalized;
			Vector3 perp = new(-dir.y, dir.x);

			/*
			                2
			                |`\
			4---------------3  `\
			|                    `\
			0----------------------`1

			tris: 123 043 013
			 */
			verts[vert++] = perp * offCenter + edge.from;

			verts[vert++] = perp * offCenter + edge.to;
			verts[vert++] = perp * offCenter + edge.to + perp * edge.width * arrowHeadSize - dir * edge.width * arrowHeadSize;
			verts[vert++] = perp * offCenter + edge.to + perp * edge.width * 1 - dir * edge.width * arrowHeadSize;

			verts[vert++] = perp * offCenter + edge.from + perp * edge.width * 1;
		}

		vert = 0;
		int tri = 0;
		foreach(Edge edge in edges)
		{

			tris[tri++] = vert + 0;
			tris[tri++] = vert + 4;
			tris[tri++] = vert + 3;

			tris[tri++] = vert + 1;
			tris[tri++] = vert + 3;
			tris[tri++] = vert + 2;

			tris[tri++] = vert + 0;
			tris[tri++] = vert + 3;
			tris[tri++] = vert + 1;

			vert += 5;
		}

		mf.mesh.RecalculateBounds();
		mf.mesh.vertices = verts;
		mf.mesh.triangles = tris;
	}

    // Update is called once per frame
    void Update()
    {
		UpdateEdges();
		UpdateMesh();
	}
}
