using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool useWorldMovement = false;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move;

        if (useWorldMovement)
            move = new Vector3(h, 0, v);
        else
            move = transform.right * h + transform.forward * v;

        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);

        move = move.normalized;
    }
}