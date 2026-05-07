using UnityEngine;
using UnityEngine.InputSystem;
using RBitUtils;

public class UI_pointer : MonoBehaviour
{
    public Camera midCam;

    public Vector3 relativeDelta;
    public Vector3 prevRelativePos;

    /// <summary>
    /// Position in world units relative to the camera.
    /// </summary>
    public Vector3 relativePos;
    public Vector3 pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        prevRelativePos = relativePos;
        transform.position = midCam.ScreenToWorldPoint(Managers.Get<MGR_input>().generalActions.MousePos.ReadValue<Vector2>().xy(-midCam.transform.position.z));
        pos = transform.position;
        relativePos = pos - midCam.transform.position;

        relativeDelta = relativePos - prevRelativePos;
    }
}
