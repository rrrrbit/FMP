using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Switchcase : MonoBehaviour
{
    public int node_influences = 10;


    void Node_correlations()
    {
        switch (node_influences)
        {
            case 5:
                print("Nodes have positive influences");
                break;
            case 4:
                print("Nodes have negative influences");
                break;
                case 3:
                print("Nodes have positive influences");
                break;
                case 2:
                print("Nodes have negative influences");
                break;
                case 1:
                print("Nodes have positive influences");
                break;
            default:
                print("Nodes have negative influences");
                break;






        }










    }
}
