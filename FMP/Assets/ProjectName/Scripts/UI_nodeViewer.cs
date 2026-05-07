using TMPro;
using UnityEngine;

public class UI_nodeViewer : MonoBehaviour
{

    public int nodeIndex;
    public MGR_gameMaths gameMaths;
    public TextMeshPro texts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameMaths = Managers.Get<MGR_gameMaths>();
    }



    // Update is called once per frame
    void Update()
    {
        
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
            "\nEnthusiasm" + Round(stats.enthusiasm.strengthPos) + 
            "\nReach" + Round(stats.reach) +
            "\nSuggestibility" + Round(stats.suggestibility.strengthPos)
            );
    }
}
