using UnityEngine;
using System.Collections;

public class Loops : MonoBehaviour
{
    public class loops
    {


    }

    int nodeCorrelation;
    int numEnemies = 5;

    public enum loop_statements
    {
          For,
          IF,
          While,
          Do,
          Foreach,
          Else
        // Just Experimenting



    }
    

    private void Start()
    { 
       

        while (nodeCorrelation < 0)
        {
            Debug.Log("The Nodes agree");
            nodeCorrelation--;

        }

        for (int i = 0; i < numEnemies; i++)   // This is adding 1 to the number of enemies
        {
            Debug.Log("Analysing enemy number: " + i);
            
        }

       

    }
    private void Update()
    {
       
    }

   
    

}
