using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] Rigidbody idea;
    public float moveSpeed;
    float xInput, yInput;
    Vector2 input;
    public float speed = 5;

    void Start()
    {

    }

    void Update()
    {
        idea.AddForce(input * speed);
    }

    private void FixedUpdate()
    {
        xInput = Input.GetAxis("Horizontal");
        xInput = Input.GetAxis("Vertical");

        transform.Translate(xInput * moveSpeed, yInput * moveSpeed,0f);
    }
    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }



















































}