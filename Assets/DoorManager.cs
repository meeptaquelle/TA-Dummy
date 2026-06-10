// DoorController.cs
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private DoorMarker marker;

    public void Reveal()
    {
        gameObject.SetActive(true);
        if (marker != null) marker.Show();
        HUDManager.Instance.ShowEscapePrompt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TriggerWin();
        }
    }
}