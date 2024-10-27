using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private List<EnemySpawner> spawners;
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!activated)
            {
                activated = true;
                foreach (EnemySpawner spawner in spawners)
                {
                    spawner.ActivateSpawn(other.gameObject);
                }
            }
        }
    }
}
