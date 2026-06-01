using UnityEngine;

public class NodeLimit : MonoBehaviour
{
    public int Node_limit = 15;
    public bool isNodeLimit;
    public bool ifClonesSpawn;
    private Rigidbody rb;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void Awake()
    {
        rb= GetComponent<Rigidbody>();

    }

    private void OnMouseDown()
    {
        rb.AddForce(-transform.forward * 650f);
        rb.useGravity = true;
    }




}


