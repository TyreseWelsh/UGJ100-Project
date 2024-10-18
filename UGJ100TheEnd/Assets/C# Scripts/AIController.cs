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
    [SerializeField] private int bulletSpeed;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletSpawn;
    [Header("Melee Stats")]
    [SerializeField] private int meleeStoppingDistance;
    [SerializeField] private int meleeHealth;
    [SerializeField] private float meleeSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] Transform attackPoint;
    [SerializeField] int attackDamage;
    private int curHealth;
    private int maxHealth;
    private bool isShooting;
    private bool isHitting=false;



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
                maxHealth = rangedHealth;
                break;
            case EnemyType.Melee:
                navAgent.stoppingDistance = meleeStoppingDistance;
                navAgent.speed = meleeSpeed;
                maxHealth = meleeHealth;
                break;
        }
        curHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (navAgent.isStopped)
        {
            navAgent.SetDestination(playerCharacter.transform.position);
            navAgent.Move(new Vector3(0,0,0));
        }
        switch (enemiesAttackType)
        {
            case EnemyType.Ranged:
                //Should put attacking logic here.
                if (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) <= 9)
                {
                    Debug.Log("In Range");
                    if (!isShooting)
                    {
                        isShooting = true;
                        InvokeRepeating("shootBullet", 0.1f, 1.0f);
                    }
                    
                }
                else
                {
                    if (isShooting)
                    {
                        isShooting = false;
                        CancelInvoke("shootBullet");
                    }
                }
                break;
            case EnemyType.Melee:
                //Should put attacking logic here
                if(Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) <= 2f)
                {
                    if (!isHitting)
                    {
                        Debug.Log("Trying to hit?");
                        isHitting = true;
                        InvokeRepeating("meleeAttack", 0.1f, 1.0f);
                    }

                }
                else
                {
                    if (isHitting)
                    {
                        isHitting = false;
                        CancelInvoke("meleeAttack");
                    }
                }
                break;
        }
    }

    public void Damaged(int damage)
    {
        curHealth -= damage;
    }

    private void shootBullet()
    {
        Debug.Log("Shooting");
        var direction = playerCharacter.transform.position - bulletSpawn.transform.position;
        direction = direction.normalized;
        GameObject curBullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, Quaternion.identity);
        Rigidbody bulletRB = curBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = direction * bulletSpeed;

    }
    private void meleeAttack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
            enemy.GetComponent<IDamagable>().Damaged(attackDamage);
        }
    }

    enum EnemyType 
    {
        Ranged,
        Melee
    };
}
