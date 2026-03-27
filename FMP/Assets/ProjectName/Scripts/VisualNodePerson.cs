using TMPro;
using UnityEngine;

public class VisualNodePerson : VisualNode
{
    MGR_gameMaths game;
    [SerializeField]TextMeshPro text;
    [SerializeField] float[] niEdges;

    private void Start()
    {
        game = Managers.Get<MGR_gameMaths>();
    }

    void Update()
    {
        int strongestId = 0;
        float strongestWeight = 0;
        niEdges = game.niMtx.GetEdgesFrom(id);
		for(int i = 0; i < niEdges.Length; i++)
		{
            if (niEdges[i] > strongestWeight)
            {
                strongestId = i;
                strongestWeight = niEdges[i];
            }
		}
        text.text = strongestId.ToString();
    }
}
