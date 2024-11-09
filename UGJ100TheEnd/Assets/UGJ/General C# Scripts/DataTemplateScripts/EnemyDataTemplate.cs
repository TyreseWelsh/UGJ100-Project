using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data Template")]
public class EnemyDataTemplate : CharacterDataTemplate
{
    [Header("Enemy Data")]
    public string type;
    public string enemyName;
    
    public float attackSpeed;
    public float attackDistance;
    
    public GameObject projectile;
}
