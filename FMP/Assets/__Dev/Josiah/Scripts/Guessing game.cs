using System.Runtime.CompilerServices;
using UnityEngine;

public class Guessinggame : MonoBehaviour
{
    public int maxNumber = 20;
    public int yourGuess;
    public string result;

    private int secretNumber;

    void Start()
    {
        secretNumber = Random.Range(1, maxNumber + 1);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (yourGuess == secretNumber)
        {
            result = "just right!";
        }
        else
        {
            switch ((int)Mathf.Sign(yourGuess - secretNumber))
            {
                case 1:
                    result = "Too high.";
                    break;

                case -1:
                    result = "Too low";
                    break;
            }
        }

    }
}
