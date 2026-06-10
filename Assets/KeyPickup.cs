// KeyPickup.cs
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float interactRadius = 2f;

    private Transform player;
    private bool collected = false;
    private bool playerInRange = false;

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
    }

    void Update()
    {
        if (collected) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool inRange = dist <= interactRadius;

        if (inRange && !playerInRange)
        {
            playerInRange = true;
            HUDManager.Instance.ShowPickupPrompt(true);
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            HUDManager.Instance.ShowPickupPrompt(false);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            collected = true;
            HUDManager.Instance.ShowPickupPrompt(false);
            GameManager.Instance.CollectKey();
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (playerInRange)
            HUDManager.Instance.ShowPickupPrompt(false);
    }
}