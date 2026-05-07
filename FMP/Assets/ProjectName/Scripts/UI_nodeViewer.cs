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

    MGR_gameMaths gameMaths;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameMaths = Managers.Get<MGR_gameMaths>();
        gameMaths.OnReadyForVisualisation += InitIdeaBars;
    }

    void InitIdeaBars()
    {
        ideaBars = new GameObject[gameMaths.ideasCount];
        for (int i = 0; gameMaths.ideasCount > 0; i++)
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
        NodeStats stats = gameMaths.nodeStats[nodeIndex];
        texts.text = (
              "Complexity: " + Round(stats.complexity) +
            "\nComplexity Tolerance: " + Round(stats.complexityTolerance.center) + 
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
        for (int i = 0; gameMaths.ideasCount > 0; i++)
        {
            if (Mathf.Abs(gameMaths.NI.mtx[nodeIndex, i]) > Mathf.Abs(maxAbsNI))
            {
                maxAbsNI = gameMaths.NI.mtx[nodeIndex, i];
            }
        }

        for (int i = 0; gameMaths.ideasCount > 0; i++)
        {
            ideaBars[i].GetComponent<UI_twoWayBar>().value = gameMaths.NI.mtx[nodeIndex, i] / maxAbsNI;
        }
    }
}
