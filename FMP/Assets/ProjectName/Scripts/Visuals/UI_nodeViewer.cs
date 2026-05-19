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

        string PosNegString(float pos, float neg, bool perSec = false, string divider = " / ")
        {
            if(perSec) return "<color=#FF0000>" + Round(pos) + "/s</color>" + divider + "<color=#00FF00>" + Round(neg) + "/s</color>";
            else return "<color=#FF0000>" + Round(pos) + "</color>"+divider+"<color=#00FF00>" + Round(neg) + "</color>";
        }

        MGR_mtx.NodeStats stats = MGR_game.mtx.nodeStats[nodeIndex];
        MGR_mtx.NodeStats dstats = MGR_game.mtx.nodeStatsDelta[nodeIndex];
        texts.text = (
                        "Complexity: " + Round(stats.complexity) + 
                                  " (" + Round(dstats.complexity) + "/s)" +

            "\nComplexity Tolerance: " + Round(stats.complexityTolerance.width) +
                                  " (" + Round(dstats.complexityTolerance.width) + "/s)" + 

                      "\nEnthusiasm: " + PosNegString(stats.enthusiasm.strengthPos, stats.enthusiasm.strengthNeg) +
                                  " (" + PosNegString(dstats.enthusiasm.strengthPos, dstats.enthusiasm.strengthNeg, true) + ")" +

                           "\nReach: " + Round(stats.reach) + 
                                  " (" + Round(dstats.reach) + "/s)" +

                  "\nSuggestibility: " + PosNegString(stats.suggestibility.strengthPos, stats.suggestibility.strengthNeg) +
                                  " (" + PosNegString(dstats.suggestibility.strengthPos, dstats.suggestibility.strengthNeg, true) + ")" +

                       "\nAdherence: " + PosNegString(stats.adherence.strengthPos, stats.adherence.strengthNeg) +
                                  " (" + PosNegString(dstats.adherence.strengthPos, dstats.adherence.strengthNeg, true) + ")" +

                    "\nExtroversion: " + Round(stats.extroversion) + 
                                  " (" + Round(dstats.extroversion) + "/s)" + 

                       "\nAvoidance: " + Round(stats.avoidance) + 
                                  " (" + Round(dstats.avoidance) + "/s)"
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
