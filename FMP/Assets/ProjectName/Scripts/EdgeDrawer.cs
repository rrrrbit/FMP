using RBitUtils;
using Unity.Burst;
using UnityEngine;

public class EdgeDrawer : MonoBehaviour
{
    MGR_graphView view;
    
    Vector2[] p;
    float[] r;
    float[,] mtx;
    
    MeshFilter mf;
    MeshRenderer mr;

    Vector3[] verts;
    int[] tris;

    [SerializeField] AnimationCurve widthByWeight;
    
    Vector2Int[] edgePairs;

    class Edge
    {
        public Vector3 from, to;
        public float width;
        public Color colour;
    }
    Edge[] edges;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        view = Managers.Get<MGR_graphView>();
        mf = GetComponent<MeshFilter>();
        edgePairs  = new Vector2Int[mtx.Length];

        int edgeIndex = 0;
        for (int from = 0; from < mtx.Rows(); from++)
        {
            for (int to = 0; to < mtx.Cols(); to++)
            {
                edgePairs[edgeIndex++] = new(from, to);
            }
        }
    }

    void InitEdges()
    {
        edges = new Edge[edgePairs.Length];
        for(int pair = 0; pair < edgePairs.Length; pair++)
        {
            //  edges[pair].from = view.
        }
    }

    void UpdateEdges()
    {
        foreach (Vector2Int pair in edgePairs)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
