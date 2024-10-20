using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamagable
{
    [SerializeField] private GameObject playerCharacter;
    private NavMeshAgent navAgent;
    private Vector3 target;
    [SerializeField] private SphereCollider distanceCheck;
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
    private int curStoppingDistance;
    private bool isFollowingplayer = false;


    // Start is called before the first frame update
    void Start()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        switch (enemiesAttackType)
        {
            case EnemyType.Ranged:
                curStoppingDistance = rangedStoppingDistance;
                navAgent.stoppingDistance = curStoppingDistance;
                navAgent.speed = rangedSpeed;
                maxHealth = rangedHealth;
                distanceCheck.radius = curStoppingDistance;
                break;
            case EnemyType.Melee:
                curStoppingDistance = meleeStoppingDistance;
                navAgent.stoppingDistance = curStoppingDistance;
                navAgent.speed = meleeSpeed;
                maxHealth = meleeHealth;
                distanceCheck.radius = curStoppingDistance;
                break;
        }
        curHealth = maxHealth;
        StartCoroutine(_followPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
        switch (enemiesAttackType)
        {
            case EnemyType.Ranged:
                //Should put attacking logic here.
                if (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) <= 9)
                {
                    
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
        if(curHealth <= 0)
        {

        }
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
            enemy.gameObject.GetComponent<IDamagable>().Damaged(10);
        }
    }

    IEnumerator _followPlayer()
    {
        while (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) >= curStoppingDistance)
        {
            
            navAgent.SetDestination(playerCharacter.transform.position);
            yield return new WaitForSeconds(0.2f);
            yield return null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            
            StartCoroutine(_followPlayer());
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    enum EnemyType 
    {
        Ranged,
        Melee
    };
}
