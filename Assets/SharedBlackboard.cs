using System.Collections.Generic;
using UnityEngine;

public class SharedBlackboard : MonoBehaviour
{
    public static SharedBlackboard Instance;

    public bool playerDetected;
    public Vector3 lastSeenPlayerPos;
    public float sharedStamina = 5f;

    public List<int> flankerSlots = new List<int>(); // tracks claimed slot indices

    void Awake()
    {
        Instance = this;
    }

    public int ClaimFlankerSlot()
    {
        int slot = flankerSlots.Count;
        flankerSlots.Add(slot);
        return slot;
    }
}
