using TMPro;
using UnityEngine;

public class VisualNodePerson : VisualNode
{
    MGR_gameMaths game;
    TextMeshPro text;

    private void Start()
    {
        game = Managers.Get<MGR_gameMaths>();
    }

    void Update()
    {
        int strongestId = 0;
        float[] edges = game.ni.GetEdgesFrom(id);
		for(int i = 0; i < edges.Length; i++)
		{
			
		}
    }
}
