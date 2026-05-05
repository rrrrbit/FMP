using NUnit;
using RBitUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class MGR_gameMaths : MonoBehaviour, IGameMaths
{
#region vars
    [Header("Misc")]
    public float timescale = 1;

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
    float[,] nnDelta;
    float[,] niDelta;
    float[,] inNext;
    float[,] iiNext;
    int nodesCount;
    int ideasCount;
    [Header("- Node Stats")]
    public NodeStats[] nodeStats;
	public NodeStats[] nodeStatsDelta;

    public NodeStats nodeStatsMin;
    public NodeStats nodeStatsMax;
    [Header("-- Idea Stats")]
    public float[] ideaComplexity;
    public MagicCurveParams[] ideaTolerance;
    public NodeStats[] ideaExemplar;
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
        float strength;
        if (xRaw >= 0)
        {
            activation = param.activationPos;
            activationSteepness = param.activationSteepnessPos;
            strength = param.strengthPos;
        }
        else
        {
            activation = param.activationNeg;
            activationSteepness = param.activationSteepnessNeg;
            strength = param.strengthNeg;
        }
        
        float x = Mathf.Abs(xRaw);

        float sigmoid = 1 / (1 + Mathf.Exp(-Mathf.Log(99)-4 * activationSteepness * (x - activation)));
        float curve = strength>0? strength - strength * Mathf.Exp(-x / strength):0;
        float total = sigmoid * curve * Mathf.Sign(xRaw);
        if (!float.IsFinite(total))
        {
            Debug.LogWarning("caught NaN in magicCurve");
            return 0;
        }
        return total;
    }

    float BumpCurve(float x, BumpCurveParams param)
    {
        float total = param.peak * Mathf.Exp(-Mathf.Pow(Mathf.Abs((x - param.center) / param.width), param.steepness));
        if (!float.IsFinite(total))
        {
            Debug.LogWarning("caught NaN in bumpCurve");
            return 0;
        }
        return total;
    }

    float ManualSum(int excludeInd, int range, Func<int, float> func)
    {
        float accm = 0;
        for (int i = 0; i < range; i++)
        {
            if (i == excludeInd) continue; // skip self
            var nanCatch = func(i);
            if (!float.IsFinite(nanCatch)) // skip breakages
            {
                Debug.LogWarning("caught NaN in sum at index " + i);
                continue;
            }
            accm += nanCatch;
        }
        return accm;
    }

    MagicCurveParams RandomMagicCurve(MagicCurveParams min, MagicCurveParams max)
    {
        return new MagicCurveParams()
        {
            activationPos = UnityEngine.Random.Range(min.activationPos, max.activationPos),
            activationSteepnessPos = UnityEngine.Random.Range(min.activationSteepnessPos, max.activationSteepnessPos),
            strengthPos = UnityEngine.Random.Range(min.strengthPos, max.strengthPos),

            activationNeg = UnityEngine.Random.Range(min.activationNeg, max.activationNeg),
            activationSteepnessNeg = UnityEngine.Random.Range(min.activationSteepnessNeg, max.activationSteepnessNeg),
            strengthNeg = UnityEngine.Random.Range(min.strengthNeg, max.strengthNeg)
        };
    }

    BumpCurveParams RandomBumpCurve(BumpCurveParams min, BumpCurveParams max)
    {
        return new BumpCurveParams()
        {
            center = UnityEngine.Random.Range(min.center, max.center),
            peak = UnityEngine.Random.Range(min.peak, max.peak),
            width = UnityEngine.Random.Range(min.width, max.width),
            steepness = UnityEngine.Random.Range(min.steepness, max.steepness),
        };
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
        UpdateGraphStatistics();
        Step(Time.deltaTime * timescale);
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
                strengthPos = UnityEngine.Random.Range(min.strengthPos, max.strengthPos),

                activationNeg = UnityEngine.Random.Range(min.activationNeg, max.activationNeg),
                activationSteepnessNeg = UnityEngine.Random.Range(min.activationSteepnessNeg, max.activationSteepnessNeg),
                strengthNeg = UnityEngine.Random.Range(min.strengthNeg, max.strengthNeg)
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

        nodesCount = nodes.Count();
        ideasCount = ideas.Count();
    }

    void InitStats()
    {
        #region default curves
        MagicCurveParams minMagicCurve = new()
        {
            activationPos = 2,
            activationNeg = 2,

            activationSteepnessPos = 1,
            activationSteepnessNeg = 1,

            strengthPos = 0.1f,
            strengthNeg = 0.1f,
        };
        MagicCurveParams maxMagicCurve = new()
        {
            activationPos = 10,
            activationNeg = 10,

            activationSteepnessPos = 1,
            activationSteepnessNeg = 1,

            strengthPos = 2,
            strengthNeg = 2,
        };

        MagicCurveParams socDecMinCurve = new()
        {
            activationPos = 5,
            activationNeg = 5,

            activationSteepnessPos = .5f,
            activationSteepnessNeg = .5f,

            strengthPos = .5f,
            strengthNeg = .5f,
        };
        MagicCurveParams socDecMaxCurve = new()
        {
            activationPos = 10,
            activationNeg = 10,

            activationSteepnessPos = .25f,
            activationSteepnessNeg = .25f,

            strengthPos = 2,
            strengthNeg = 2,
        };

        BumpCurveParams minBumpCurve = new()
        {
            center = -1,
            peak = 1,
            width = 1,
            steepness = 2,
        };
        BumpCurveParams maxBumpCurve = new()
        {
            center = 1,
            peak = 1.5f,
            width = 3,
            steepness = 5,
        };
        #endregion

        // node stats
        nodeStats = new NodeStats[nodesCount];
		nodeStatsDelta = new NodeStats[nodesCount];
		for (int i = 0; i < nodesCount; i++)
        {
            nodeStats[i] = new NodeStats()
            {
                complexity = UnityEngine.Random.Range(.5f, 1),
                complexityTolerance = RandomBumpCurve(minBumpCurve, maxBumpCurve),
                enthusiasm = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                reach = UnityEngine.Random.Range(.5f, 4),
                suggestibility = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                adherence = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                socialAttention = RandomMagicCurve(socDecMinCurve, socDecMaxCurve),
            };
        }

        // idea stats
        InitFloatArr(ref ideaComplexity, startingNumberIdeas, .5f, 1);
        InitMagicCurves(ref ideaTolerance, startingNumberIdeas, minMagicCurve, maxMagicCurve);

        ideaExemplar = new NodeStats[ideasCount];
        for (int i = 0; i < ideasCount; i++)
        {
            ideaExemplar[i] = new NodeStats()
            {
                complexity = UnityEngine.Random.Range(.5f, 1),
                complexityTolerance = RandomBumpCurve(minBumpCurve, maxBumpCurve),
                enthusiasm = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                reach = UnityEngine.Random.Range(.5f, 4),
                suggestibility = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                adherence = RandomMagicCurve(minMagicCurve, maxMagicCurve),
                socialAttention = RandomMagicCurve(socDecMinCurve, socDecMaxCurve),
            };
        }
    }

    void InitMtx()
    {
        

        // initialise nn with random weights
        NN = new AdjacencyMtx(nodes, nodes);
        nnDelta = new float[nodesCount, nodesCount];
        for (int i = 0; i < nodesCount; i++)
        {
            for (int j = 0; j < nodesCount; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                NN.mtx[i, j] = 0;//Mathf.Pow(x, 11) /10f;
            }
        }
        NN.mtx[0, 1] = 1;

        // initialise ni with random weights
        NI = new AdjacencyMtx(ideas, nodes);
        niDelta = new float[nodesCount, ideasCount];
        for (int i = 0; i < nodesCount; i++)
        {
            for (int j = 0; j < ideasCount; j++)
            {
                float x = UnityEngine.Random.value * 2 - 1;
                NI.mtx[i, j] = x;
            }
        }

        // initialise in to 0s
        IN = new AdjacencyMtx(nodes, ideas);
        inNext = new float[ideasCount, nodesCount];
        for (int i = 0; i < ideasCount; i++)
        {
            for (int j = 0; j < nodesCount; j++)
            {
                IN.mtx[i, j] = 0;
            }
        }

        // initialise ii with random weights
        II = new AdjacencyMtx(ideas, ideas);
        for (int i = 0; i < ideasCount; i++)
        {
            for (int j = 0; j < ideasCount; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                II.mtx[i, j] = x;
            }
        }
    }

    #endregion

#region calculations
    void Step(float dt)
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
                niDelta[n, i] = CalcDeltaNI(n, i);
            }
        }

        // nn
        for (int a = 0; a < nodes.Count; a++)
        {
            for (int b = 0; b < nodes.Count; b++)
            {
                if (a == b) continue;
                nnDelta[a, b] = CalcDeltaNN(a, b);
            }
        }

        // stats
        for (int n = 0; n < nodesCount; n++)
        {
			CalcDeltaStats(n);
        }

		// update all
        IN.mtx = inNext;

        for (int i = 0; i < NI.mtx.Rows(); i++)
        {
            for (int j = 0; j < NI.mtx.Cols(); j++)
            {
                NI.mtx[i, j] += niDelta[i, j] * dt;
            }
        }

        for (int i = 0; i < NN.mtx.Rows(); i++)
        {
            for (int j = 0; j < NN.mtx.Cols(); j++)
            {
                NN.mtx[i, j] += nnDelta[i, j] * dt;
            }
        }

		for (int n = 0; n < nodesCount; n++)
		{
			nodeStats[n] += nodeStatsDelta[n] * dt;
            nodeStats[n] = ClampStats(nodeStats[n], nodeStatsMin, nodeStatsMax);
		}
	}
    NodeStats ClampStats(NodeStats stats, NodeStats min, NodeStats max)
    {
        float ClampFloatStat(Func<NodeStats, float> stat) => Mathf.Clamp(stat(stats), stat(min), stat(max));
        MagicCurveParams ClampMagicCurveStat(Func<NodeStats, MagicCurveParams> stat)
        {
            float ClampMagicCurveParam(Func<MagicCurveParams, float> param) => Mathf.Clamp(param(stat(stats)), param(stat(min)), param(stat(max)));
            return new()
            {
                activationNeg = ClampMagicCurveParam(x => x.activationNeg),
                activationPos = ClampMagicCurveParam(x => x.activationPos),
                activationSteepnessNeg = ClampMagicCurveParam(x => x.activationSteepnessNeg),
                activationSteepnessPos = ClampMagicCurveParam(x => x.activationSteepnessPos),
                strengthNeg = ClampMagicCurveParam(x => x.strengthNeg),
                strengthPos = ClampMagicCurveParam(x => x.strengthPos),
            };
        }

        BumpCurveParams ClampBumpCurveStat(Func<NodeStats, BumpCurveParams> stat)
        {
            float ClampBumpCurveParam(Func<BumpCurveParams, float> param) => Mathf.Clamp(param(stat(stats)), param(stat(min)), param(stat(max)));
            return new()
            {
                center = ClampBumpCurveParam(x => x.center),
                peak = ClampBumpCurveParam(x => x.peak),
                width = ClampBumpCurveParam(x => x.width),
                steepness = ClampBumpCurveParam(x => x.steepness),
            };
        }
        
        NodeStats clamped = new NodeStats()
        {
            complexity = ClampFloatStat(x => x.complexity),
            complexityTolerance = ClampBumpCurveStat(x => x.complexityTolerance),
            enthusiasm = ClampMagicCurveStat(x => x.enthusiasm),
            reach = ClampFloatStat(x => x.reach),
            suggestibility = ClampMagicCurveStat(x => x.suggestibility),
            adherence = ClampMagicCurveStat(x => x.adherence),
            socialAttention = ClampMagicCurveStat(x => x.socialAttention),
        };

        return clamped;
    }
    
	float CalcDeltaStat(int n, Func<NodeStats, float> stat)
	{
		float social = ManualSum(n, nodesCount, x =>
			(stat(nodeStats[x]) - stat(nodeStats[n])) * (stat(nodeStats[x]) - stat(nodeStats[n])) * NN.mtx[n, x]
			);

		float ideological = ManualSum(-1, ideasCount, x =>
			(stat(ideaExemplar[x]) - stat(nodeStats[n])) * (stat(ideaExemplar[x]) - stat(nodeStats[n])) * NI.mtx[n, x]
			);

		return MagicCurve(social, nodeStats[n].suggestibility) + MagicCurve(ideological, nodeStats[n].adherence);
	}

	void CalcDeltaStats(int n)
	{
		nodeStatsDelta[n].complexity = CalcDeltaStat(n, x => x.complexity);

		nodeStatsDelta[n].complexityTolerance.steepness = CalcDeltaStat(n, x => x.complexityTolerance.steepness);
		nodeStatsDelta[n].complexityTolerance.width = CalcDeltaStat(n, x => x.complexityTolerance.width);
		nodeStatsDelta[n].complexityTolerance.center = CalcDeltaStat(n, x => x.complexityTolerance.center);
		nodeStatsDelta[n].complexityTolerance.peak =	CalcDeltaStat(n, x => x.complexityTolerance.peak);

		nodeStatsDelta[n].enthusiasm.activationNeg = CalcDeltaStat(n, x => x.enthusiasm.activationNeg);
		nodeStatsDelta[n].enthusiasm.activationPos = CalcDeltaStat(n, x => x.enthusiasm.activationPos);
		nodeStatsDelta[n].enthusiasm.activationSteepnessNeg = CalcDeltaStat(n, x => x.enthusiasm.activationSteepnessNeg);
		nodeStatsDelta[n].enthusiasm.activationSteepnessPos = CalcDeltaStat(n, x => x.enthusiasm.activationSteepnessPos);
		nodeStatsDelta[n].enthusiasm.strengthNeg = CalcDeltaStat(n, x => x.enthusiasm.strengthNeg);
		nodeStatsDelta[n].enthusiasm.strengthPos = CalcDeltaStat(n, x => x.enthusiasm.strengthPos);

		nodeStatsDelta[n].reach = CalcDeltaStat(n, x => x.reach);

		nodeStatsDelta[n].suggestibility.activationNeg = CalcDeltaStat(n, x => x.suggestibility.activationNeg);
		nodeStatsDelta[n].suggestibility.activationPos = CalcDeltaStat(n, x => x.suggestibility.activationPos);
		nodeStatsDelta[n].suggestibility.activationSteepnessNeg = CalcDeltaStat(n, x => x.suggestibility.activationSteepnessNeg);
		nodeStatsDelta[n].suggestibility.activationSteepnessPos = CalcDeltaStat(n, x => x.suggestibility.activationSteepnessPos);
		nodeStatsDelta[n].suggestibility.strengthNeg = CalcDeltaStat(n, x => x.suggestibility.strengthNeg);
		nodeStatsDelta[n].suggestibility.strengthPos = CalcDeltaStat(n, x => x.suggestibility.strengthPos);

		nodeStatsDelta[n].adherence.activationNeg = CalcDeltaStat(n, x => x.adherence.activationNeg);
		nodeStatsDelta[n].adherence.activationPos = CalcDeltaStat(n, x => x.adherence.activationPos);
		nodeStatsDelta[n].adherence.activationSteepnessNeg = CalcDeltaStat(n, x => x.adherence.activationSteepnessNeg);
		nodeStatsDelta[n].adherence.activationSteepnessPos = CalcDeltaStat(n, x => x.adherence.activationSteepnessPos);
		nodeStatsDelta[n].adherence.strengthNeg = CalcDeltaStat(n, x => x.adherence.strengthNeg);
		nodeStatsDelta[n].adherence.strengthPos = CalcDeltaStat(n, x => x.adherence.strengthPos);

		nodeStatsDelta[n].socialAttention.activationNeg = CalcDeltaStat(n, x => x.socialAttention.activationNeg);
		nodeStatsDelta[n].socialAttention.activationPos = CalcDeltaStat(n, x => x.socialAttention.activationPos);
		nodeStatsDelta[n].socialAttention.activationSteepnessNeg = CalcDeltaStat(n, x => x.socialAttention.activationSteepnessNeg);
		nodeStatsDelta[n].socialAttention.activationSteepnessPos = CalcDeltaStat(n, x => x.socialAttention.activationSteepnessPos);
		nodeStatsDelta[n].socialAttention.strengthNeg = CalcDeltaStat(n, x => x.socialAttention.strengthNeg);
		nodeStatsDelta[n].socialAttention.strengthPos = CalcDeltaStat(n, x => x.socialAttention.strengthPos);

		nodeStatsDelta[n] = ClampStats(nodeStatsDelta[n], nodeStatsMin, nodeStatsMax);
	}

    float CalcIN(int i, int n)
    {
        // similarity here
        var agreement = ManualSum(i, ideasCount, x => NI.mtx[n, x] * II.mtx[i, x]);
        return agreement; // + similarity
    }

    float CalcDeltaNI(int n, int i)
    {
        float social = ManualSum(n, nodesCount, x =>
            MagicCurve(NI.mtx[x, i], nodeStats[x].enthusiasm) * NN.mtx[n, x]
            );
        float ideological = ManualSum(i, ideasCount, x =>
            II.mtx[x, i] * NI.mtx[n, x]
            );
        float complexity = BumpCurve(ideaComplexity[i] - nodeStats[n].complexity, nodeStats[n].complexityTolerance);

        return MagicCurve(social, nodeStats[n].suggestibility) + MagicCurve(ideological, nodeStats[n].adherence) * complexity;
    }

    float CalcDeltaNN(int n, int m)
    {
        float social = ManualSum(n, nodesCount, x => NN.mtx[x, m] * NN.mtx[n, x]) + nodeStats[m].reach;
        //float social = ManualSumInd(n, nodes.Count, x =>
        //    MagicCurve(NN.mtx[x, m] * NN.mtx[n, x], nodeSuggestibility[n])
        //    ) + nodeReach[m];
        float ideological = ManualSum(-1, ideasCount, x => IN.mtx[x,m] * NI.mtx[n, x]);

        //float nn2 = NN.mtx[n, m] * NN.mtx[n, m];
        float decay = -MagicCurve(NN.mtx[n, m], nodeStats[n].socialAttention);

        return MagicCurve(social, nodeStats[n].suggestibility) * MagicCurve(ideological, nodeStats[n].adherence) + decay;
    }

	void UpdateGraphStatistics()
	{
		max = NN.maxWeight;
		min = NN.minWeight;
		maxAbs = NN.maxAbsWeight;
		sumAbs = NN.sumAbsWeight;
		maxOutdegree = NN.maxIndegree;
        debugFlatMtx = NN.FlatMtx().ToList();
	}

    #endregion

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
    public float maxIndegree;

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
        maxIndegree = nodes.Max(x => GetIndegree(x)); // !!!!!!!! change indegree visualisation to use geometric mean as it better ignores few, strong connections and emphasises many connections.
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
    
    public float strengthPos;
    public float strengthNeg;

    public static MagicCurveParams operator +(MagicCurveParams a, MagicCurveParams b)
    {
        return new()
        {
            activationNeg = a.activationNeg + b.activationNeg,
            activationPos = a.activationPos + b.activationPos,
            activationSteepnessNeg = a.activationSteepnessNeg + b.activationSteepnessNeg,
            activationSteepnessPos = a.activationSteepnessPos + b.activationSteepnessPos,
            strengthNeg = a.strengthPos + b.strengthNeg,
            strengthPos = a.strengthPos + b.strengthPos,
        };
    }

	public static MagicCurveParams operator *(MagicCurveParams a, float b)
	{
		return new()
		{
			activationNeg = a.activationNeg * b,
			activationPos = a.activationPos * b,
			activationSteepnessNeg = a.activationSteepnessNeg * b,
			activationSteepnessPos = a.activationSteepnessPos * b,
			strengthNeg = a.strengthPos * b,
			strengthPos = a.strengthPos * b,
		};
	}
}

[Serializable]
public struct BumpCurveParams
{
    public float center;
    public float peak;
    public float width;
    public float steepness;
	public static BumpCurveParams operator +(BumpCurveParams a, BumpCurveParams b)
	{
		return new()
		{
			center = a.center + b.center,
			peak = a.peak + b.peak,
			width = a.width + b.width,
			steepness = a.steepness + b.steepness,
		};
	}

	public static BumpCurveParams operator *(BumpCurveParams a, float b)
	{
		return new()
		{
			center = a.center * b,
			peak = a.peak * b,
			width = a.width * b,
			steepness = a.steepness * b,
		};
	}
}

[Serializable]
public struct NodeStats
{
    public float complexity;
    public BumpCurveParams complexityTolerance;
    public MagicCurveParams enthusiasm;
    public float reach;
    public MagicCurveParams suggestibility; // rename conformity...?
    public MagicCurveParams adherence;
    public MagicCurveParams socialAttention;

	public static NodeStats operator +(NodeStats a, NodeStats b)
	{
		return new()
		{
			complexity = a.complexity + b.complexity,
			complexityTolerance = a.complexityTolerance + b.complexityTolerance,
			enthusiasm = a.enthusiasm + b.enthusiasm,
			reach = a.reach + b.reach,
			suggestibility = a.suggestibility + b.suggestibility,
			adherence = a.adherence + b.adherence,
			socialAttention = a.socialAttention + b.socialAttention,
		};
	}

	public static NodeStats operator *(NodeStats a, float b)
	{
		return new()
		{
			complexity = a.complexity * b,
			complexityTolerance = a.complexityTolerance * b,
			enthusiasm = a.enthusiasm * b,
			reach = a.reach * b,
			suggestibility = a.suggestibility * b,
			adherence = a.adherence * b,
			socialAttention = a.socialAttention * b,
		};
	}
}