using RBitUtils;
using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EdgeDrawer : MonoBehaviour
{
    MGR_graphView view;
	MGR_gameMaths gameMaths;

	float[,] mtx;
    
    MeshFilter mf;
    MeshRenderer mr;

	int vertCount = 10;
    Vector3[] verts;
    int[] tris;
	Vector2[] uvs;

	[SerializeField] AnimationCurve widthByWeight;
	[SerializeField] float width;
	[SerializeField] float maxWorldScale;
    [SerializeField] float fadeLength = 0.2f;
    [SerializeField] bool normaliseWeight = true;
    [SerializeField] float colourMinWeight = -2f;
	[SerializeField] float colourMaxWeight = 2f;
    [SerializeField] bool constantScreenWidth = true;
    [SerializeField] float fadeTime = 0.25f;

	Vector2Int[] edgePairs;

	public float offCenter = 0.1f;
	public float arrowHeadSize = 3f;

	[SerializeField] GameCamera cam;
    [Serializable]
    class Edge
    {
        public Vector3 from, to;
        public float width;
		public Vector2 uvFrom;
		public Vector2 uvTo;
        public Vector2 uvMiddle;
        public float fadeLength;
    }
    [SerializeField] Edge[] edges;

    void Start()
    {
        view = Managers.Get<MGR_graphView>();
		gameMaths = Managers.Get<MGR_gameMaths>();
		mtx = view.graph;
        mf = GetComponent<MeshFilter>();
        edgePairs  = new Vector2Int[mtx.Length];
		edges = new Edge[edgePairs.Length];
		for (int i = 0; i < edges.Length; i++)
		{
			edges[i] = new Edge();
		}

		verts = new Vector3[edgePairs.Length * vertCount];
		uvs = new Vector2[edgePairs.Length * vertCount];
		tris = new int[edgePairs.Length * (vertCount-2) * 3];

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

		UpdateEdges(Time.deltaTime);
		UpdateMesh();
	}
    // Update is called once per frame
    void Update()
    {
		UpdateEdges( Time.deltaTime);
		UpdateMesh();
		UpdateColours();
	}

    void UpdateEdges(float dt)
    {
		for (int pair = 0; pair < edgePairs.Length; pair++)
		{
			MGR_graphView.VisualNodeProperties from = view.vn[edgePairs[pair].x];
			MGR_graphView.VisualNodeProperties to = view.vn[edgePairs[pair].y];

			float scaleMult = (constantScreenWidth ? ((cam.currentZoom-maxWorldScale) /(1-Mathf.Exp(cam.currentZoom - maxWorldScale))) + maxWorldScale : 1);
			
            float weight = mtx[edgePairs[pair].x, edgePairs[pair].y];
			Vector2 dir = (to.p - from.p).normalized;
            Vector2 perp = new(-dir.y, dir.x);

            edges[pair].from = from.p + dir * from.r + perp * offCenter * scaleMult; 
			edges[pair].to = to.p - dir * to.r + perp * offCenter * scaleMult;



            edges[pair].width = width * widthByWeight.Evaluate(Mathf.Abs(weight) / (normaliseWeight ? gameMaths.statsNN.maxAbs : 1f)) * scaleMult;

			float u = (weight - colourMinWeight) / (colourMaxWeight - colourMinWeight);
			float vFrom = view.vn[edgePairs[pair].x].obj.onScreen ? 1 : 0;
            float vTo = view.vn[edgePairs[pair].y].obj.onScreen ? 1 : 0;
            float vMiddle = view.vn[edgePairs[pair].x].obj.onScreen && view.vn[edgePairs[pair].y].obj.onScreen ? 1 : 0;

            edges[pair]. fadeLength = fadeLength * scaleMult;

            float vFromNext = Mathf.MoveTowards(edges[pair].uvFrom.y, vFrom, dt / fadeTime);
            float vMiddleNext = Mathf.MoveTowards(edges[pair].uvMiddle.y, vMiddle, dt / fadeTime);
            float vToNext= Mathf.MoveTowards(edges[pair].uvTo.y, vTo, dt / fadeTime);



            edges[pair].uvFrom = new Vector2(u, vFromNext);
            edges[pair].uvMiddle = new Vector2(u, vMiddleNext);
			edges[pair].uvTo = new Vector2(u, vToNext);
        }
	}

	void UpdateColours()
	{
		int vert = 0;
		foreach(Edge edge in edges)
		{
            uvs[vert++] = edge.uvFrom;

            uvs[vert++] = edge.uvMiddle;
            uvs[vert++] = edge.uvMiddle;

            uvs[vert++] = edge.uvTo;
            uvs[vert++] = edge.uvTo;
            uvs[vert++] = edge.uvTo;
            uvs[vert++] = edge.uvTo;

            uvs[vert++] = edge.uvMiddle;
            uvs[vert++] = edge.uvMiddle;

            uvs[vert++] = edge.uvFrom;
        }

		mf.mesh.uv = uvs;
	}

	void UpdateMesh()
	{
        int vert = 0;
		foreach(Edge edge in edges)
		{
            Vector3 dir = (edge.to - edge.from).normalized;
            Vector3 perp = new(-dir.y, dir.x);
            Vector3 Offset(float x, float y) => dir * x + perp * y;

            /* verts:
							5
							|`\
			9---8-------7---6  `\
			|                    `\
			0---1-------2---3------`4

			*/
            verts[vert++] = edge.from;
            verts[vert++] = edge.from + Offset(edge.fadeLength, 0);
            verts[vert++] = edge.to + Offset(-edge.width * arrowHeadSize - edge.fadeLength, 0);
            verts[vert++] = edge.to + Offset(-edge.width * arrowHeadSize, 0);
            verts[vert++] = edge.to;

            verts[vert++] = edge.to + Offset(-edge.width * arrowHeadSize, edge.width * arrowHeadSize);

            verts[vert++] = edge.to + Offset(-edge.width * arrowHeadSize, edge.width);

            verts[vert++] = edge.to + Offset(-edge.width * arrowHeadSize - edge.fadeLength, edge.width);
            verts[vert++] = edge.from + Offset(edge.fadeLength, edge.width);
            verts[vert++] = edge.from + Offset(0, edge.width);
        }

		vert = 0;
		int tri = 0;
		foreach(Edge edge in edges)
		{

            /* tris: 

			019
			189

			128
			278

			237
			367

			345

			*/

            tris[tri++] = vert + 0;
            tris[tri++] = vert + 1;
            tris[tri++] = vert + 9;

            tris[tri++] = vert + 1;
            tris[tri++] = vert + 8;
            tris[tri++] = vert + 9;

            tris[tri++] = vert + 1;
            tris[tri++] = vert + 2;
            tris[tri++] = vert + 8;

            tris[tri++] = vert + 2;
            tris[tri++] = vert + 7;
            tris[tri++] = vert + 8;

            tris[tri++] = vert + 2;
            tris[tri++] = vert + 3;
            tris[tri++] = vert + 7;

            tris[tri++] = vert + 3;
            tris[tri++] = vert + 6;
            tris[tri++] = vert + 7;

            tris[tri++] = vert + 3;
            tris[tri++] = vert + 4;
            tris[tri++] = vert + 5;

            vert += vertCount;
        }

		mf.mesh.RecalculateBounds();
		mf.mesh.vertices = verts;
		mf.mesh.triangles = tris;
	}

}
