using UnityEngine;
using UnityEngine.SceneManagement;

public class MGR_mainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    public void Play()
    {
        SceneManager.LoadScene("game");
    }

    public void Options()
    {
        print("options");
    }

    public void ShowCredits()
    {
        print("credits");
    }
}
