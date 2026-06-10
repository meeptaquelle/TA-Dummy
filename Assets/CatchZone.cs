// CatchZone.cs — sits on the child CatchZone object
using UnityEngine;

public class CatchZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}