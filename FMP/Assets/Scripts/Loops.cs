using UnityEngine;
using System.Collections;

public class Loops : MonoBehaviour
{
    int nodeCorrelation;
    int numEnemies = 5;



    private void Start()
    {
        while (nodeCorrelation < 0)
        {
            Debug.Log("The Nodes agree");
            nodeCorrelation--;

        }

        for (int i = 0; i < numEnemies; i++)
        {
            Debug.Log("Analysing enemy number: " + i);

        }



    }
    private void Update()
    {
       
    }

    

}
