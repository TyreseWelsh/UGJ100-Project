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
    private NavMeshAgent navAgent;
    private AttackComponent attackComponent;
    
    [SerializeField] private GameObject playerCharacter;
    private Vector3 target;
    
    public enum EHealthStates {Alive, Dead}
    public EHealthStates currentHealthState = EHealthStates.Alive;
    
    enum EnemyType 
    {
        Ranged,
        Melee
    };
    [SerializeField] private EnemyType currentEnemyType;
    
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

        attackComponent = GetComponent<AttackComponent>();
    }

    public void Init(EnemyDataTemplate enemyData, GameObject playerRef)
    {
        playerCharacter = playerRef;
        
        currentEnemyType = (EnemyType) Enum.Parse(typeof(EnemyType), enemyData.type);
        currentHealth = enemyData.health;
        currentSpeed = enemyData.moveSpeed;
        navAgent.speed = currentSpeed;
        attackSpeed = enemyData.attackSpeed;
        navAgent.stoppingDistance = enemyData.attackDistance;
        
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
                    if (attackComponent)
                    {
                        attackComponent.StartAttack();
                    }
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
                
                Vector3 groundVelocity = new Vector3(enemyRigidbody.velocity.x, 0, enemyRigidbody.velocity.z);
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
        
            enemyRigidbody.velocity = directionToNextPoint.normalized * currentSpeed;

            enemyAnimator.SetFloat("Speed", enemyRigidbody.velocity.magnitude * 100);
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
        
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            StartCoroutine(DamageFlash(meshRenderer, originalMaterial, damageFlashMaterial, damageFlashDuration));
        }
        
        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            DamagedKnockback(damageSource);
        }
        
    }
    
    private void Die()
    {
        StopAllCoroutines();
        ResetLimbMaterials();
        
        Destroy(mainCollider);
        Destroy(enemyAnimator);
        Destroy(navAgent);
        
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

    public GameObject GetMesh()
    {
        return mesh;
    }
    
    private void OnDisable()
    {
        MainPlayerController.onPlayerDeath -= FindNewPlayer;
    }
}
