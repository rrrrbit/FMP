using UnityEngine;

public class Visual_Spin : MonoBehaviour
{
    [SerializeField] float degPerSec;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(-degPerSec * Time.deltaTime * Vector3.forward);
    }
}
