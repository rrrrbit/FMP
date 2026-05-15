using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class UI_nodeViewer : MonoBehaviour
{

    public int nodeIndex;
    public TMP_Text texts;
    public RectTransform ideaBarsContainer;
    public GameObject[] ideaBars;
    public GameObject ideaBarPrefab;

    [SerializeField] float fadeLength = 0.05f;
    
    void Start()
    {
        MGR_game.mtx.OnReadyForVisualisation += InitIdeaBars;
    }
    void Update()
    {
        float a = GetComponent<CanvasGroup>().alpha;
        if (nodeIndex == -1)
        {
            a = Mathf.MoveTowards(a, 0, Time.deltaTime / fadeLength);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            UpdateViewer();
            a = Mathf.MoveTowards(a, 1, Time.deltaTime / fadeLength);
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        GetComponent<CanvasGroup>().alpha = a;
    }

    void InitIdeaBars()
    {
        ideaBars = new GameObject[MGR_game.mtx.ideasCount];
        for (int i = 0; i < MGR_game.mtx.ideasCount; i++)
        {
            if (ideaBars[i] == null)
            {
                GameObject newBar = Instantiate(ideaBarPrefab, ideaBarsContainer);
                ideaBars[i] = newBar;
            }
        }
    }

    void UpdateViewer()
    {
        float Round(float x)
        {
            int nearest = 100;
            return Mathf.Round(x * nearest) / nearest;
        }

        MGR_mtx.NodeStats stats = MGR_game.mtx.nodeStats[nodeIndex];
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
        float[,] NI = MGR_game.mtx.NI;

        for (int i = 0; i < MGR_game.mtx.ideasCount; i++)
        {
            if (Mathf.Abs(NI[nodeIndex, i]) > Mathf.Abs(maxAbsNI))
            {
                maxAbsNI = NI[nodeIndex, i];
            }
        }

        for (int i = 0; i < MGR_game.mtx.ideasCount; i++)
        {
            ideaBars[i].GetComponent<UI_twoWayBar>().value = NI[nodeIndex, i] / maxAbsNI;
            ideaBars[i].GetComponentInChildren<Image>().color = MGR_game.visuals.ideaColours[i];
        }
    }
}
