using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera fpsCamera;
    public Camera topDownCamera;

    void Start()
    {
        ActivateFPS();
    }

    void Update()
    {
        // Press TAB to switch
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
    }

    void ActivateTopDown()
    {
        fpsCamera.enabled = false;
        topDownCamera.enabled = true;
    }
}