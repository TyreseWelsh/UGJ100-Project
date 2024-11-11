using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPlayerController : MonoBehaviour, IDamageable, ICanHoldCorpse
{
    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mesh;
    [SerializeField] public GameObject pickupPosition;
    [SerializeField] private LayerMask interactableObjectLayer;
    [SerializeField] private GameObject meleeAttackPoint;

    [Header("")]
    [SerializeField] private Material meshMaterial;
    [SerializeField] private GameObject durabilityBar;

    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCollider;
    private Animator characterAnimator;
    private PlayerInput playerInput;
    private AttackComponent attackComponent;
    private StaminaComponent staminaComponent;
    
    [Header("Basic Stats")]
    [SerializeField] private PlayerDataTemplate playerData;
    public int maxHealth;
    private float maxSpeed;
    public int lives;
    public enum EHealthStates {Alive, Reviving, Dead}
    public EHealthStates currentHealthState = EHealthStates.Alive;
    private Vector3 mousePoint;
    
    [Header("Dash Stats")]
    private float dashSpeed;
    private float dashDuration;
    private int dashCost;
    
    [Header("Reviving")]
    private float reviveDuration;
    private Material reviveMaterial;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public float currentSpeed;

    private Vector3 movementDirection;
    private bool gravityOn;
    private bool holdingCorpse;
    private GameObject heldCorpse;
    private ConfigurableJoint heldCorpseJoint;

    [Header("Ranged Attack")] 
    [SerializeField] private GameObject throwStartPoint;
    private float currentThrowForce;
    private float throwChargeRate;
    private int maxThrowForce;
    private Vector3 throwVector;
    private bool chargingThrow;
    Coroutine throwChargeCoroutine;

    [Header("Blocking")] 
    private float parryDuration;
    private float parryStaminaGain;
    private int blockCost;
    private float blockConsumptionRate;
    private float brokenBlockRegenDelay;
    private Coroutine blockCoroutine;
    private bool isBlocking;
    private bool isParrying;

    [Header("Damaged")] 
    private Material damageFlashMaterial;
    private Material originalMaterial;
    private float damageFlashDuration;
    private float knockbackForce = 60f;
    private SkinnedMeshRenderer[] damageableMeshes;

    public delegate void OnPlayerDeath(GameObject newPlayer);
    public static OnPlayerDeath onPlayerDeath;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        characterAnimator = GetComponent<Animator>();
        playerInput  = GetComponent<PlayerInput>();
        staminaComponent = GetComponent<StaminaComponent>();
        attackComponent = GetComponent<AttackComponent>();

        InitPlayerData();
        attackComponent.InitData(playerData);
        currentHealth = maxHealth;
        currentSpeed = maxSpeed;
        
        damageableMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void InitPlayerData()
    {
        if (playerData)
        {
            // Base character data
            maxHealth = playerData.health;
            maxSpeed = playerData.moveSpeed;
            damageFlashMaterial = playerData.damageFlashMaterial;
            originalMaterial = playerData.originalMaterial;
            damageFlashDuration = playerData.damageFlashDuration;
            knockbackForce = playerData.knockbackForce;
        
            // Player data
            lives = playerData.lives;
            dashSpeed = playerData.dashSpeed;
            dashDuration = playerData.dashDuration;
            dashCost = playerData.dashCost;
            
            reviveDuration = playerData.reviveDuration;
            reviveMaterial = playerData.reviveMaterial;
            
            throwChargeRate = playerData.throwChargeRate;
            maxThrowForce = playerData.maxThrowForce;
            
            parryDuration = playerData.parryDuration;
            parryStaminaGain = playerData.parryStaminaGain;
            blockCost = playerData.blockCost;
            blockConsumptionRate = playerData.blockConsumptionRate;
            brokenBlockRegenDelay = playerData.brokenBlockRegenDelay;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (currentHealthState != EHealthStates.Dead)
        {
            // Movement and Rotation
            //movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Quaternion moveRotation = Quaternion.identity;
            
            if (movementDirection != Vector3.zero)
            {
                moveRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            }
            Vector3 animationDirectionRotation = mesh.gameObject.transform.rotation.eulerAngles - moveRotation.eulerAngles;
            if (animationDirectionRotation.y > 180)
            {
                animationDirectionRotation.y -= 360;
            }
            characterAnimator.SetFloat("Rotation", animationDirectionRotation.y);
            
            // Throw
            if (chargingThrow)
            {
                currentThrowForce += throwChargeRate * Time.deltaTime;
                if (currentThrowForce > maxThrowForce)
                {
                    currentThrowForce = maxThrowForce;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Applying constant gravity
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Physics.gravity.y, playerRigidbody.velocity.z);
        
        if (currentHealthState != EHealthStates.Dead)
        {
            movementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxis("Vertical"));
            movementDirection.Normalize();
            if (movementDirection != Vector3.zero)
            {
                playerRigidbody.velocity =  new Vector3(movementDirection.x * currentSpeed, playerRigidbody.velocity.y, movementDirection.z * currentSpeed);
            }
            
            Vector3 groundVelocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            characterAnimator.SetFloat("Speed", groundVelocity.magnitude * 100);
        }
        
        print("Y Velocity = " + playerRigidbody.velocity.y);
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentHealthState != EHealthStates.Dead)
            {
                // Getting the screen mouse position when the mouse is moved
                Vector2 screenMousePosition = context.ReadValue<Vector2>();

                //
                float cameraToPlayerDistance = Mathf.Abs(mainCamera.transform.position.y - transform.position.y);
                mousePoint = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePosition.x,
                    screenMousePosition.y, cameraToPlayerDistance));

                Vector3 LookDirection = mousePoint - transform.position;
                LookDirection.y = 0;
                
                mesh.transform.forward = LookDirection;
            }
        }
    }

    public void Melee(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (attackComponent)
            {
                attackComponent.StartAttack();
            }
        }
    }

    public void StartThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (heldCorpse != null)
            {
                chargingThrow = true;
            }
        }
        else if (context.canceled)
        {
            chargingThrow = false;
            if (heldCorpse != null)
            {
                throwVector = mesh.transform.forward * currentThrowForce * 10;
                throwVector.y = 50;
                heldCorpse.gameObject.transform.position = throwStartPoint.transform.position;
                heldCorpse.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
                DropCorpse();
            }
            currentThrowForce = 0;
        }
    }

    public void StartBlock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isParrying = true;
            isBlocking = true;
            if (blockCoroutine != null)
            {
                StopCoroutine(blockCoroutine);
            }
            blockCoroutine = StartCoroutine(Block());
        }

        if (context.canceled)
        {
            StopBlocking();
        }
    }

    IEnumerator Block()
    {
        float startTime = Time.time;
        
        while (staminaComponent.currentStamina > staminaComponent.negStaminaLimit)
        {
            if (Time.time > startTime + parryDuration)
            {
                isParrying = false;
            }
            
            staminaComponent.ConsumeStamina(blockCost);
            
            yield return new WaitForSeconds(blockConsumptionRate);
            yield return null;
        }
      
        StopBlocking();
    }
    
    void StopBlocking()
    {
        isParrying = false;
        isBlocking = false;
        if (blockCoroutine != null)
        {
            StopCoroutine(blockCoroutine);
        }
    }
    
    public void StartDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentHealthState == EHealthStates.Alive)
            {
                float dashStartTime = Time.time;

                switch (staminaComponent.ConsumeStamina(dashCost))
                {
                    case StaminaComponent.EStaminaAbilityStrength.Full:
                        ToggleInvincibility(true);
                        StartCoroutine(Dash(movementDirection, dashStartTime, dashDuration));
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Reduced:
                        StartCoroutine(Dash(movementDirection, dashStartTime, dashDuration / 2));
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Zero:
                        // Not enough stamina
                        break;
                    default:
                        // Default: Full power
                        StartCoroutine(Dash(movementDirection, dashStartTime, dashDuration));
                        break;
                }
            }
        }
    }
    
    IEnumerator Dash(Vector3 direction, float startTime, float duration)
    {
        while (Time.time < startTime + duration)
        {
            playerRigidbody.useGravity = false;
            playerRigidbody.AddForce(dashSpeed * Time.deltaTime * direction * 1100, ForceMode.Force);
            
            yield return null;
        }
        ToggleInvincibility(false);
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.useGravity = true;
    }
    
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            if (currentHealthState == EHealthStates.Alive)
            {
                if (holdingCorpse)
                {
                    DropCorpse();
                    return;
                }
                
                Collider[] interactingObjects = Physics.OverlapSphere(throwStartPoint.transform.position, 0.6f, interactableObjectLayer);

                // if hit object is not null
                if (interactingObjects.Length > 0)
                {
                    GameObject interactingObject = interactingObjects[0].gameObject;
                    if(interactingObject != null)
                    {
                        if(interactingObject.GetComponent<IInteractable>() != null)
                        {
                            interactingObject.GetComponent<IInteractable>().Interact(gameObject);
                            if (interactingObject.CompareTag("Corpse") && heldCorpse == null)
                            {
                                PickupCorpse(interactingObject);
                            }
                        }
                    }
                }
            }
        }
        if (context.performed) 
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, mesh.transform.forward * 2, out hit);
            Debug.DrawRay(transform.position, mesh.transform.forward * 2, Color.red, 0.5f);
            if (hit.collider)
            {
                if (hit.collider.gameObject.GetComponent<IInteractable>() != null)
                {
                    hit.collider.gameObject.GetComponent<IInteractable>().InteractHeld(gameObject);
                }
            }
        }
    }

    public void PickupCorpse(GameObject corpse)
    {
        heldCorpse = corpse;
        durabilityBar.SetActive(true);
        if (heldCorpse.GetComponent<CorpseController>() != null)
        {
            heldCorpse.GetComponent<CorpseController>().Pickup(gameObject, pickupPosition);
        }
        holdingCorpse = true;
    }

    public void DropCorpse()
    {
        if (heldCorpse != null)
        {
            if (heldCorpse.GetComponent<CorpseController>() != null)
            {
                durabilityBar.SetActive(false);
                heldCorpse.GetComponent<CorpseController>().Drop();
            }
        }
        
        CorpseHasBeenDropped();
    }

    void CorpseHasBeenDropped()
    {
        heldCorpse = null;
        holdingCorpse = false;
        
        chargingThrow = false;
        currentThrowForce = 0;
    }

    public void DamageSelf(InputAction.CallbackContext context)
    {
        /*if (context.performed)
        {
            Damaged(maxHealth, gameObject);
        }*/
    }

    void SetLimbMaterials(Material newMaterial)
    {
        foreach (SkinnedMeshRenderer meshRenderer in damageableMeshes)
        {
            meshRenderer.material = newMaterial;
        }
    }
    
    public void StartRevive(GameObject attacker)
    {
        currentHealthState = EHealthStates.Reviving;
        ToggleInvincibility(true);
        attackComponent?.SetCanAttack(false);
        SetLimbMaterials(reviveMaterial);
        if (attackComponent)
        {
            if (attackComponent.GetCurrentWeapon())
            {
                attackComponent.GetCurrentWeapon().GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }
        playerRigidbody.velocity = Vector3.zero;
        currentHealth = maxHealth;
        currentSpeed /= 1.2f;
        staminaComponent.currentStamina = staminaComponent.maxStamina;
        
        StartCoroutine(ReviveCoroutine(Time.time));
    }

    IEnumerator ReviveCoroutine(float startTime)
    {
        while (Time.time < startTime + reviveDuration)
        {
            yield return null;
        }
        
        Revive();
    }

    void Revive()
    {
        SetLimbMaterials(meshMaterial);
        attackComponent.GetCurrentWeapon().gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        currentSpeed = maxSpeed;
        StopAllCoroutines();
        
        attackComponent?.SetCanAttack(true);
        ToggleInvincibility(false);
        onPlayerDeath?.Invoke(gameObject);
        currentHealthState = EHealthStates.Alive;
    }

    void ToggleInvincibility(bool isInvincible)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), isInvincible);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bullet"), isInvincible);
    }
    
    // Enable ragdoll 
    // Disable relevant player components
    // Enable relevant corpse components
    // Spawn a new player object and call its StartRevive function
    private void Die(GameObject attacker)
    {
        StopAllCoroutines();
        SetLimbMaterials(meshMaterial);
        
        InputActionAsset playerInputAsset = playerInput.actions;
        playerInput.actions = null;
        
        // Creating new player
        GameObject newPlayer = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
        newPlayer.transform.SetParent(gameObject.transform.parent);
        MainPlayerController newPlayerScript = newPlayer.GetComponent<MainPlayerController>();
        if (newPlayerScript != null)
        {
            newPlayerScript.StartRevive(attacker);
            newPlayerScript.playerInput.actions = playerInputAsset;
            newPlayerScript.lives = lives;
        }
        
        print("Now destroy scripts");
        // Disable this player
        Destroy(attackComponent.GetCurrentWeapon());
        Destroy(playerCollider);
        Destroy(characterAnimator);
        Destroy(playerInput);
        Destroy(attackComponent);
        Destroy(staminaComponent);
        Destroy(mainCamera.gameObject);
        Destroy(pickupPosition);
        
        // Enable this corpse
        gameObject.GetComponent<CorpseController>().enabled = true;
        
        Destroy(this);
    }

    public void DamagedKnockback(GameObject knockbackSource)
    {
        Vector3 knockbackDirection = gameObject.transform.position - knockbackSource.transform.position;
        knockbackDirection.y = 0;
        playerRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
    }
    
    private void TakeDamage(int damage, GameObject attacker)
    {
        if (currentHealthState == EHealthStates.Alive)
        {
            currentHealth -= damage;
            DamagedKnockback(attacker);
            
            foreach (SkinnedMeshRenderer meshRender in damageableMeshes)
            {
                StartCoroutine(DamageFlash(meshRender, originalMaterial, damageFlashMaterial,damageFlashDuration));
            }
        
            if (currentHealth <= 0)
            {
                lives--;

                if (lives > 0)
                {
                    Die(attacker);
                }
                else
                {
                    currentHealthState = EHealthStates.Dead;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
    }

    public IEnumerator DamageFlash(SkinnedMeshRenderer meshRender, Material startingMaterial, Material flashMaterial, float flashTime)
    {
        meshRender.material = flashMaterial;
        yield return new WaitForSeconds(flashTime);
        
        meshRender.material = originalMaterial;
    }
    
    public void Damaged(int damage, GameObject damageSource)
    {
        if (isParrying)
        {
            staminaComponent.GainStamina(parryStaminaGain);
        }
        else if (isBlocking)
        {
            if (heldCorpse != null)
            {
                IDamageable damageableCorpse = heldCorpse.GetComponent<IDamageable>();
                if (damageableCorpse != null)
                {
                    damageableCorpse.Damaged(damage, gameObject);
                }
            }
            else
            {
                switch (staminaComponent.ConsumeStamina(damage))
                {
                    case StaminaComponent.EStaminaAbilityStrength.Full:
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Reduced:
                        TakeDamage(damage / 2, damageSource);
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Zero:
                        StopBlocking();
                        staminaComponent.currentStamina = staminaComponent.negStaminaLimit;
                        TakeDamage(damage / 2, damageSource);
                        staminaComponent.StartRegenDelay(brokenBlockRegenDelay);
                        break;
                    default:
                        TakeDamage(damage, damageSource);
                        break;
                }
            }
        }
        else
        {
            TakeDamage(damage, damageSource);
        }
    }
}
