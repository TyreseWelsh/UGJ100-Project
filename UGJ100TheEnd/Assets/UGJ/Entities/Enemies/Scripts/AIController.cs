using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject mesh;
    private Rigidbody enemyRigidbody;
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
    [SerializeField] private EnemyType currentEnemyType;
    
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
    private int currentHealth;
    private int maxHealth;
    private float currentSpeed;
    private bool isAttacking;
    private float currentStoppingDistance;

    private NavMeshPath targetPath;
    
    private Coroutine followTargetCoroutine;
    private Coroutine attackCoroutine;
    
    [Header("Damaged")]
    [SerializeField] private Material damageFlashMaterial;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private float knockbackForce = 60f;
    private SkinnedMeshRenderer[] damageableMeshes;

    
    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<BoxCollider>();
        enemyAnimator = GetComponent<Animator>();
        damageableMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        navAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public void Init(EnemyDataTemplate enemyData, GameObject playerRef)
    {
        playerCharacter = playerRef;
        
        currentEnemyType = (EnemyType) Enum.Parse(typeof(EnemyType), enemyData.type);
        currentHealth = enemyData.health;
        currentSpeed = enemyData.speed;
        navAgent.speed = currentSpeed;
        attackDamage = enemyData.damage;
        attackSpeed = enemyData.attackSpeed;
        navAgent.stoppingDistance = enemyData.attackDistance;
        
        distanceCheck.radius = enemyData.attackDistance;

        MainPlayerController.onPlayerDeath += FindNewPlayer;
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

                if (Vector3.Distance(transform.position, playerCharacter.transform.position) <=
                    navAgent.stoppingDistance)
                {
                    StopFollowingTarget();
                    Attack();
                }
                else
                {
                    targetPath = new NavMeshPath();
                    navAgent.CalculatePath(playerCharacter.transform.position, targetPath);

                    if (followTargetCoroutine == null)
                    {
                        followTargetCoroutine = StartCoroutine(FollowPlayer());
                    }
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

    IEnumerator FollowPlayer()
    {
        while (transform.position != targetPath.corners[targetPath.corners.Length - 1])
        {
            Vector3 directionToNextPoint = targetPath.corners[1] - transform.position;
        
            enemyRigidbody.velocity = directionToNextPoint.normalized * navAgent.speed;
            enemyAnimator.SetFloat("Speed", enemyRigidbody.velocity.magnitude * 100);
            print("anim = " + enemyAnimator.GetFloat("Speed"));
            yield return new WaitForSeconds(0.2f);
            yield return null;
        }
    }

    private void StopFollowingTarget()
    {
        enemyRigidbody.velocity = Vector3.zero;
        if (followTargetCoroutine != null)
        {
            StopCoroutine(followTargetCoroutine);
            followTargetCoroutine = null;
        }
    }
    
    private void Attack()
    {
        switch (currentEnemyType)
        {
            case EnemyType.Melee:
                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine("MeleeAttack");
                }
                break;
            case EnemyType.Ranged:
                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine("RangedAttack");
                }
                break;
            default:
                print("Error: " + gameObject.name + " has no current EnemyType!");
                break;
        }
    }
    
    private IEnumerator MeleeAttack()
    {
        yield return new WaitForSeconds(attackSpeed);
        
        enemyAnimator.SetTrigger("Attack");
        
        //
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider enemy in hitEnemies)
        {
            enemy.gameObject.GetComponent<IDamageable>().Damaged(10, gameObject);
        }
        
        StopCoroutine(attackCoroutine);
        attackCoroutine = null;
    }
    
    private IEnumerator RangedAttack()
    {
        yield return new WaitForSeconds(attackSpeed);
        
        enemyAnimator.SetTrigger("Attack");
        
        //
        Vector3 shootDirection = mesh.transform.forward;
        shootDirection.Normalize();
        GameObject curBullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, Quaternion.identity);
        Rigidbody bulletRB = curBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = shootDirection * bulletSpeed;
        
        StopCoroutine(attackCoroutine);
        attackCoroutine = null;
    }
    
    public void DamagedKnockback(GameObject knockbackSource)
    {
        Vector3 knockbackDirection = gameObject.transform.position - knockbackSource.transform.position;
        knockbackDirection.y = 0;
        enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
    }
    
    public IEnumerator DamageFlash(SkinnedMeshRenderer meshRender, Material startingMaterial, Material flashMaterial, float flashTime)
    {
        meshRender.material = flashMaterial;
        yield return new WaitForSeconds(flashTime);
        
        meshRender.material = startingMaterial;
    }
    
    public void Damaged(int damage, GameObject damageSource)
    {
        currentHealth -= damage;
        DamagedKnockback(damageSource);
        
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            StartCoroutine(DamageFlash(meshRenderer, originalMaterial, damageFlashMaterial, damageFlashDuration));
        }
        
        if(currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        StopAllCoroutines();
        ResetLimbMaterials();
        
        Destroy(mainCollider);
        Destroy(enemyAnimator);
        Destroy(navAgent);
        Destroy(attackPoint.gameObject);
        Destroy(distanceCheck.gameObject);
        
        // Enable this corpse
        gameObject.GetComponent<CorpseController>().enabled = true;
        
        Destroy(this);
    }
    
    void ResetLimbMaterials()
    {
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    void FindNewPlayer(GameObject newPlayer)
    {
        playerCharacter = newPlayer;
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
