using TMPro;
using UnityEngine;

public class VisualNodePerson : VisualNode
{
    MGR_gameMaths game;
    [SerializeField]TextMeshPro text;
    [SerializeField]float[] niEdges;
    [SerializeField]float[] inEdges;
    [SerializeField]float[] nnEdgesTo;

    private void Start()
    {
        game = Managers.Get<MGR_gameMaths>();
    }

    void Update()
    {
        int strongestId = 0;
        float strongestWeight = 0;
        niEdges = game.NI.GetEdgesFrom(id);
		for(int i = 0; i < niEdges.Length; i++)
		{
            if (niEdges[i] > strongestWeight)
            {
                strongestId = i;
                strongestWeight = niEdges[i];
            }
		}
        text.text = strongestId.ToString();

        inEdges = game.IN.GetEdgesTo(id);
        nnEdgesTo = game.NN.GetEdgesTo(id);
    }
}
