using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera fpsCamera;
    public Camera topDownCamera;
    public PlayerMovement movement;
    public MonoBehaviour mouseLook; // drag MouseLook here
    public Transform player;
    private Quaternion savedRotation;
    void Start()
    {
        ActivateFPS();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (fpsCamera.enabled)
                ActivateTopDown();
            else
                ActivateFPS();
        }
    }

    void ActivateFPS()
    {
        fpsCamera.enabled = true;
        topDownCamera.enabled = false;

        mouseLook.enabled = true;

        movement.useWorldMovement = false; // ?? ADD HERE

        LockCursor(true);
    }

    void ActivateTopDown()
    {
        fpsCamera.enabled = false;
        topDownCamera.enabled = true;

        mouseLook.enabled = false;

        movement.useWorldMovement = true; // ?? ADD HERE

        LockCursor(false);
    }
    void LockCursor(bool state)
    {
        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}