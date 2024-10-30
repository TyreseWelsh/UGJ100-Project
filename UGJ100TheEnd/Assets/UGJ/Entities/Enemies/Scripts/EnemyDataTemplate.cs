using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_Data", menuName = "Enemy Data Template")]
public class EnemyDataTemplate : ScriptableObject
{
    public string type;
    public string enemyName;
    public int health;
    public float speed;
    public int damage;
    
    public float attackSpeed;
    public float attackDistance;
}
