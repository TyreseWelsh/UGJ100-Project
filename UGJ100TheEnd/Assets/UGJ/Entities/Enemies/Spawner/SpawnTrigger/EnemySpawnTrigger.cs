using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private List<EnemySpawner> spawners;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (EnemySpawner spawner in spawners)
            {
                spawner.ActivateSpawn(other.gameObject);
            }
        }
    }
}
