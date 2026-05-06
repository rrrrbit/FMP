using UnityEngine;
using UnityEngine.InputSystem;
using RBitUtils;

public class UI_pointer : MonoBehaviour
{
    public Camera midCam;

    public Vector3 delta;
    public Vector3 prevPos;
    public Vector3 pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = midCam.ScreenToWorldPoint(Managers.Get<MGR_input>().generalActions.MousePos.ReadValue<Vector2>().xy(-midCam.transform.position.z));
        delta = transform.position - prevPos;
        prevPos = transform.position;
    }
}
