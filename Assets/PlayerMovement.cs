using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool useWorldMovement = false;

    Rigidbody rb;

    float h, v;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read input here (important)
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector3 move;

        if (useWorldMovement)
            move = new Vector3(h, 0, v);
        else
            move = transform.right * h + transform.forward * v;

        move = move.normalized;

        // Apply velocity instead of forcing position
        Vector3 velocity = move * speed;
        velocity.y = rb.linearVelocity.y; // keep gravity if needed

        rb.linearVelocity = velocity;
    }
}