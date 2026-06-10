// KeySpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class KeySpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private int keysToSpawn = 7;
    [SerializeField] private Transform[] spawnPoints; // drag all 38 patrol nodes here

    void Start()
    {
        SpawnKeys();
    }

    void SpawnKeys()
    {
        if (spawnPoints.Length < keysToSpawn)
        {
            Debug.LogError("Not enough spawn points for keys.");
            return;
        }

        // Pick 7 unique random indices from the 38 patrol nodes
        List<int> available = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            available.Add(i);

        for (int i = 0; i < keysToSpawn; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            int chosenIndex = available[randomIndex];
            available.RemoveAt(randomIndex);

            Vector3 spawnPos = spawnPoints[chosenIndex].position;
            spawnPos.y += 0.01f; // slight offset so key sits above floor
            Instantiate(keyPrefab, spawnPos, Quaternion.identity);
        }
    }
}