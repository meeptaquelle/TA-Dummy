// DoorMarker.cs
using UnityEngine;
using TMPro;

public class DoorMarker : MonoBehaviour
{
    private Transform mainCamera;

    void Awake()
    {
        mainCamera = Camera.main.transform;
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Always face camera
        transform.LookAt(transform.position + mainCamera.forward);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}