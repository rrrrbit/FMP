using RBitUtils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class MGR_visuals : MonoBehaviour
{
	[Header("Misc")]
	public AnimationCurve sizeByIndegree;
	public float pairForceWeightThreshold;

	[Header("Show")]
	public bool showNodes;
	public bool showIdeas;
	public bool applyNN;
	public bool applyNI;
	public bool applyIN;
	public bool applyII;

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
	public EdgeDrawer edgeDrawerPrefab;
	public VisualNodePerson visualNodePrefab;
	public VisualNodeIdea visualIdeaPrefab;
	public Dictionary<float[,], EdgeDrawer> edgeDrawers;

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
        MGR_game.mtx.OnReadyForVisualisation += Init;
	}

	private void Update()
	{
		edgeDrawers[MGR_game.mtx.NN].show = applyNN && showNodes;
		edgeDrawers[MGR_game.mtx.NI].show = applyNI && showNodes && showIdeas;
        edgeDrawers[MGR_game.mtx.IN].show = applyIN && showIdeas && showNodes;
        edgeDrawers[MGR_game.mtx.II].show = applyII && showIdeas;
    }

	void Init()
	{
		visualNodes = new List<VisualNode>();
		for (int i = 0; i < MGR_game.mtx.nodesCount; i++)
		{
			VisualNodePerson newNode = Instantiate(visualNodePrefab);
			newNode.id = i;
			newNode.gameObject.name = "Node " + newNode.id.ToString();
			newNode.transform.position = Random.insideUnitCircle * 10;

			visualNodes.Add(newNode);
		}


		visualIdeas = new List<VisualNode>();
		for (int i = 0; i < MGR_game.mtx.ideasCount; i++)
		{
			VisualNodeIdea newNode = Instantiate(visualIdeaPrefab);
			newNode.id = i;
			newNode.gameObject.name = "Idea " + newNode.id.ToString();
			newNode.transform.position = Random.insideUnitCircle * 10;

			visualIdeas.Add(newNode);
		}

		edgeDrawers = new Dictionary<float[,], EdgeDrawer>();
		AddEdgeDrawer(MGR_game.mtx.NN, visualNodes, visualNodes, "EdgeDrawer_NN");
        AddEdgeDrawer(MGR_game.mtx.NI, visualNodes, visualIdeas, "EdgeDrawer_NI");
		AddEdgeDrawer(MGR_game.mtx.IN, visualIdeas, visualNodes, "EdgeDrawer_IN");
		AddEdgeDrawer(MGR_game.mtx.II, visualIdeas, visualIdeas, "EdgeDrawer_II");
    }

	void AddEdgeDrawer(float[,] mtx, List<VisualNode> nodesFrom, List<VisualNode> nodesTo, string name = "EdgeDrawer")
	{
		if (edgeDrawers.ContainsKey(mtx))return;
		EdgeDrawer edgeDrawer = Instantiate(edgeDrawerPrefab);
		edgeDrawer.gameObject.name = name;
		edgeDrawer.nodesFrom = nodesFrom;
		edgeDrawer.nodesTo = nodesTo;
		edgeDrawer.mtx = mtx;
		edgeDrawers[mtx] = edgeDrawer;
		edgeDrawers[mtx].Init();
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