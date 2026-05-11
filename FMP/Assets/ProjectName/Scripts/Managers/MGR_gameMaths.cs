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
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class MGR_gameMaths : MonoBehaviour
{
#region vars
    [Header("Misc")]
    public float timescale = 1;

    public int startingNumberPeople;
	public int startingNumberIdeas;
	public event System.Action OnReadyForVisualisation;
	[Header("Runtime & Refs")]
    [Header("- Lists")]
	public List<PersonNode> nodes;
    public List<IdeaNode> ideas;
    public int nodesCount;
    public int ideasCount;
    [Header("- Matrices")]
	public float[,] NN;
    public float[,] NI;
    public float[,] IN;
    public float[,] II;
	public MtxStats statsNN;
	public MtxStats statsNI;
	public MtxStats statsIN;
	public MtxStats statsII;
	float[,] nnDelta;
    float[,] niDelta;
    float[,] inNext;
    float[,] iiNext;

	[Serializable]
    public struct NodeStats
    {
        public float complexity;
        public BumpCurveParams complexityTolerance;
        public MagicCurveParams enthusiasm;
        public float reach;
        public MagicCurveParams suggestibility; // rename conformity...?
        public MagicCurveParams adherence;
        public float extroversion;
        public float avoidance;

        public static NodeStats operator +(NodeStats a, NodeStats b) => new()
        {
            complexity = a.complexity + b.complexity,
            complexityTolerance = a.complexityTolerance + b.complexityTolerance,
            enthusiasm = a.enthusiasm + b.enthusiasm,
            reach = a.reach + b.reach,
            suggestibility = a.suggestibility + b.suggestibility,
            adherence = a.adherence + b.adherence,
            extroversion = a.extroversion + b.extroversion,
            avoidance = a.avoidance + b.avoidance,
        };
        public static NodeStats operator *(NodeStats a, float b) => new()
        {
            complexity = a.complexity * b,
            complexityTolerance = a.complexityTolerance * b,
            enthusiasm = a.enthusiasm * b,
            reach = a.reach * b,
            suggestibility = a.suggestibility * b,
            adherence = a.adherence * b,
            extroversion = a.extroversion * b,
            avoidance = a.avoidance * b,
        };
    }
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
    [Serializable]
    public struct MagicCurveParams
    {
        public float thresholdPos;
        public float thresholdNeg;

        public float strengthPos;
        public float strengthNeg;

        public static MagicCurveParams operator +(MagicCurveParams a, MagicCurveParams b) => new()
        {
            thresholdNeg = a.thresholdNeg + b.thresholdNeg,
            thresholdPos = a.thresholdPos + b.thresholdPos,
            strengthNeg = a.strengthPos + b.strengthNeg,
            strengthPos = a.strengthPos + b.strengthPos,
        };

        public static MagicCurveParams operator *(MagicCurveParams a, float b) => new()
        {
            thresholdPos = a.thresholdPos * b,
            thresholdNeg = a.thresholdNeg * b,
            strengthNeg = a.strengthPos * b,
            strengthPos = a.strengthPos * b,
        };
    }
    [Serializable]
    public struct BumpCurveParams
    {
        public float center;
        public float peak;
        public float width;
        public float steepness;
        public static BumpCurveParams operator +(BumpCurveParams a, BumpCurveParams b) => new()
        {
            center = a.center + b.center,
            peak = a.peak + b.peak,
            width = a.width + b.width,
            steepness = a.steepness + b.steepness,
        };
        public static BumpCurveParams operator *(BumpCurveParams a, float b) => new()
        {
            center = a.center * b,
            peak = a.peak * b,
            width = a.width * b,
            steepness = a.steepness * b,
        };
    }
    /// <summary>
    /// Parametric f(x) with an optional threshold and asymmetric shape. <a href="https://www.desmos.com/calculator/ygh3492ofo">See demo.</a>
    /// </summary>
    /// <param name="xRaw"></param>
    /// <param name="activation">value of x where threshold is roughly fully open (0.99 hard coded).</param>
    /// <param name="steepness">Maximum gradient of threshold</param>
    /// <returns></returns>
    float MagicCurve(float xRaw, MagicCurveParams param)
    {
        float strength;
        float threshold;
        if (xRaw >= 0)
        {
            strength = param.strengthPos;
            threshold = param.thresholdPos;
        }
        else
        {
            strength = param.strengthNeg;
            threshold = param.thresholdNeg;
        }
        
        float x = Mathf.Abs(xRaw);
        float x2 = x * x;

        float curve = strength > 0 ? strength - strength * Mathf.Exp(-x / strength) : 0;
        float activation =  x < threshold ? -x2 / (2*threshold*x - 2*x2 - threshold*threshold) : 1;

        float total = activation * curve * Mathf.Sign(xRaw);
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
            thresholdPos = UnityEngine.Random.Range(min.thresholdPos, max.thresholdPos),
            strengthPos = UnityEngine.Random.Range(min.strengthPos, max.strengthPos),

            thresholdNeg = UnityEngine.Random.Range(min.thresholdNeg, max.thresholdNeg),
            strengthNeg = UnityEngine.Random.Range(min.strengthNeg, max.strengthNeg),
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
        UpdateMtxStats();
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
            x[i] = RandomMagicCurve(min,max);
        }
    }
    void InitBumpCurves(ref BumpCurveParams[] x, int length, BumpCurveParams min, BumpCurveParams max)
    {
        x = new BumpCurveParams[length];
        for (int i = 0; i < length; i++)
        {
            x[i] = RandomBumpCurve(min,max);
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
            thresholdPos = 2,
            thresholdNeg = 2,

            strengthPos = 0.1f,
            strengthNeg = 0.1f,
        };
        MagicCurveParams maxMagicCurve = new()
        {
            thresholdPos = 10,
            thresholdNeg = 10,

            strengthPos = 2,
            strengthNeg = 2,
        };

        MagicCurveParams socDecMinCurve = new()
        {
            thresholdPos = 5,
            thresholdNeg = 5,

            strengthPos = .5f,
            strengthNeg = .5f,
        };
        MagicCurveParams socDecMaxCurve = new()
        {
            thresholdPos = 20,
            thresholdNeg = 20,

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
                extroversion = UnityEngine.Random.Range(.5f, 4),
                avoidance = UnityEngine.Random.Range(.5f, 4),
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
                extroversion = UnityEngine.Random.Range(.5f, 4),
                avoidance = UnityEngine.Random.Range(.5f, 4),
            };
        }
    }

    void InitMtx()
    {
        

        // initialise nn with random weights
        NN = new float[nodesCount, nodesCount];
        nnDelta = new float[nodesCount, nodesCount];
        for (int i = 0; i < nodesCount; i++)
        {
            for (int j = 0; j < nodesCount; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                NN[i, j] = 0;//Mathf.Pow(x, 11) /10f;
            }
        }
        NN[0, 1] = 1;

        // initialise ni with random weights
        NI = new float[nodesCount, ideasCount];
        niDelta = new float[nodesCount, ideasCount];
        for (int i = 0; i < nodesCount; i++)
        {
            for (int j = 0; j < ideasCount; j++)
            {
                float x = UnityEngine.Random.value * 2 - 1;
                NI[i, j] = x;
            }
        }

        // initialise in to 0s
        IN = new float[ideasCount, nodesCount];
        inNext = new float[ideasCount, nodesCount];
        for (int i = 0; i < ideasCount; i++)
        {
            for (int j = 0; j < nodesCount; j++)
            {
                IN[i, j] = 0;
            }
        }

        // initialise ii with random weights
        II = new float[ideasCount, ideasCount];
        for (int i = 0; i < ideasCount; i++)
        {
            for (int j = 0; j < ideasCount; j++)
            {
                if (i == j) continue; // skip self-connections
                float x = UnityEngine.Random.value * 2 - 1;
                II[i, j] = x;
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
        IN = inNext;

        for (int i = 0; i < NI.Rows(); i++)
        {
            for (int j = 0; j < NI.Cols(); j++)
            {
                NI[i, j] += niDelta[i, j] * dt;
            }
        }

        for (int i = 0; i < NN.Rows(); i++)
        {
            for (int j = 0; j < NN.Cols(); j++)
            {
                NN[i, j] += nnDelta[i, j] * dt;
            }
        }

		for (int n = 0; n < nodesCount; n++)
		{
			nodeStats[n] += nodeStatsDelta[n] * dt;
            nodeStats[n] = ClampStats(nodeStats[n], nodeStatsMin, nodeStatsMax);
		}
	}

#region - stat
    NodeStats ClampStats(NodeStats stats, NodeStats min, NodeStats max)
    {
        float ClampFloatStat(Func<NodeStats, float> stat) => Mathf.Clamp(stat(stats), stat(min), stat(max));
        MagicCurveParams ClampMagicCurveStat(Func<NodeStats, MagicCurveParams> stat)
        {
            float ClampMagicCurveParam(Func<MagicCurveParams, float> param) => Mathf.Clamp(param(stat(stats)), param(stat(min)), param(stat(max)));
            return new()
            {
                thresholdNeg = ClampMagicCurveParam(x => x.thresholdNeg),
                thresholdPos = ClampMagicCurveParam(x => x.thresholdPos),
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
            extroversion = ClampFloatStat(x => x.extroversion),
            avoidance = ClampFloatStat(x => x.avoidance),
        };

        return clamped;
    }
    
	float CalcDeltaStat(int n, Func<NodeStats, float> stat)
	{
		float social = ManualSum(n, nodesCount, x =>
			(stat(nodeStats[x]) - stat(nodeStats[n])) * (stat(nodeStats[x]) - stat(nodeStats[n])) * NN[n, x]
			);

		float ideological = ManualSum(-1, ideasCount, x =>
			(stat(ideaExemplar[x]) - stat(nodeStats[n])) * (stat(ideaExemplar[x]) - stat(nodeStats[n])) * NI[n, x]
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

        nodeStatsDelta[n].enthusiasm.thresholdNeg = CalcDeltaStat(n, x => x.enthusiasm.thresholdNeg);
        nodeStatsDelta[n].enthusiasm.thresholdPos = CalcDeltaStat(n, x => x.enthusiasm.thresholdPos);
		nodeStatsDelta[n].enthusiasm.strengthNeg = CalcDeltaStat(n, x => x.enthusiasm.strengthNeg);
		nodeStatsDelta[n].enthusiasm.strengthPos = CalcDeltaStat(n, x => x.enthusiasm.strengthPos);

		nodeStatsDelta[n].reach = CalcDeltaStat(n, x => x.reach);

        nodeStatsDelta[n].suggestibility.thresholdNeg = CalcDeltaStat(n, x => x.suggestibility.thresholdNeg);
        nodeStatsDelta[n].suggestibility.thresholdPos = CalcDeltaStat(n, x => x.suggestibility.thresholdPos);
		nodeStatsDelta[n].suggestibility.strengthNeg = CalcDeltaStat(n, x => x.suggestibility.strengthNeg);
		nodeStatsDelta[n].suggestibility.strengthPos = CalcDeltaStat(n, x => x.suggestibility.strengthPos);

        nodeStatsDelta[n].adherence.thresholdNeg = CalcDeltaStat(n, x => x.adherence.thresholdNeg);
        nodeStatsDelta[n].adherence.thresholdPos = CalcDeltaStat(n, x => x.adherence.thresholdPos);
		nodeStatsDelta[n].adherence.strengthNeg = CalcDeltaStat(n, x => x.adherence.strengthNeg);
		nodeStatsDelta[n].adherence.strengthPos = CalcDeltaStat(n, x => x.adherence.strengthPos);

        nodeStatsDelta[n].extroversion = CalcDeltaStat(n, x => x.extroversion);
        nodeStatsDelta[n].avoidance = CalcDeltaStat(n, x => x.avoidance);
	}
    #endregion
    float CalcIN(int i, int n)
    {
        // similarity here
        var agreement = ManualSum(i, ideasCount, x => NI[n, x] * II[i, x]);
        return agreement; // + similarity
    }

    float CalcDeltaNI(int n, int i)
    {
        float social = ManualSum(n, nodesCount, x =>
            MagicCurve(NI[x, i], nodeStats[x].enthusiasm) * NN[n, x]
            );
        float ideological = ManualSum(i, ideasCount, x =>
            II[x, i] * NI[n, x]
            );
        float complexity = BumpCurve(ideaComplexity[i] - nodeStats[n].complexity, nodeStats[n].complexityTolerance);

        return MagicCurve(social, nodeStats[n].suggestibility) + MagicCurve(ideological, nodeStats[n].adherence) * complexity;
    }

    float CalcDeltaNN(int n, int m)
    {
        float social = ManualSum(n, nodesCount, x => NN[x, m] * NN[n, x]) + nodeStats[m].reach;
        //float social = ManualSumInd(n, nodes.Count, x =>
        //    MagicCurve(NN.mtx[x, m] * NN.mtx[n, x], nodeSuggestibility[n])
        //    ) + nodeReach[m];
        float ideological = ManualSum(-1, ideasCount, x => IN[x,m] * NI[n, x]);
        //float nn2 = NN[n, m] * NN[n, m];
        float dScaled;
        if (NN[n, m] >= 0)
        {
            dScaled = NN[n, m] / nodeStats[n].extroversion;
        }
        else
        {
            dScaled = NN[n,m] * nodeStats[n].avoidance;
        }


        float decay = - dScaled * dScaled * Mathf.Sign(NN[n, m]);
        return MagicCurve(social, nodeStats[n].suggestibility) * MagicCurve(ideological, nodeStats[n].adherence) + decay;
    }

	void UpdateMtxStats()
	{
		statsNN = NN.GetStats();
		statsNI = NI.GetStats();
		statsIN = IN.GetStats();
		statsII = II.GetStats();
	}

    #endregion

}

[Serializable]
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

public static class MtxUtils
{
	public static int Rows(this float[,] mtx) => mtx.GetLength(0);
	public static int Cols(this float[,] mtx) => mtx.GetLength(1);
	public static Vector2Int Dimensions(this float[,] mtx) => new Vector2Int(mtx.Rows(), mtx.Cols());

	public static float Max(this float[,] mtx, Func<float, float> selector = null)
	{
		float max = float.MinValue;
		foreach (float f in mtx)
		{
			float value = selector != null ? selector(f) : f;
			if (value > max) max = value;
		}
		return max;
	}

	public static float Min(this float[,] mtx, Func<float, float> selector = null)
	{
		float min = float.MaxValue;
		foreach (float f in mtx)
		{
			float value = selector != null ? selector(f) : f;
			if (value < min) min = value;
		}
		return min;
	}

	public static float Sum(this float[,] mtx, Func<float, float> selector = null)
	{
		float sum = 0;
		foreach (float f in mtx)
		{
			sum += selector != null ? selector(f) : f;
		}
		return sum;
	}

	public static float[] Flat(this float[,] mtx)
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

	/// <summary>
	/// Get a row of a matrix as an array.
	/// </summary>
	/// <param name="mtx"></param>
	/// <param name="row"></param>
	/// <returns></returns>
	public static T[] GetRow<T>(this T[,] mtx, int row)
	{
		T[] edges = new T[mtx.Cols()];
		for (int to = 0; to < edges.Length; to++)
		{
			edges[to] = mtx[row, to];
		}
		return edges;
	}

	/// <summary>
	/// Get a row of an adjacency matrix.
	/// </summary>
	/// <param name="mtx"></param>
	/// <param name="row"></param>
	/// <returns></returns>
	public static float[] AllFrom(this float[,] mtx, int row) => mtx.GetRow(row);

	public static T[] GetCol<T>(this T[,] mtx, int col)
	{
		T[] edges = new T[mtx.Rows()];
		for (int from = 0; from < edges.Length; from++)
		{
			edges[from] = mtx[from, col];
		}
		return edges;
	}

	/// <summary>
	/// Get a column of an adjacency matrix.
	/// </summary>
	/// <param name="mtx"></param>
	/// <param name="col"></param>
	/// <returns></returns>
	public static float[] AllTo(this float[,] mtx, int col) => mtx.GetCol(col);

	public static float Indegree(this float[,] mtx, int to)
	{
		float sum = 0;
		for (int from = 0; from < mtx.Rows(); from++)
		{
			sum += Mathf.Abs(mtx[from, to]);
		}
		return sum;
	}

	public static float MaxIndegree(this float[,] mtx)
	{
		float max = float.MinValue;
		for (int to = 0; to < mtx.Cols(); to++)
		{
			float indegree = mtx.Indegree(to);
			if (indegree > max) max = indegree;
		}
		return max;
	}

	public static MtxStats GetStats(this float[,] mtx)
	{
		MtxStats stats = new MtxStats();
		stats.max = mtx.Max();
		stats.min = mtx.Min();
		stats.maxAbs = mtx.Max(x => Mathf.Abs(x));
		stats.sumAbs = mtx.Sum(x => Mathf.Abs(x));
		stats.maxIndegree = mtx.MaxIndegree();
		return stats;
	}
}

[Serializable]
public struct MtxStats
{
	public float max;
	public float min;
	public float maxAbs;
	public float sumAbs;
	public float maxIndegree;
}