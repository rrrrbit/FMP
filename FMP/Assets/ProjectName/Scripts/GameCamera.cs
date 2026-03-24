using RBitUtils;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameCamera : MonoBehaviour
{
    MGR_input input;
    public Vector2 panSpeed;
    public float zoomMin;
    public float zoomMax;
    public float zoomInterval;
    Camera cam;

    [SerializeField]float currentZoom = 1;
    void Awake()
    {
        input = Managers.Get<MGR_input>();
        cam = GetComponent<Camera>();
        input.OnInputReady += AddCallbacks;
    }

    void AddCallbacks()
    {
        input.gameActions.Scroll.performed += Zoom;
        print("added callbacks");
    }

    void ZoomOnPoint(float delta, Vector2 point)
    {
        currentZoom *= delta;
        transform.position = (delta * (transform.position.xy() - point) + point).xy(transform.position.z);
    }

    private void Zoom(InputAction.CallbackContext obj)
    {
        float amt = -obj.ReadValue<float>();
        ZoomOnPoint(
            Mathf.Pow(zoomInterval, amt), 
            cam.ScreenToWorldPoint(
                Mouse.current.position.ReadValue().xy(-transform.position.z)
                )
            );
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += input.gameActions.Pan.ReadValue<Vector2>().xy().Scaled(panSpeed) * currentZoom * Time.deltaTime;
        cam.orthographicSize = currentZoom;

        if (input.gameActions.PanBtn.IsPressed())
        {
            
            transform.position -= ScreenToWorldDelta(
                cam, 
                Mouse.current.delta.ReadValue(), 
                -transform.position.z
                );
            //Mouse.current.delta.ReadValue().xy() / currentZoom / cam.aspect / 2
        }
    }

    Vector3 ScreenToWorldDelta(Camera cam, Vector2 screenDelta, float zDepth)
    {
        Vector3 p1 = cam.ScreenToWorldPoint(Vector2.zero.xy(zDepth));
        Vector3 p2 = cam.ScreenToWorldPoint(Vector2.one.xy(zDepth));

        return screenDelta.Scaled(p2-p1);
    }
}
