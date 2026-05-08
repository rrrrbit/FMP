using RBitUtils;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EdgeDrawerTest : MonoBehaviour
{
	[SerializeField] Transform from;
	[SerializeField] Transform to;
    
    MeshFilter mf;
    MeshRenderer mr;

    Vector3[] verts;
    int[] tris;
	Vector2[] uvs;

	[SerializeField] float width = 0.1f;

	public float u;
	public float offCenter = 0.1f;
	public float arrowHeadSize = 3f;
    public float endsFadeLength = 0.2f;
	public bool fromShow;
	public bool toShow;

    int vertCount = 10;

    [Serializable]
	class Edge
    {
        public Vector3 from, to;
        public float width;
		public Vector2 uvFrom;
		public Vector2 uvTo;
        public Vector2 uvMiddle;
    }
    [SerializeField] Edge edge;

    void Start()
    {
        mf = GetComponent<MeshFilter>();

		verts = new Vector3[vertCount];
		uvs = new Vector2[vertCount];
		tris = new int[(vertCount-2) * 3];

		mf = GetComponent<MeshFilter>();
		mf.mesh = new Mesh();

		UpdateEdges();
		UpdateMesh();
	}
    void UpdateEdges()
    {
        Vector2 dir = (to.position - from.position).normalized;
        Vector3 perp = new(-dir.y, dir.x);

        edge.from = from.position + perp * offCenter;
        edge.to = to.position + perp * offCenter;

        edge.width = width;

        float vFrom = fromShow ? 1 : 0;
        float vTo = toShow ? 1 : 0;
        float vMiddle = fromShow && toShow ? 1 : 0;


        edge.uvFrom = new Vector2(u, vFrom);
        edge.uvTo = new Vector2(u, vTo);
        edge.uvMiddle = new Vector2(u, vMiddle);
    }

	void UpdateColours()
	{
		int vert = 0;
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

        mf.mesh.uv = uvs;
	}

	void UpdateMesh()
	{
		int vert = 0;
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
        verts[vert++] = edge.from + Offset(endsFadeLength, 0);
        verts[vert++] = edge.to + Offset(-edge.width * 2 - endsFadeLength, 0);
        verts[vert++] = edge.to + Offset(-edge.width * 2, 0);
        verts[vert++] = edge.to;

        verts[vert++] = edge.to + Offset(-edge.width * 2, edge.width * 2);

        verts[vert++] = edge.to + Offset(-edge.width * 2, edge.width);

        verts[vert++] = edge.to + Offset(-edge.width * 2 - endsFadeLength, edge.width);
        verts[vert++] = edge.from + Offset(endsFadeLength, edge.width);
        verts[vert++] = edge.from + Offset(0, edge.width);

        /* tris: 

        019
        189

        128
        278

        237
        367

        345

         */
        vert = 0;
		int tri = 0;
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

        mf.mesh.RecalculateBounds();
		mf.mesh.vertices = verts;
		mf.mesh.triangles = tris;
	}

    // Update is called once per frame
    void Update()
    {
		UpdateEdges();
		UpdateMesh();
		UpdateColours();
	}
}
