using TMPro;
using UnityEngine;

public class VisualNodePerson : VisualNode
{
    MGR_gameMaths game;
    [SerializeField]TextMeshPro text;
    [SerializeField]float[] edges;

    private void Start()
    {
        game = Managers.Get<MGR_gameMaths>();
    }

    void Update()
    {
        int strongestId = 0;
        float strongestWeight = 0;
        edges = game.NI.GetEdgesFrom(id);
		for(int i = 0; i < edges.Length; i++)
		{
            if (edges[i] > strongestWeight)
            {
                strongestId = i;
                strongestWeight = edges[i];
            }
		}
        text.text = strongestId.ToString();
    }
}
