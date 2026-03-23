using UnityEngine;

public class Camera : MonoBehaviour
{

    MGR_input input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = Managers.Get<MGR_input>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
