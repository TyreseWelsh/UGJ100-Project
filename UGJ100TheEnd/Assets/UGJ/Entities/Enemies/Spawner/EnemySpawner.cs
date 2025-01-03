using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyDataTemplate meleeEnemyData;
    [SerializeField] private EnemyDataTemplate rangedEnemyData;
    enum EnemyType 
    {
        Ranged,
        Melee
    };
    [SerializeField] private EnemyType currentEnemyType;
    [SerializeField] private GameObject enemyTemplate;
    
    private GameObject player;
    

    public void ActivateSpawn(GameObject playerRef)
    {
        player = playerRef;
        switch (currentEnemyType)
        {
            case(EnemyType.Melee):
                SpawnEnemy(meleeEnemyData);
                break;
            case(EnemyType.Ranged):
                SpawnEnemy(rangedEnemyData);
                break;
            default:
                // Melee
                break;
        }
    }

    private void SpawnEnemy(EnemyDataTemplate data)
    {
        GameObject spawnedEnemy = Instantiate(enemyTemplate, transform.position, Quaternion.identity);
        if (spawnedEnemy != null)
        {
            AIController enemyScript = spawnedEnemy.GetComponent<AIController>();
            if (enemyScript)
            {
                enemyScript.Init(data, player);
            }

            AttackComponent_Enemy attackScript = spawnedEnemy.GetComponent<AttackComponent_Enemy>();
            if (attackScript)
            {
                attackScript.InitData(data);
            }
        }
    }
}
