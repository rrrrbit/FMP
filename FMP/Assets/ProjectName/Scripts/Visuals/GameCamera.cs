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
            gameCamera.InitZoomEasing();
        }
    }
}

public class GameCamera : MonoBehaviour
{
    public Vector2 panSpeed;
    public float zoomMin;
    public float zoomMax;
    public float zoomInterval;
    public Camera[] cameras;

    public float currentZoom = 1;
    public float targetZoom = 1;

    float prevZoom;

    public Spring zoomEasing;
    public TokenResolvableValue<SpringSettings> zoomEasingSettings;
    void Awake()
    {
        prevZoom = currentZoom;
        MGR_game.input.OnInputReady += AddCallbacks;
        zoomEasingSettings.token.Reload += InitZoomEasing;
        cameras = GetComponentsInChildren<Camera>();
        InitZoomEasing();
    }

    void Update()
    {
        MGR_input input = MGR_game.input;
        currentZoom = zoomEasing.Update(Time.deltaTime, targetZoom);
        currentZoom.CheckChange(ref prevZoom, () =>
            ZoomAlignPos(
                currentZoom / prevZoom,
                input.pointer.pos.xy()
                )
            );
        transform.position += currentZoom * Time.deltaTime * input.gameActions.Pan.ReadValue<Vector2>().xy().Scaled(panSpeed);
        foreach (Camera cam in cameras)
        {
            cam.orthographicSize = currentZoom;
        }

        if (input.gameActions.PanBtn.WasPressedThisFrame())
        {
            // if click reaches the bg then pan.
        }

        if (input.gameActions.PanBtn.WasReleasedThisFrame())
        {
            // blehhh
        }

        if (input.gameActions.PanBtn.IsPressed())
        {

            transform.position -= input.pointer.relativeDelta;
        }
    }

    public void InitZoomEasing()
    {
        SpringSettings settings = zoomEasingSettings;
        zoomEasing = new(currentZoom, settings.frequency, settings.damping, settings.response);
    }

    void AddCallbacks()
    {
        MGR_game.input.generalActions.Scroll.performed += Zoom;
    }

    void ZoomAlignPos(float delta, Vector2 point)
    {
        transform.position = (delta * (transform.position.xy() - point) + point).xy(transform.position.z);
    }

    private void Zoom(InputAction.CallbackContext obj)
    {
        float amt = -obj.ReadValue<float>();
        targetZoom *= Mathf.Pow(zoomInterval, amt);
        targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);

    }
}
