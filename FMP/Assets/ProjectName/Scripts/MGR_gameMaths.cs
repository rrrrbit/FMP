using RBitUtils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class MGR_gameMaths : MonoBehaviour, IGameMaths
{
	[Header("Misc")]
	public int startingNumberPeople;
	public int startingNumberIdeas;
	[Header("Statistics")]
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxOutdegree;
	public float sumOutdegree;
	[Header("Runtime & Refs")]
	public AdjacencyMtx n_n;
    public AdjacencyMtx n_i;
    public AdjacencyMtx i_n;
    public AdjacencyMtx i_i;
	public List<PersonNode> nodes;
    public List<IdeaNode> ideas;
	public TextMeshProUGUI debugText;
	public event System.Action OnReadyForVisualisation;
    [Header("debug")]
    public List<float> debugFlatMtx;

	private void Start()
	{
		// initialise lists of all nodes & ideas
		nodes = new List<PersonNode>(startingNumberPeople);
		for (int i = 0; i < startingNumberPeople; i++)
		{
			nodes.Add(new PersonNode());
		}

        ideas = new List<IdeaNode>(startingNumberIdeas);
        for (int i = 0; i < startingNumberIdeas; i++)
        {
            ideas.Add(new IdeaNode());
        }

        // initialise nn with random weights
        n_n = new AdjacencyMtx(nodes, nodes);
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = Random.value * 2 - 1;
                n_n.mtx[i, j] = Mathf.Pow(x, 10) /10f;
            }
        }

        // initialise ni with random weights
        n_i = new AdjacencyMtx(ideas, nodes);
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < ideas.Count; j++)
            {
                float x = Random.value * 2 - 1;
                n_i.mtx[i, j] = x;
            }
        }

        // initialise in to 0s
        i_n = new AdjacencyMtx(nodes, ideas);
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                i_n.mtx[i, j] = 0;
            }
        }

        // initialise ii with random weights
        i_i = new AdjacencyMtx(ideas, ideas);
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int j = 0; j < ideas.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = Random.value * 2 - 1;
                i_i.mtx[i, j] = x;
            }
        }



        OnReadyForVisualisation?.Invoke();
	}

    void Step(float dt)
    {
        // weights are read for calculations and also written in the same pass.
        // same issue as graph view physics, read and write should be done in seperate passes.
        // but might tank performance. fix l8r

        // in
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int n = 0; n < nodes.Count; n++)
            {
                i_n.mtx[i, n] = CalcIN(i, n);
            }
        }

        // ni
        for (int n = 0; n < nodes.Count; n++)
        {
            for (int i = 0; i < ideas.Count; i++)
            {
                n_i.mtx[n, i] += CalcDeltaNI(n, i) * dt;
            }
        }

        // nn
        for (int a = 0; a < nodes.Count; a++)
        {
            for (int b = 0; b < nodes.Count; b++)
            {
                if (a == b) continue;
                n_n.mtx[a, b] += CalcDeltaNN(a, b) * dt;
            }
        }
    }

    float CalcIN(int i, int n)
    {
        float inAccm = 0;
        for (int k = 0; k < ideas.Count; k++)
        {
            if (i == k || i_i.mtx[i, k] == 0) continue; // skip if nodes are equal

            // alignment of idea i to node n scales by 
            // sum of alignments of i to each idea * alignments of each idea to i
            float nanCatch = LogScaling(i_i.mtx[i, k] * n_i.mtx[n, k]);

            if (!float.IsFinite(nanCatch)) continue; // skip if calculation broke

            inAccm += nanCatch;
        }
        return inAccm;
    }

    float CalcDeltaNI(int n, int i)
    {
        float niDeltaAccm = 0;
        for (int k = 0; k < nodes.Count; k++)
        {
            if (n == k || n_n.mtx[n, k] == 0) continue;

            // delta alignment of node n to idea i scales by 
            // sum of alignments of n to each node * alignments of each node to i. (log)
            float nanCatch = LogScaling(n_n.mtx[n, k] * n_i.mtx[k, i]);

            if (!float.IsFinite(nanCatch)) continue;

            niDeltaAccm += nanCatch;
        }
        return niDeltaAccm;
    }

    float CalcDeltaNN(int a, int b)
    {
        float nnDeltaAccm = 0;
        for (int k = 0; k < ideas.Count; k++)
        {
            // delta alignment of node a to node b scales by
            // sum of alignments of each idea to b * alignments of a to each idea
            float nanCatch = LogScaling(i_n.mtx[k, b] * n_i.mtx[a, k]); // add mutual connection somewhere in here

            if (!float.IsFinite(nanCatch)) continue;

            nnDeltaAccm += nanCatch;
        }
        return nnDeltaAccm;
    }

    float LogScaling(float x) => Mathf.Log(Mathf.Abs(x) + 1) * Mathf.Sign(x); // replace with something cheaper

	private void Update()
	{
        n_n.RecalculateStats();
        UpdateStatistics();
        Step(Time.deltaTime);
	}

	void UpdateStatistics()
	{
		max = n_n.maxWeight;
		min = n_n.minWeight;
		maxAbs = n_n.maxAbsWeight;
		sumAbs = n_n.sumAbsWeight;
		maxOutdegree = n_n.maxOutdegree;
        debugFlatMtx = i_i.FlatMtx().ToList();
	}
}

[System.Serializable]
public class Node
{
	public VisualNode visual;
	public static implicit operator VisualNode(Node node) => node.visual;
}

public class PersonNode : Node
{

}

public class IdeaNode : Node
{
    //public VisualNode visual;
    //public static implicit operator VisualNode(Node node) => node.visual;
}


public class AdjacencyMtx
{
	public List<Node> nodes;
	public float[,] mtx;
    public float maxWeight;
    public float minWeight;
    public float maxAbsWeight;
    public float sumAbsWeight;
    public float maxOutdegree;

    /// <summary>
    /// Construct a matrix [y,x] with From as rows and To as columns.
    /// </summary>
    /// <param name="nodesTo"></param>
    /// <param name="nodesFrom"></param>
    public AdjacencyMtx(IEnumerable<Node> nodesTo, IEnumerable<Node> nodesFrom)
	{
		nodes = new List<Node>();
		nodes.AddRange(nodesTo);
		nodes.AddRange(nodesFrom);
        nodes = nodes.ToHashSet().ToList();
		mtx = new float[nodesFrom.Count(), nodesTo.Count()];
	}

    public void RecalculateStats()
    {
        maxWeight = Mathf.Max(FlatMtx());
        minWeight = Mathf.Min(FlatMtx());
        maxAbsWeight = Mathf.Max(FlatMtx().Select(x => Mathf.Abs(x)).ToArray());
        sumAbsWeight = FlatMtx().Sum(x => Mathf.Abs(x));
        maxOutdegree = nodes.Max(x => GetIndegree(x));
    }

    /// <summary>
    /// Get a row as an array.
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public float[] GetEdgesFrom(int from)
    {
        float[] edges = new float[mtx.Cols()];
        for (int to = 0; to < edges.Length; to++)
        {
            edges[to] = mtx[from, to];
        }
        return edges;
    }
	public float[] GetEdgesFrom(Node fromNode) => GetEdgesFrom(nodes.FindIndex(x => x == fromNode));

    /// <summary>
    /// Get a column as an array.
    /// </summary>
    /// <param name="to"></param>
    /// <returns></returns>
    public float[] GetEdgesTo(int to)
    {
        float[] edges = new float[mtx.Rows()];
        for (int from = 0; from < edges.Length; from++)
        {
            edges[from] = mtx[from, to];
        }
        return edges;
    }
    public float[] GetEdgesTo(Node fromNode) => GetEdgesTo(nodes.FindIndex(x => x == fromNode));

    public float GetIndegree(int to)
    {
        float sum = 0;
        for (int from = 0; from < mtx.Cols(); from++)
        {
			sum += Mathf.Abs(mtx[from, to]);
		}
        return sum;
    }
    public float GetIndegree(Node fromNode) => GetIndegree(nodes.FindIndex(x => x == fromNode));
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

    //public float maxWeight => Mathf.Max(FlatMtx());
    //public float minWeight => Mathf.Min(FlatMtx());
    //public float maxAbsWeight => Mathf.Max(FlatMtx().Select(x => Mathf.Abs(x)).ToArray());
    //public float sumAbsWeight => FlatMtx().Sum(x => Mathf.Abs(x));
    //public float maxOutdegree => nodes.Max(x => GetOutdegree(x));
}