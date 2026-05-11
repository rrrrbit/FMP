using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UI_nodeViewer : MonoBehaviour
{

    public int nodeIndex;
    public TMP_Text texts;
    public RectTransform ideaBarsContainer;
    public GameObject[] ideaBars;
    public GameObject ideaBarPrefab;

    MGR_gameMaths game;
    
    void Start()
    {
        game = Managers.Get<MGR_gameMaths>();
        game.OnReadyForVisualisation += InitIdeaBars;
    }

    void InitIdeaBars()
    {
        ideaBars = new GameObject[game.ideasCount];
        for (int i = 0; i < game.ideasCount; i++)
        {
            if (ideaBars[i] == null)
            {
                GameObject newBar = Instantiate(ideaBarPrefab, ideaBarsContainer);
                ideaBars[i] = newBar;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateViewer();
    }

    float Round(float x)
    {
        int nearest = 100;
        return Mathf.Round(x * nearest) / nearest;
    }

    void UpdateViewer()
    {
        MGR_gameMaths.NodeStats stats = game.nodeStats[nodeIndex];
        texts.text = (
              "Complexity: " + Round(stats.complexity) +
            "\nComplexity Tolerance: " + Round(stats.complexityTolerance.width) + 
            "\nEnthusiasm: <color=#FF0000>" + Round(stats.enthusiasm.strengthPos) + "</color> / <color=#00FF00>" + Round(stats.enthusiasm.strengthNeg) + "</color>" +
            "\nReach: " + Round(stats.reach) +
            "\nSuggestibility: <color=#FF0000>" + Round(stats.suggestibility.strengthPos) + "</color> / <color=#00FF00>" + Round(stats.suggestibility.strengthNeg) + "</color>" +
            "\nAdherence: <color=#FF0000>" + Round(stats.adherence.strengthPos) + "</color> / <color=#00FF00>" + Round(stats.adherence.strengthNeg) + "</color>" +
            "\nExtroversion: " + Round(stats.extroversion) + 
            "\nAvoidance: " + Round(stats.avoidance)
            );

        UpdateIdeaBars();
    }

    void UpdateIdeaBars()
    {
        float maxAbsNI = 0;
        
        for (int i = 0; i < game.ideasCount; i++)
        {
            if (Mathf.Abs(game.NI[nodeIndex, i]) > Mathf.Abs(maxAbsNI))
            {
                maxAbsNI = game.NI[nodeIndex, i];
            }
        }

        for (int i = 0; i < game.ideasCount; i++)
        {
            ideaBars[i].GetComponent<UI_twoWayBar>().value = game.NI[nodeIndex, i] / maxAbsNI;
        }
    }
}
