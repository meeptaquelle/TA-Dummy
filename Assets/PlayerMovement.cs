using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool useWorldMovement = false;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move;

        if (useWorldMovement)
            move = new Vector3(h, 0, v); // world direction
        else
            move = transform.right * h + transform.forward * v;

        transform.Translate(move * speed * Time.deltaTime, Space.World);
    }
}