using NUnit;
using RBitUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class MGR_gameMaths : MonoBehaviour, IGameMaths
{
    #region vars
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
	public AdjacencyMtx NN;
    public AdjacencyMtx NI;
    public AdjacencyMtx IN;
    public AdjacencyMtx II;
    [Header("-- Internal")]
    float[,] nnNext;
    float[,] niNext;
    float[,] inNext;
    float[,] iiNext;
    [Header("- Node Stats")]
    public float[] nodeComplexity;
    public BumpCurveParams[] nodeComplexityTolerance;
    public MagicCurveParams[] nodeEnthusiasm;
    public float[] nodeReach;
    public MagicCurveParams[] nodeSuggestibility; // rename conformity...?
    public MagicCurveParams[] nodeAdherence;
    [Header("-- Idea Stats")]
    public float[] ideaComplexity;
    public MagicCurveParams[] ideaTolerance;
    [Header("debug")]
	public TextMeshProUGUI debugText;
    public List<float> debugFlatMtx;
    #endregion

    #region utilities
    /// <summary>
    /// Parametric f(x) with an optional threshold and asymmetric shape. <a href="https://www.desmos.com/calculator/ygh3492ofo">See demo.</a>
    /// </summary>
    /// <param name="xRaw"></param>
    /// <param name="activation">value of x where threshold is roughly fully open (0.99 hard coded).</param>
    /// <param name="steepness">Maximum gradient of threshold</param>
    /// <returns></returns>
    float MagicCurve(float xRaw, MagicCurveParams param)
    {
        float activation;
        float activationSteepness;
        float flatness;
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

    float ManualSum(int excludeInd, int range, Func<int, float> func)
    {
        float accm = 0;
        for (int i = 0; i < range; i++)
        {
            if (i == excludeInd) continue; // skip self
            var nanCatch = func(i);
            if (!float.IsFinite(nanCatch)) continue; // skip breakages
            accm += nanCatch;
        }
        return accm;
    }
    #endregion

    #region main
    private void Start()
    {
        InitLists();
        InitStats();
        InitMtx();

        OnReadyForVisualisation?.Invoke();
    }

    private void Update()
    {
        NN.RecalculateStats();
        UpdateStatistics();
        StepWithNext(Time.deltaTime);
    }
    #endregion

    #region init procs
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

    void InitLists()
    {
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
    }

    void InitStats()
    {
        MagicCurveParams minMagicCurve = new()
        {
            activationPos = 2,
            activationNeg = 2,

            activationSteepnessPos = 1,
            activationSteepnessNeg = 1,

            flatnessPos = 10,
            flatnessNeg = 10,
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
    }

    void InitMtx()
    {
        // initialise nn with random weights
        NN = new AdjacencyMtx(nodes, nodes);
        nnNext = new float[nodes.Count(), nodes.Count()];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                NN.mtx[i, j] = 0;//Mathf.Pow(x, 11) /10f;
            }
        }
        NN.mtx[0, 1] = 1;

        // initialise ni with random weights
        NI = new AdjacencyMtx(ideas, nodes);
        niNext = new float[nodes.Count(), ideas.Count()];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < ideas.Count; j++)
            {
                float x = UnityEngine.Random.value * 2 - 1;
                NI.mtx[i, j] = x;
            }
        }

        // initialise in to 0s
        IN = new AdjacencyMtx(nodes, ideas);
        inNext = new float[ideas.Count(), nodes.Count()];
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                IN.mtx[i, j] = 0;
            }
        }

        // initialise ii with random weights
        II = new AdjacencyMtx(ideas, ideas);
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int j = 0; j < ideas.Count; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                II.mtx[i, j] = x;
            }
        }
    }
    
    #endregion

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
                IN.mtx[i, n] = CalcIN(i, n);
            }
        }

        // ni
        for (int n = 0; n < nodes.Count; n++)
        {
            for (int i = 0; i < ideas.Count; i++)
            {
                NI.mtx[n, i] += CalcDeltaNI(n, i) * dt;
            }
        }

        // nn
        for (int a = 0; a < nodes.Count; a++)
        {
            for (int b = 0; b < nodes.Count; b++)
            {
                if (a == b) continue;
                NN.mtx[a, b] += CalcDeltaNN(a, b) * dt;
            }
        }

        // update stats
        for (int n = 0; n < nodes.Count; n++)
        {
            UpdateStat(n);
        }

        //i_n.mtx = inNext;
        //n_i.mtx = niNext;
        //n_n.mtx = nnNext; as expected does tank.
    }

    void StepWithNext(float dt)
    {
        // in
        for (int i = 0; i < ideas.Count; i++)
        {
            for (int n = 0; n < nodes.Count; n++)
            {
                inNext[i, n] = CalcIN(i, n);
            }
        }

        // ni
        for (int n = 0; n < nodes.Count; n++)
        {
            for (int i = 0; i < ideas.Count; i++)
            {
                niNext[n, i] += CalcDeltaNI(n, i) * dt;
            }
        }

        // nn
        for (int a = 0; a < nodes.Count; a++)
        {
            for (int b = 0; b < nodes.Count; b++)
            {
                if (a == b) continue;
                nnNext[a, b] += CalcDeltaNN(a, b) * dt;
            }
        }

        // update stats
        for (int n = 0; n < nodes.Count; n++)
        {
            UpdateStat(n);
        }

        IN.mtx = inNext;

        for (int i = 0; i < NI.mtx.Rows(); i++)
        {
            for (int j = 0; j < NI.mtx.Cols(); j++)
            {
                NI.mtx[i, j] += niNext[i, j];
            }
        }

        for (int i = 0; i < NN.mtx.Rows(); i++)
        {
            for (int j = 0; j < NN.mtx.Cols(); j++)
            {
                NN.mtx[i, j] += nnNext[i, j];
            }
        }

        nnNext = new float[nodes.Count(), nodes.Count()];
        niNext = new float[nodes.Count(), ideas.Count()];
    }

    void UpdateStat(int n)
    {
        
    }

    float CalcIN(int i, int n)
    {
        // similarity here
        var agreement = ManualSum(i, ideas.Count, x => NI.mtx[n, x] * II.mtx[i, x]);
        return agreement; // + similarity
    }

    float CalcDeltaNI(int n, int i)
    {
        float social = ManualSum(n, nodes.Count, x =>
            MagicCurve(NI.mtx[x, i], nodeEnthusiasm[x]) * NN.mtx[n, x]
            );
        float ideological = ManualSum(i, ideas.Count, x =>
            II.mtx[x, i] * NI.mtx[n, x]
            );
        float complexity = BumpCurve(ideaComplexity[i] - nodeComplexity[n], nodeComplexityTolerance[n]);

        return MagicCurve(social, nodeSuggestibility[n]) + MagicCurve(ideological, nodeAdherence[n]) * complexity;
    }

    float CalcDeltaNN(int n, int m)//problem is here
    {
        float socialRaw = ManualSum(n, nodes.Count, x => NN.mtx[x, m] * NN.mtx[n, x]) + nodeReach[m];
        //float social = ManualSumInd(n, nodes.Count, x =>
        //    MagicCurve(NN.mtx[x, m] * NN.mtx[n, x], nodeSuggestibility[n])
        //    ) + nodeReach[m];
        float ideological = ManualSum(-1, ideas.Count, x => IN.mtx[x,m] * NI.mtx[n, x]);

        return MagicCurve(socialRaw, nodeSuggestibility[n]) * MagicCurve(ideological, nodeAdherence[n]);
    }

	void UpdateStatistics()
	{
		max = NN.maxWeight;
		min = NN.minWeight;
		maxAbs = NN.maxAbsWeight;
		sumAbs = NN.sumAbsWeight;
		maxOutdegree = NN.maxOutdegree;
        debugFlatMtx = NN.FlatMtx().ToList();
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

    //public static implicit operator float[,](AdjacencyMtx adjMtx) => adjMtx.mtx; fuck

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
    
    public float GetOutdegree(int from)
    {
        float sum = 0;
        for (int to = 0; to < mtx.Cols(); to++)
        {
            sum += Mathf.Abs(mtx[from, to]);
        }
        return sum;
    }
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

[Serializable]
public struct MagicCurveParams
{
    public float activationPos;
    public float activationNeg;

    public float activationSteepnessPos;
    public float activationSteepnessNeg;
    
    public float flatnessPos;
    public float flatnessNeg;
}

[Serializable]
public struct BumpCurveParams
{
    public float center;
    public float peak;
    public float width;
    public float steepness;
}