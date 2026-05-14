using UnityEngine;

public class NodesUpdated : MonoBehaviour
{
    public static int Node_per_influence = 5;
    public int nodeCount;
    public bool influences;
    // This variable will be incremented with the Nodes method
    
    // 

    public void Nodes()
    {
        nodeCount++;
    }

    void Node_influence_counter()
    {
        if (influences == false)
        {
            Debug.Log("No influence has occured");
            // This actually helps to print informational messages
            while (influences == false)
            {

            }
        }
        else if (influences == true)
        {
            Debug.Log("Influences have begun to occur");
        }
    }

    void Start()
    {
        Node_influence_counter();
        Nodes();
    }

   
    void Update()
    {
        
    }
}
