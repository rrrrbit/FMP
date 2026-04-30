using UnityEngine;
using System.Collections;
public class Exitgame : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("quit");
    }
}
