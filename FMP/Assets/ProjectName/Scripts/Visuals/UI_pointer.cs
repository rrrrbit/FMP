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

    void Update()
    {
        prevRelativePos = relativePos;
        transform.position = midCam.ScreenToWorldPoint(MGR_game.input.generalActions.MousePos.ReadValue<Vector2>().xy(-midCam.transform.position.z));
        pos = transform.position;
        relativePos = pos - midCam.transform.position;

        relativeDelta = relativePos - prevRelativePos;
    }
}
