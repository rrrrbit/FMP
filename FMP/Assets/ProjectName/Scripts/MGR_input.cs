using UnityEngine;

public class MGR_input : MonoBehaviour, IInput
{
    public Input input;
    public Input.GameActions gameActions;
    public Input.GeneralActions generalActions;
    public event System.Action OnInputReady;

    public UI_pointer pointer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = new Input();
        input.Enable();

        gameActions = input.Game;
        gameActions.Enable();

        generalActions = input.General;
        generalActions.Enable();

        OnInputReady?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
