using UnityEngine;
using System.Collections;
public class Nodecolours : MonoBehaviour
{
    public int colourcode;

    void node_colour_system()
    {
        switch (colourcode)
        {
            case 5:
                print("Node is red");
                break;
            case 4:
                print("Node is blue");
                break;
            case 3:
                print("Node is green");
                break;
            case 2:
                print("Node is black");
                break;
            case 1:
                print("Node is white");
                break;
            default:
                print("Node colour is classified");
                // With this line of code I'm basically explaining how each node colour system works based on which case it's situated in
                break;




        }


    }


    
}
