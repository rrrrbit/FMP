using RBitUtils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class MGR_gameMaths : MonoBehaviour, IGameMaths
{
	[Header("Misc")]
	public int startingNumber;
	[Header("Statistics")]
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxOutdegree;
	public float sumOutdegree;
	[Header("Runtime & Refs")]
	public AdjacencyMtx nn;
	public List<Node> nodes;
	public TextMeshProUGUI debugText;
	public event System.Action OnReadyForVisualisation;

	private void Start()
	{
		// initialise list of all nodes
		nodes = new List<Node>();

		for (int i = 0; i < startingNumber; i++)
		{
			nodes.Add(new Node());
		}

		// initialise 2d dictionary with random weights
		nn = new AdjacencyMtx(nodes, nodes);
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = Random.value * 2 - 1;
                nn.mtx[i, j] = Mathf.Pow(x, 11) * 10;
            }
        }
        OnReadyForVisualisation?.Invoke();
	}

	private void Update()
	{
		UpdateStatistics();
	}

	void UpdateStatistics()
	{
		max = nn.maxWeight;
		min = nn.minWeight;
		maxAbs = nn.maxAbsWeight;
		sumAbs = nn.sumAbsWeight;
		maxOutdegree = nn.maxOutdegree;
	}
}

[System.Serializable]
public class Node
{
	public VisualNode visual;
	public static implicit operator VisualNode(Node node) => node.visual;
}

public class AdjacencyMtx
{
	public List<Node> nodes;
	public float[,] mtx;
	
	public AdjacencyMtx(IEnumerable<Node> nodesTo, IEnumerable<Node> nodesFrom)
	{
		nodes = new List<Node>();
		nodes.AddRange(nodesTo);
		nodes.AddRange(nodesFrom);
        nodes = nodes.ToHashSet().ToList();
		mtx = new float[nodesTo.Count(), nodesFrom.Count()];
	}
    public float[] GetEdgesFrom(int from)
    {
        float[] edges = new float[mtx.Rows()];
        for (int to = 0; to < edges.Length; to++)
        {
            edges[to] = mtx[to, from];
        }
        return edges;
    }
	public float[] GetEdgesFrom(Node fromNode) => GetEdgesFrom(nodes.FindIndex(x => x == fromNode));

    public float[] GetEdgesTo(int to)
    {
        float[] edges = new float[mtx.Cols()];
        for (int from = 0; from < edges.Length; from++)
        {
            edges[from] = mtx[to, from];
        }
        return edges;
    }
    public float[] GetEdgesTo(Node fromNode) => GetEdgesTo(nodes.FindIndex(x => x == fromNode));

    public float GetOutdegree(int from)
    {
        float sum = 0;
        for (int to = 0; to < mtx.Rows(); to++)
        {
            try
            {
                sum += Mathf.Abs(mtx[to, from]);
            }
            catch
            {
                Debug.Log("fucking broke at "+to.ToString()+", "+ from.ToString());
            }
        }
        return sum;
    }
    public float GetOutdegree(Node fromNode) => GetOutdegree(nodes.FindIndex(x => x == fromNode));

    public float[] FlatMtx()
    {
        float[] flat = new float[mtx.Length];
        Vector2Int size = mtx.Dimensions();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                flat[i * size.y + j] = mtx[i, j];
            }
        }
        return flat;
    }

    public float maxWeight => Mathf.Max(FlatMtx());
	public float minWeight => Mathf.Min(FlatMtx());
    public float maxAbsWeight => Mathf.Max(FlatMtx().Select(x => Mathf.Abs(x)).ToArray());
    public float sumAbsWeight => FlatMtx().Sum(x => Mathf.Abs(x));
    public float maxOutdegree => nodes.Max(x => GetOutdegree(x));
}