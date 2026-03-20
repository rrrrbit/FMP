using JetBrains.Annotations;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

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
	public AdjacencyDictionary nn;
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
		nn = new AdjacencyDictionary(nodes);
		foreach (Node i in nodes)
		{
			foreach (Node j in nodes)
			{
				if(i == j) continue; // skip self-connections
				float x = Random.value * 2 - 1;
				nn.dict[i][j]=Mathf.Pow(x, 11)*10;
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
		debugText.text = string.Join("\n", nn.dict.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString()))));
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

public class AdjacencyDictionary
{
	public List<Node> nodes;
	public Dictionary<Node, Dictionary<Node, float>> dict;
	public AdjacencyDictionary()
	{
		nodes = new List<Node>();
		dict = new Dictionary<Node, Dictionary<Node, float>>();
	}
	
	public AdjacencyDictionary(IEnumerable<Node> nodes)
	{
		this.nodes = nodes.ToList();
		dict = new Dictionary<Node, Dictionary<Node, float>>();
		foreach (Node to in nodes)
		{
			dict[to] = new Dictionary<Node, float>();
			foreach (Node from in nodes)
			{
				dict[to][from] = 0;
			}
		}
	}
	public Dictionary<Node, float> GetConnectionsFrom(Node from)
	{
		Dictionary<Node, float> connectionsTo = new Dictionary<Node, float>();
		foreach (Node to in nodes)
		{
			connectionsTo[to] = dict[to][from];
		}
		return connectionsTo;
	}
	public float GetOutdegree(Node from)
	{
		float sum = 0;
		foreach (Node to in nodes)
		{
			sum += Mathf.Abs(dict[to][from]);
		}
		return sum;
	}

	public IEnumerable<(Node to, Node from, float weight)> GetAllConnections()
	{
		foreach (var to in dict.Keys)
		{
			foreach (var from in dict[to].Keys)
			{
				yield return (to, from, dict[from][to]);
			}
		}
	}
	public IEnumerable<(Node other, float weight)> GetAllConnections(Node node)
	{
		foreach (var connection in dict[node].Concat(GetConnectionsFrom(node)))
		{
			yield return (connection.Key, connection.Value);
		}
	}
	public float maxWeight => dict.Values.Max(x => x.Values.Max());
	public float minWeight => dict.Values.Min(x => x.Values.Min());
	public float maxAbsWeight => dict.Values.Max(x => x.Values.Max(y => Mathf.Abs(y)));
	public float sumAbsWeight => dict.Values.Sum(x => x.Values.Sum(y => Mathf.Abs(y)));
	public float maxOutdegree => nodes.Max(x => GetOutdegree(x));
}