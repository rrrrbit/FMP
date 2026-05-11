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
	}

	void UpdateView(float dt)
	{

	}
}