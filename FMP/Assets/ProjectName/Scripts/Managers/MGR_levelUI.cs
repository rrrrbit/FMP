using UnityEngine;

public class MGR_levelUI : MonoBehaviour
{
    Visual_Node selectedNode;
    public Camera uiCam;
    public Canvas canvas;
    public UI_nodeViewer nodeViewer;
    public UI_bg bg;
    public Visual_NodeSelection selection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnfocusNodeViewer();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryFocusNodeViewer(Visual_Node node, bool idea = false)
    {
        if (nodeViewer.nodeIndex == node.id)
        {
            UnfocusNodeViewer();
            return;
        }
        nodeViewer.nodeIndex = node.id;
        nodeViewer.viewIdea = idea;
        selection.target = node.transform;
        selection.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void UnfocusNodeViewer()
    {
        nodeViewer.nodeIndex = -1;
        selection.GetComponent<SpriteRenderer>().enabled = false;
    }
}
