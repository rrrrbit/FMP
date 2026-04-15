using RBitUtils;
using RBitUtils.ResponseTypes;
using Unity.Hierarchy;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

[CustomEditor(typeof(GameCamera), true)]
public class EDITOR_GameCamera : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameCamera gameCamera = (GameCamera)target;
        if (GUILayout.Button("Reload Zoom Easing Settings"))
        {
            gameCamera.InitialiseZoomEasing();
        }
    }
}

public class GameCamera : MonoBehaviour
{
    MGR_input input;
    public Vector2 panSpeed;
    public float zoomMin;
    public float zoomMax;
    public float zoomInterval;
    Camera backCam;

    public float currentZoom = 1;
    public Camera midCam, frontCam;
    Vector3 targetPos;
    public float targetZoom = 1;
    float prevZoom;

    public Spring zoomEasing;
    public TokenResolvableValue<SpringSettings> zoomEasingSettings;
    void Awake()
    {
        input = Managers.Get<MGR_input>();
        backCam = GetComponent<Camera>();
        input.OnInputReady += AddCallbacks;
        zoomEasingSettings.token.Reload += InitialiseZoomEasing;
        InitialiseZoomEasing();
    }

    public void InitialiseZoomEasing()
    {
        SpringSettings settings = zoomEasingSettings;
        zoomEasing = new(currentZoom, settings.frequency, settings.damping, settings.response);
    }

    void AddCallbacks()
    {
        input.gameActions.Scroll.performed += Zoom;
        print("added callbacks");
    }

    void ZoomOnPoint(float delta, Vector2 point)
    {
        transform.position = (delta * (transform.position.xy() - point) + point).xy(transform.position.z);
    }

    private void Zoom(InputAction.CallbackContext obj)
    {
        float amt = -obj.ReadValue<float>();
        targetZoom *= Mathf.Pow(zoomInterval, amt);


    }

    void UpdateZoom()
    {
        ZoomOnPoint(
            currentZoom/prevZoom,
            midCam.ScreenToWorldPoint(
                Mouse.current.position.ReadValue().xy(-transform.position.z)
                )
            );
    }

    // Update is called once per frame
    void Update()
    {
        currentZoom = zoomEasing.Update(Time.deltaTime, targetZoom);
        currentZoom.CheckChange(ref prevZoom, UpdateZoom);
        transform.position += input.gameActions.Pan.ReadValue<Vector2>().xy().Scaled(panSpeed) * currentZoom * Time.deltaTime;
        //cam.orthographicSize = currentZoom;
        midCam.orthographicSize = currentZoom;
        //frontCam.orthographicSize = currentZoom;

        if (input.gameActions.PanBtn.IsPressed())
        {
            
            transform.position -= ScreenToWorldDelta(
                midCam, 
                Mouse.current.delta.ReadValue(), 
                transform.position.z
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
