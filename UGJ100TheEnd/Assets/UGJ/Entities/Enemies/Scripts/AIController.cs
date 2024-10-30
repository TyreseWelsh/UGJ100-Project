using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject mesh;
    private BoxCollider mainCollider;
    private Animator enemyAnimator;
    [SerializeField] private GameObject playerCharacter;
    private NavMeshAgent navAgent;
    private Vector3 target;
    [SerializeField] private SphereCollider distanceCheck;
    
    public enum EHealthStates {Alive, Dead}
    public EHealthStates currentHealthState = EHealthStates.Alive;
    
    enum EnemyType 
    {
        Ranged,
        Melee
    };
    [SerializeField] private EnemyType enemyType;
    
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
    
    private float attackSpeed;
    private int curHealth;
    private int maxHealth;
    private float curSpeed;
    private bool isShooting;
    private bool isHitting=false;
    private float curStoppingDistance;
    private bool isFollowingplayer = false;
    
    [Header("Damaged")]
    [SerializeField] private Material damageFlashMaterial;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    private SkinnedMeshRenderer[] damageableMeshes;

    
    private void Awake()
    {
        mainCollider = GetComponent<BoxCollider>();
        enemyAnimator = GetComponent<Animator>();
        damageableMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        navAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public void Init(EnemyDataTemplate enemyData, GameObject playerRef)
    {
        playerCharacter = playerRef;
        
        enemyType = (EnemyType) Enum.Parse(typeof(EnemyType), enemyData.type);
        curHealth = enemyData.health;
        curSpeed = enemyData.speed;
        navAgent.speed = curSpeed;
        attackDamage = enemyData.damage;
        attackSpeed = enemyData.attackSpeed;
        navAgent.stoppingDistance = enemyData.attackDistance;
        
        distanceCheck.radius = enemyData.attackDistance;

        MainPlayerController.onPlayerDeath += FindNewPlayer;
        StartCoroutine(_followPlayer());
    }
    
    // Update is called once per frame
    void Update()
    {
        if (currentHealthState != EHealthStates.Dead)
        {
            if (playerCharacter)
            {
                Vector3 lookDirection = playerCharacter.transform.position - transform.position;
                lookDirection.y = 0;
                mesh.transform.forward = lookDirection;

                switch (enemyType)
                {
                    case EnemyType.Ranged:
                        //Should put attacking logic here.
                        if (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) <=
                            navAgent.stoppingDistance)
                        {

                            if (!isShooting)
                            {
                                isShooting = true;
                                enemyAnimator.SetTrigger("Attack");
                                navAgent.speed = 0;
                                InvokeRepeating("shootBullet", 0.1f, 1.0f);
                            }

                        }
                        else
                        {
                            if (isShooting)
                            {
                                isShooting = false;
                                navAgent.speed = curSpeed;
                                CancelInvoke("shootBullet");
                            }
                        }

                        break;
                    case EnemyType.Melee:
                        //Should put attacking logic here
                        if (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) <=
                            navAgent.stoppingDistance)
                        {
                            if (!isHitting)
                            {
                                isHitting = true;
                                enemyAnimator.SetTrigger("Attack");
                                navAgent.speed = 0;
                                InvokeRepeating("meleeAttack", 0.1f, 1.0f);
                            }

                        }
                        else
                        {
                            if (isHitting)
                            {
                                isHitting = false;
                                navAgent.speed = curSpeed;
                                CancelInvoke("meleeAttack");
                            }
                        }

                        break;
                }
                
                Vector3 groundVelocity = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
                enemyAnimator.SetFloat("Speed", groundVelocity.magnitude * 100);
            }
            else
            {
                StopAllCoroutines();
            }
        }
    }

    /*private void FixedUpdate()
    {
        Vector3 movementDirection = playerCharacter.transform.position - transform.position;
        movementDirection.Normalize();
        if(Vector3.Distance(transform.position, playerCharacter.transform.position) <= rangedStoppingDistance)
    }*/

    void FindNewPlayer(GameObject newPlayer)
    {
        playerCharacter = newPlayer;
    }
    
    public IEnumerator DamageFlash(SkinnedMeshRenderer meshRender, Material startingMaterial, Material flashMaterial, float flashTime)
    {
        meshRender.material = flashMaterial;
        yield return new WaitForSeconds(flashTime);
        
        meshRender.material = startingMaterial;
    }
    
    public void Damaged(int damage, GameObject attacker)
    {
        curHealth -= damage;
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            StartCoroutine(DamageFlash(meshRenderer, originalMaterial, damageFlashMaterial, damageFlashDuration));
        }
        
        if(curHealth <= 0)
        {
            Die();
        }
    }

    void ResetLimbMaterials()
    {
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            meshRenderer.material = originalMaterial;
        }
    }
    
    private void Die()
    {
        StopAllCoroutines();
        ResetLimbMaterials();
        
        Destroy(mainCollider);
        Destroy(navAgent);
        Destroy(attackPoint.gameObject);
        Destroy(distanceCheck.gameObject);
        
        // Enable this corpse
        gameObject.GetComponent<CorpseController>().enabled = true;
        
        Destroy(this);
    }
    
    private void shootBullet()
    {
        Debug.Log("Shooting");
        //var direction = playerCharacter.transform.position - bulletSpawn.transform.position;
        Vector3 shootDirection = mesh.transform.forward;
        shootDirection.Normalize();
        GameObject curBullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, Quaternion.identity);
        Rigidbody bulletRB = curBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = shootDirection * bulletSpeed;
        
        navAgent.speed = curSpeed;

    }
    private void meleeAttack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
            enemy.gameObject.GetComponent<IDamageable>().Damaged(10, gameObject);
        }
        navAgent.speed = curSpeed;
    }

    /*public void Interact(GameObject interactingObj)
    {
        if (navAgent.isOnNavMesh)
        {
            StartCoroutine(_followPlayer());
        }
    }*/
    public void InteractHeld(GameObject interactingObj) { }

    IEnumerator _followPlayer()
    {
        /*while (Vector3.Distance(gameObject.transform.position, playerCharacter.transform.position) >= navAgent.stoppingDistance)
        {*/
            navAgent.SetDestination(playerCharacter.transform.position);
            enemyAnimator.SetFloat("Speed", navAgent.velocity.magnitude);
            yield return new WaitForSeconds(0.2f);
            yield return null;
            
            StartCoroutine(_followPlayer());
       // }
    }

    private void OnTriggerExit(Collider other)
    {
        /*if(other.gameObject.tag == "Player")
        {
            StartCoroutine(_followPlayer());
        }*/
    }

    private void OnDisable()
    {
        MainPlayerController.onPlayerDeath -= FindNewPlayer;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
