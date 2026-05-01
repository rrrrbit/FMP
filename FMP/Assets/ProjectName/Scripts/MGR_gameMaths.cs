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
    float[,] nnNext;
    float[,] niNext;
    float[,] inNext;
    float[,] iiNext;
    int nodesCount;
    int ideasCount;
    [Header("- Node Stats")]
    public NodeStats[] nodeStats;
    public NodeStats[] nodeTargetStats;
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
        float curve = flatness == 0 ? x : Mathf.Log(flatness * x + 1)/flatness;

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
            flatnessPos = UnityEngine.Random.Range(min.flatnessPos, max.flatnessPos),

            activationNeg = UnityEngine.Random.Range(min.activationNeg, max.activationNeg),
            activationSteepnessNeg = UnityEngine.Random.Range(min.activationSteepnessNeg, max.activationSteepnessNeg),
            flatnessNeg = UnityEngine.Random.Range(min.flatnessNeg, max.flatnessNeg)
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
        UpdateStatistics();
        StepWithNext(Time.deltaTime * timescale);
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

        MagicCurveParams socDecMinCurve = new()
        {
            activationPos = 5,
            activationNeg = 5,

            activationSteepnessPos = .5f,
            activationSteepnessNeg = .5f,

            flatnessPos = .5f,
            flatnessNeg = .5f,
        };
        MagicCurveParams socDecMaxCurve = new()
        {
            activationPos = 10,
            activationNeg = 10,

            activationSteepnessPos = .25f,
            activationSteepnessNeg = .25f,

            flatnessPos = 2,
            flatnessNeg = 2,
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
        nodeTargetStats = new NodeStats[nodesCount];
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
            nodeTargetStats[i] = nodeStats[i];
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
        nnNext = new float[nodesCount, nodesCount];
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
        niNext = new float[nodesCount, ideasCount];
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

    void Step(float dt)
    {
        // weights are read for calculations and also written in the same pass.
        // same issue as graph view physics, read and write should be done in seperate passes.
        // but might tank performance. fix l8r

        // in
        for (int i = 0; i < ideasCount; i++)
        {
            for (int n = 0; n < nodesCount; n++)
            {
                IN.mtx[i, n] = CalcIN(i, n);
            }
        }

        // ni
        for (int n = 0; n < nodesCount; n++)
        {
            for (int i = 0; i < ideasCount; i++)
            {
                NI.mtx[n, i] += CalcDeltaNI(n, i) * dt;
            }
        }

        // nn
        for (int a = 0; a < nodesCount; a++)
        {
            for (int b = 0; b < nodesCount; b++)
            {
                if (a == b) continue;
                NN.mtx[a, b] += CalcDeltaNN(a, b) * dt;
            }
        }

        // update stats
        for (int n = 0; n < nodesCount; n++)
        {
            UpdateTargetStats(n);
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
        for (int n = 0; n < nodesCount; n++)
        {
            UpdateTargetStats(n);
            UpdateStats(n, dt);
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

    void UpdateTargetStats(int n)
    {
        float[] mappedNI = new float[ideasCount];
        float[] mappedNN = new float[nodesCount];
        for (int i = 0; i < ideasCount; i++)
        {
            mappedNI[i] = MagicCurve(NI.mtx[n, i], nodeStats[n].adherence);
        }
        for (int m = 0; m < nodesCount; m++)
        {
            mappedNN[m] = MagicCurve(NN.mtx[n, m], nodeStats[n].suggestibility);
        }

        float totalIdeasWeight = ManualSum(-1, ideasCount, x => mappedNI[x]);
        float totalNodesWeight = ManualSum(n, nodesCount, x => mappedNN[x]);
        float totalWeight = totalIdeasWeight + totalNodesWeight + 1;

        float WeightAvStat(Func<NodeStats, float> stat)
        {
            float sumIdeaExmplr = ManualSum(-1, ideasCount, x => stat(ideaExemplar[x]) * mappedNI[x]);
            float sumNodes = ManualSum(n, nodesCount, x => stat(nodeStats[x]) * mappedNN[x]);

            return (sumIdeaExmplr + sumNodes + stat(nodeStats[n]))/(totalWeight);
        }

        nodeTargetStats[n] = new()  
        {
            complexity = WeightAvStat(x => x.complexity),
            complexityTolerance = new BumpCurveParams()
            {
                center = WeightAvStat(x => x.complexityTolerance.center),
                peak = WeightAvStat(x => x.complexityTolerance.peak),
                steepness = WeightAvStat(x => x.complexityTolerance.steepness),
                width = WeightAvStat(x => x.complexityTolerance.peak),
            },
            enthusiasm = new MagicCurveParams()
            {
                activationNeg = WeightAvStat(x => x.enthusiasm.activationNeg),
                activationPos = WeightAvStat(x => x.enthusiasm.activationPos),
                activationSteepnessNeg = WeightAvStat(x => x.enthusiasm.activationSteepnessNeg),
                activationSteepnessPos = WeightAvStat(x => x.enthusiasm.activationSteepnessPos),
                flatnessNeg = WeightAvStat(x => x.enthusiasm.flatnessNeg),
                flatnessPos = WeightAvStat(x => x.enthusiasm.flatnessPos),
            },
            reach = WeightAvStat(x => x.reach),
            suggestibility = new MagicCurveParams()
            {
                activationNeg = WeightAvStat(x => x.suggestibility.activationNeg),
                activationPos = WeightAvStat(x => x.suggestibility.activationPos),
                activationSteepnessNeg = WeightAvStat(x => x.suggestibility.activationSteepnessNeg),
                activationSteepnessPos = WeightAvStat(x => x.suggestibility.activationSteepnessPos),
                flatnessNeg = WeightAvStat(x => x.suggestibility.flatnessNeg),
                flatnessPos = WeightAvStat(x => x.suggestibility.flatnessPos),
            },
            adherence = new MagicCurveParams()
            {
                activationNeg = WeightAvStat(x => x.adherence.activationNeg),
                activationPos = WeightAvStat(x => x.adherence.activationPos),
                activationSteepnessNeg = WeightAvStat(x => x.adherence.activationSteepnessNeg),
                activationSteepnessPos = WeightAvStat(x => x.adherence.activationSteepnessPos),
                flatnessNeg = WeightAvStat(x => x.adherence.flatnessNeg),
                flatnessPos = WeightAvStat(x => x.adherence.flatnessPos),
            },
            socialAttention = new MagicCurveParams()
            {
                activationNeg = WeightAvStat(x => x.socialAttention.activationNeg),
                activationPos = WeightAvStat(x => x.socialAttention.activationPos),
                activationSteepnessNeg = WeightAvStat(x => x.socialAttention.activationSteepnessNeg),
                activationSteepnessPos = WeightAvStat(x => x.socialAttention.activationSteepnessPos),
                flatnessNeg = WeightAvStat(x => x.socialAttention.flatnessNeg),
                flatnessPos = WeightAvStat(x => x.socialAttention.flatnessPos),
            }
        };
    }
    void UpdateStats(int n, float dt)
    {
        float CalcDelta(Func<NodeStats, float> stat)
        {
            float d = (stat(nodeTargetStats[n]) - stat(nodeStats[n])); // fucked
            return d * d * -Mathf.Sign(d) / 2 * dt;
        }
        nodeStats[n].complexity += CalcDelta(x => x.complexity);

        nodeStats[n].complexityTolerance.steepness += CalcDelta(x => x.complexityTolerance.steepness);
        nodeStats[n].complexityTolerance.width += CalcDelta(x => x.complexityTolerance.width);
        nodeStats[n].complexityTolerance.center += CalcDelta(x => x.complexityTolerance.center);
        nodeStats[n].complexityTolerance.peak += CalcDelta(x => x.complexityTolerance.peak);

        nodeStats[n].enthusiasm.activationNeg += CalcDelta(x => x.enthusiasm.activationNeg);
        nodeStats[n].enthusiasm.activationPos += CalcDelta(x => x.enthusiasm.activationPos);
        nodeStats[n].enthusiasm.activationSteepnessNeg += CalcDelta(x => x.enthusiasm.activationSteepnessNeg);
        nodeStats[n].enthusiasm.activationSteepnessPos += CalcDelta(x => x.enthusiasm.activationSteepnessPos);
        nodeStats[n].enthusiasm.flatnessNeg += CalcDelta(x => x.enthusiasm.flatnessNeg);
        nodeStats[n].enthusiasm.flatnessPos += CalcDelta(x => x.enthusiasm.flatnessPos);

        nodeStats[n].reach += CalcDelta(x => x.reach);

        nodeStats[n].suggestibility.activationNeg += CalcDelta(x => x.suggestibility.activationNeg);
        nodeStats[n].suggestibility.activationPos += CalcDelta(x => x.suggestibility.activationPos);
        nodeStats[n].suggestibility.activationSteepnessNeg += CalcDelta(x => x.suggestibility.activationSteepnessNeg);
        nodeStats[n].suggestibility.activationSteepnessPos += CalcDelta(x => x.suggestibility.activationSteepnessPos);
        nodeStats[n].suggestibility.flatnessNeg += CalcDelta(x => x.suggestibility.flatnessNeg);
        nodeStats[n].suggestibility.flatnessPos += CalcDelta(x => x.suggestibility.flatnessPos);

        nodeStats[n].adherence.activationNeg += CalcDelta(x => x.adherence.activationNeg);
        nodeStats[n].adherence.activationPos += CalcDelta(x => x.adherence.activationPos);
        nodeStats[n].adherence.activationSteepnessNeg += CalcDelta(x => x.adherence.activationSteepnessNeg);
        nodeStats[n].adherence.activationSteepnessPos += CalcDelta(x => x.adherence.activationSteepnessPos);
        nodeStats[n].adherence.flatnessNeg += CalcDelta(x => x.adherence.flatnessNeg);
        nodeStats[n].adherence.flatnessPos += CalcDelta(x => x.adherence.flatnessPos);

        nodeStats[n].socialAttention.activationNeg += CalcDelta(x => x.socialAttention.activationNeg);
        nodeStats[n].socialAttention.activationPos += CalcDelta(x => x.socialAttention.activationPos);
        nodeStats[n].socialAttention.activationSteepnessNeg += CalcDelta(x => x.socialAttention.activationSteepnessNeg);
        nodeStats[n].socialAttention.activationSteepnessPos += CalcDelta(x => x.socialAttention.activationSteepnessPos);
        nodeStats[n].socialAttention.flatnessNeg += CalcDelta(x => x.socialAttention.flatnessNeg);
        nodeStats[n].socialAttention.flatnessPos += CalcDelta(x => x.socialAttention.flatnessPos);
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

	void UpdateStatistics()
	{
		max = NN.maxWeight;
		min = NN.minWeight;
		maxAbs = NN.maxAbsWeight;
		sumAbs = NN.sumAbsWeight;
		maxOutdegree = NN.maxIndegree;
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
}