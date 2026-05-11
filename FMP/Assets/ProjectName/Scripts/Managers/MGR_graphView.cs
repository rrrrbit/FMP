using RBitUtils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class MGR_graphView : MonoBehaviour
{
	[Header("Misc")]
	public float lineGap = 0;
	public float pairwiseForceThreshold = 0.01f;
	public AnimationCurve sizeByIndegree;
	[Header("Graphs")]
	public bool showNodes;
	public bool showIdeas;
	public bool applyNN;
	public bool applyNI;
	public bool applyIN;
	public bool applyII;

	[Header("Edge")]
	public EdgeDrawer edgeDrawerPrefab;
	public Dictionary<float[,], EdgeDrawer> edgeDrawers;

    [Header("Forces")]
	public float padding = 10f;
	public bool useScale = true;
	public bool normaliseWeights = true;
	public bool symmetriseWeights = true;
	public float maxVel = 1000f;
	[Space]
	public float centeringStrength;
	public float dragStrength;
	public float clusterStrength = 1;
	[Space]
	public float attractionStrength = 2;
	public AnimationCurve attractionByWeight;
	public enum AttractionTypes
	{
		Linear,
		Log,
		Quadratic,
	}
	public AttractionTypes attractionType;
	[Space]
	public float repulsionStrength = 100;
	public enum RepulsionTypes
	{
		Reciprocal,
		InverseSqr,
	}
	public RepulsionTypes repulsionType;
	[Header("Runtime & Refs")]
	public GameCamera cam;
	public VisualNodePerson visualNodePrefab;
	public VisualNodeIdea visualIdeaPrefab;
	public int nodeCount;
	public MGR_gameMaths gameMaths;
	public float[,] graph;

	public struct VisualNodeProperties
	{
		public VisualNode obj;
		public Vector2 p, v, a;
		public float r;
	}
	public VisualNodeProperties[] vn;

	public List<VisualNode> visualNodes;
	public List<VisualNode> visualIdeas;

	private void Awake()
	{
		gameMaths = Managers.Get<MGR_gameMaths>();
		gameMaths.OnReadyForVisualisation += Init;
	}

	private void Update()
	{
		edgeDrawers[gameMaths.NN].show = applyNN && showNodes;
		edgeDrawers[gameMaths.NI].show = applyNI && showNodes && showIdeas;
        edgeDrawers[gameMaths.IN].show = applyIN && showIdeas && showNodes;
        edgeDrawers[gameMaths.II].show = applyII && showIdeas;
    }

    private void FixedUpdate()
	{
        UpdateView(Time.fixedDeltaTime);
    }

	void Init()
	{
		visualNodes = new List<VisualNode>();
		for (int i = 0; i < gameMaths.nodesCount; i++)
		{
			VisualNodePerson newNode = Instantiate(visualNodePrefab);
			newNode.id = i;
			newNode.gameObject.name = "Node " + newNode.id.ToString();
			newNode.transform.position = Random.insideUnitCircle * 10;
			newNode.graphView = this;
			newNode.gameMaths = gameMaths;

			visualNodes.Add(newNode);
		}


		visualIdeas = new List<VisualNode>();
		for (int i = 0; i < gameMaths.ideasCount; i++)
		{
			VisualNodeIdea newNode = Instantiate(visualIdeaPrefab);
			newNode.id = i;
			newNode.gameObject.name = "Idea " + newNode.id.ToString();
			newNode.transform.position = Random.insideUnitCircle * 10;
			newNode.graphView = this;
			newNode.gameMaths = gameMaths;

			visualIdeas.Add(newNode);
		}

		edgeDrawers = new Dictionary<float[,], EdgeDrawer>();
		AddEdgeDrawer(gameMaths.NN, visualNodes, visualNodes, "EdgeDrawer_NN");
        AddEdgeDrawer(gameMaths.NI, visualNodes, visualIdeas, "EdgeDrawer_NI");
		AddEdgeDrawer(gameMaths.IN, visualIdeas, visualNodes, "EdgeDrawer_IN");
		AddEdgeDrawer(gameMaths.II, visualIdeas, visualIdeas, "EdgeDrawer_II");
    }

	void AddEdgeDrawer(float[,] mtx, List<VisualNode> nodesFrom, List<VisualNode> nodesTo, string name = "EdgeDrawer")
	{
		if (edgeDrawers.ContainsKey(mtx))return;
		EdgeDrawer edgeDrawer = Instantiate(edgeDrawerPrefab);
		edgeDrawer.gameObject.name = name;
		edgeDrawer.nodesFrom = nodesFrom;
		edgeDrawer.nodesTo = nodesTo;
		edgeDrawer.mtx = mtx;
		edgeDrawer.cam = cam;
		edgeDrawers[mtx] = edgeDrawer;
		edgeDrawers[mtx].Init();
    }

    void UpdateView(float dt)
	{

	}

	public void SocialView(bool toggle)
	{
		if (!toggle) return;

		showNodes = true;
		applyNN = true;

		showIdeas = false;
		applyII = false;
	}

	public void IdeaView(bool toggle)
	{
		if (!toggle) return;

		showIdeas = true;
		applyII = true;

		showNodes = false;
		applyNN = false;
	}
}