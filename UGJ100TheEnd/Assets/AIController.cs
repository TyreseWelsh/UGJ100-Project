using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamagable
{
    [SerializeField] private GameObject playerCharacter;
    private NavMeshAgent navAgent;
    private Vector3 target;
    [SerializeField] private EnemyType enemiesAttackType;
    [Header("Ranged Stats")]
    [SerializeField] private int rangedStoppingDistance;
    [SerializeField] private int rangedHealth;
    [SerializeField] private float rangedSpeed;
    [Header("Melee Stats")]
    [SerializeField] private int meleeStoppingDistance;
    [SerializeField] private int meleeHealth;
    [SerializeField] private float meleeSpeed;
    private int health;
    private int maxHealth;



    // Start is called before the first frame update
    void Start()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        navAgent.SetDestination(playerCharacter.transform.position);
        switch (enemiesAttackType)
        {
            case EnemyType.Ranged:
                navAgent.stoppingDistance = rangedStoppingDistance;
                navAgent.speed = rangedSpeed;
                health = rangedHealth;
                break;
            case EnemyType.Melee:
                navAgent.stoppingDistance = meleeStoppingDistance;
                navAgent.speed = meleeSpeed;
                health = meleeHealth;
                break;
        }
        maxHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Damaged(float damage)
    {

    }

    enum EnemyType 
    {
        Ranged,
        Melee
    };
}
