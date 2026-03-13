using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameMaths : MonoBehaviour
{
    public Dictionary<Node, Dictionary<Node, float>> nn;
    public Node[] nodes;
    public TextMeshProUGUI debugText;

    public int startingNumber;
    private void Start()
    {
        nodes = new Node[startingNumber];
        foreach (Node i in nodes)
        {
            if (i == null)
            {
                i = new Node();
            }
            Dictionary<Node, float> newRow = new Dictionary<Node, float>();
            foreach (Node j in nodes)
            {
                newRow.Add(j, Random.Range(-10f, 10f));
            }

            nn.Add(i, newRow);
        }
    }

    private void Update()
    {
        debugText.text = string.Join("\n", nn.Values.Select(x => string.Join(" ", x.Values.Select(y => Mathf.Round(y).ToString()))));
    }
}

public class Node
{

}