using TMPro;
using Unity.Android.Types;
using UnityEngine;
using UnityEngine.EventSystems;

public class VisualNodePerson : VisualNode, IPointerEnterHandler, IPointerExitHandler
{
    MGR_gameMaths game;
    [SerializeField] TextMeshPro text;
    [SerializeField] float[] niEdges;
    [SerializeField] float[] inEdges;
    [SerializeField] float[] nnEdgesTo;
    [SerializeField] MGR_gameMaths.NodeStats stats;
    [SerializeField] MGR_gameMaths.NodeStats dstats;
    [SerializeField] bool mouseOver;
    [SerializeField] bool dragging;
    [SerializeField] Vector2 toMouse;

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

        stats = game.nodeStats[id];
        dstats = game.nodeStatsDelta[id];
    }

    void IPointerEnterHandler.OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        print("over bg node");
    }

    void IPointerExitHandler.OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        print("mouse exit bg node");
    }
}
