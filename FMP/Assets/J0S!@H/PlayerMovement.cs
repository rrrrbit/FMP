using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private float Move;

    private Rigidbody2D HK;
    void Start()
    {
        HK = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        Move = Input.GetAxis("Horizontal");
    }
}
