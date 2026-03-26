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
        int max;
        float[] edges = game.ni.GetEdgesFrom(id);
    }
}
