using RBitUtils;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    MGR_input input;
    public Vector2 panSpeed;
    public float zoomMin;
    public float zoomMax;
    public float zoomSens;

    [SerializeField]float currentZoom;
    void Awake()
    {
        input = Managers.Get<MGR_input>();
        input.OnInputReady += AddCallbacks;
    }

    void AddCallbacks()
    {
        input.gameActions.Scroll.started += Zoom;
        print("added callbacks");
    }

    private void Zoom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        print("zoom" + obj.ReadValue<float>().ToString());
        currentZoom -= obj.ReadValue<float>() * zoomSens;
        currentZoom = Mathf.Clamp(currentZoom, zoomMin, zoomMax);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += input.gameActions.Pan.ReadValue<Vector2>().xy().Scaled(panSpeed) * currentZoom * Time.deltaTime;
        GetComponent<Camera>().orthographicSize = currentZoom;
    }
}
