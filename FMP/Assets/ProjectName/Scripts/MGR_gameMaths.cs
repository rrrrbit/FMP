using RBitUtils;
using System;
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
	[Header("graph stats")]
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxOutdegree;
	public float sumOutdegree;
	public event System.Action OnReadyForVisualisation;
	[Header("Runtime & Refs")]
    [Header("- Lists")]
	public List<PersonNode> nodes;
    public List<IdeaNode> ideas;
    [Header("- Matrices")]
	public AdjacencyMtx n_n;
    public AdjacencyMtx n_i;
    public AdjacencyMtx i_n;
    public AdjacencyMtx i_i;
    [Header("-- Internal")]
    float[,] nnNext;
    float[,] niNext;
    float[,] inNext;
    float[,] iiNext;
    [Header("- Node Stats")]
    float[] nodeComplexity;
    BumpCurveParams[] nodeComplexityTolerance;
    MagicCurveParams[] nodeEnthusiasm;
    float[] nodeReach;
    MagicCurveParams[] nodeSuggestibility;
    MagicCurveParams[] nodeAdherence;
    [Header("-- Idea Stats")]
    float[] ideaComplexity;
    MagicCurveParams[] ideaTolerance;
    [Header("debug")]
	public TextMeshProUGUI debugText;
    public List<float> debugFlatMtx;


    #region utilities
    /// <summary>
    /// Mirror f(x) for positive x, to negative x. Good for making sigmoids
    /// </summary>
    /// <param name="f"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    float Symmetricise(Func<float, float> f, float x) => f(Mathf.Abs(x)) * Mathf.Sign(x);
    float LogScaling(float x) => Symmetricise(x => Mathf.Log10(x + 1), x);
    float RecScaling(float x, float k) => Symmetricise(x => 1 - (k / (x + k)), x);
    /// <summary>
    /// Parametric f(x) with an optional threshold and asymmetric shape. <a href="https://www.desmos.com/calculator/ygh3492ofo">See demo.</a>
    /// </summary>
    /// <param name="xRaw"></param>
    /// <param name="activation">value of x where threshold is roughly fully open (0.99 hard coded).</param>
    /// <param name="steepness">Maximum gradient of threshold</param>
    /// <returns></returns>
    float MagicCurve(float xRaw, MagicCurveParams param)
    {
        float activation = 0;
        float activationSteepness = 0;
        float flatness = 0;
        if (xRaw >= 0)
        {
            activation = param.activationPos;
            activationSteepness = param.activationSteepnessPos;
            flatness = param.flatnessPos;
        }
        else
        {
            activation = param.activationNeg;
            activationSteepness = param.activationSteepnessNeg;
            flatness = param.flatnessNeg;
        }
        
        float x = Mathf.Abs(xRaw);

        float sigmoid = 1 / (1 + Mathf.Exp(-Mathf.Log(99)-4 * activationSteepness * (x - activation)));
        float curve = Mathf.Pow(Mathf.Log(flatness * x + 1), 1/flatness);

        return sigmoid * curve * Mathf.Sign(xRaw);
    }

    float BumpCurve(float x, BumpCurveParams param)
    {
        return param.peak * Mathf.Exp(Mathf.Pow(-Mathf.Abs((x - param.center) / param.width), param.steepness));
    }

    void InitFloatArr(ref float[] x, int length, float min, float max)
    {
        x = new float[length];
        for (int i = 0; i < length; i++)
        {
            x[i] = UnityEngine.Random.Range(min, max);
        }
    }

    void InitMagicCurves(ref MagicCurveParams[] x, int length, MagicCurveParams min, MagicCurveParams max)
    {
        x = new MagicCurveParams[length];
        for (int i = 0; i < length; i++)
        {
            x[i] = new MagicCurveParams()
            {
                activationPos = UnityEngine.Random.Range(min.activationPos, max.activationPos),
                activationSteepnessPos = UnityEngine.Random.Range(min.activationSteepnessPos, max.activationSteepnessPos),
                flatnessPos = UnityEngine.Random.Range(min.flatnessPos, max.flatnessPos),

                activationNeg = UnityEngine.Random.Range(min.activationNeg, max.activationNeg),
                activationSteepnessNeg = UnityEngine.Random.Range(min.activationSteepnessNeg, max.activationSteepnessNeg),
                flatnessNeg = UnityEngine.Random.Range(min.flatnessNeg, max.flatnessNeg)
            };
        }
    }

    void InitBumpCurves(ref BumpCurveParams[] x, int length, BumpCurveParams min, BumpCurveParams max)
    {
        x = new BumpCurveParams[length];
        for (int i = 0; i < length; i++)
        {
            x[i] = new BumpCurveParams()
            {
                center = UnityEngine.Random.Range(min.center, max.center),
                peak = UnityEngine.Random.Range(min.peak, max.peak),
                width = UnityEngine.Random.Range(min.width, max.width),
                steepness = UnityEngine.Random.Range(min.steepness, max.steepness),
            };
        }
    }
    #endregion

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

        MagicCurveParams minMagicCurve = new()
        {
            activationPos = 2,
            activationNeg = 2,

            activationSteepnessPos = 1,
            activationSteepnessNeg = 1,
            
            flatnessPos = 1,
            flatnessNeg = 1,
        };
        MagicCurveParams maxMagicCurve = new()
        {
            activationPos = 10,
            activationNeg = 10,

            activationSteepnessPos = 1,
            activationSteepnessNeg = 1,

            flatnessPos = 10,
            flatnessNeg = 10,
        };

        BumpCurveParams minBumpCurve = new()
        {
            center = -1,
            peak = 1,
            width = 1,
            steepness = 2,
        };

        // node stats
        InitFloatArr(ref nodeComplexity, startingNumberPeople, 0.5f, 1);
        InitBumpCurves(ref nodeComplexityTolerance, startingNumberPeople, minBumpCurve, minBumpCurve);
        InitMagicCurves(ref nodeEnthusiasm, startingNumberPeople, minMagicCurve, maxMagicCurve);
        InitFloatArr(ref nodeReach, startingNumberPeople, 0.5f, 1);
        InitMagicCurves(ref nodeSuggestibility, startingNumberPeople, minMagicCurve, maxMagicCurve);
        InitMagicCurves(ref nodeAdherence, startingNumberPeople, minMagicCurve, maxMagicCurve);

        // idea stats
        InitFloatArr(ref ideaComplexity, startingNumberIdeas, .5f, 1);
        InitMagicCurves(ref ideaTolerance, startingNumberIdeas, minMagicCurve, maxMagicCurve);

        // initialise nn with random weights
        n_n = new AdjacencyMtx(nodes, nodes);
        nnNext = new float[nodes.Count(), nodes.Count()];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                n_n.mtx[i, j] = Mathf.Pow(x, 11) /10f;
            }
        }

        // initialise ni with random weights
        n_i = new AdjacencyMtx(ideas, nodes);
        niNext = new float[nodes.Count(), ideas.Count()];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < ideas.Count; j++)
            {
                float x = UnityEngine.Random.value * 2 - 1;
                n_i.mtx[i, j] = x;
            }
        }

        // initialise in to 0s
        i_n = new AdjacencyMtx(nodes, ideas);
        inNext = new float[ideas.Count(), nodes.Count()];
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
                float x = UnityEngine.Random.value * 2 - 1;
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

        //i_n.mtx = inNext;
        //n_i.mtx = niNext;
        //n_n.mtx = nnNext; as expected does tank.
    }

    float CalcIN(int i, int n)
    {
        float inAccm = 0;
        for (int k = 0; k < ideas.Count; k++)
        {
            if (i == k || i_i.mtx[i, k] == 0) continue; // skip if nodes are equal

            // alignment of idea i to node n scales by 
            // sum of alignments of i to each idea * alignments of each idea to i
            float nanCatch = i_i.mtx[i, k] * n_i.mtx[n, k];

            if (!float.IsFinite(nanCatch)) continue; // skip if calculation broke

            inAccm += nanCatch;
        }
        return inAccm;
    }

    float CalcDeltaNI(int n, int i)
    {
        float suggestabilityAccm = 0;
        for (int k = 0; k < nodes.Count; k++)
        {
            if (n == k || n_n.mtx[n, k] == 0) continue;

            // delta alignment of node n to idea i scales by 
            // sum of alignments of n to each node * alignments of each node to i. (log)
            float nanCatch = LogScaling(n_n.mtx[n, k] * n_i.mtx[k, i]);

            if (!float.IsFinite(nanCatch)) continue;

            suggestabilityAccm += nanCatch;
        }

        float internalInfluenceAccm = 0;
        for (int k = 0; k < ideas.Count; k++)
        {
            if (i == k || i_i.mtx[i, k] == 0) continue;


        }

        return suggestabilityAccm + internalInfluenceAccm;
    }

    float CalcDeltaNN(int a, int b)
    {
        float popularityTermAccm = 0;
        for (int k = 0; k < ideas.Count; k++)
        {
            float nanCatch = LogScaling(i_n.mtx[k, b] * n_i.mtx[a, k]);

            if (!float.IsFinite(nanCatch)) continue;

            popularityTermAccm += nanCatch;
        }

        float mutualScore = 0;
        for (int k = 0; k < nodes.Count; k++)
        {
            mutualScore += Mathf.Abs(n_n.mtx[a, k] * n_n.mtx[k, b]);  
        }


        return 0;//nnDeltaAccm * RecScaling(mutualScore, 3);
    }

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
}

public struct MagicCurveParams
{
    public float activationPos;
    public float activationNeg;

    public float activationSteepnessPos;
    public float activationSteepnessNeg;
    
    public float flatnessPos;
    public float flatnessNeg;
}

public struct BumpCurveParams
{
    public float center;
    public float peak;
    public float width;
    public float steepness;
}