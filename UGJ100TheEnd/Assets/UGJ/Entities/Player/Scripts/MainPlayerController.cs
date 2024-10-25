using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerController : MonoBehaviour, IDamageable, ICanHoldCorpse
{
    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mesh;
    [SerializeField] public GameObject pickupPosition;
    [SerializeField] private LayerMask interactableObjectLayer;
    [SerializeField] private GameObject meleeAttackPoint;

    [Header("")] 
    private CharacterController characterController;
    private Animator characterAnimator;
    private PlayerInput playerInput;
    private StaminaComponent staminaComponent;
    
    [Header("Basic Stats")]
    [SerializeField] public int maxHealth = 100;
    [SerializeField] private float maxSpeed = 9;
    [SerializeField] private int lives;
    public enum EHealthStates {Alive, Reviving, Dead}
    public EHealthStates currentHealthState = EHealthStates.Alive;
    
    [Header("Dash Stats")]
    [SerializeField] private float dashSpeed = 100;
    [SerializeField] private float dashDuration = 0.05f;
    [SerializeField] private int dashCost = 40;
    
    [Header("")]
    [SerializeField] private float reviveDuration = 2.5f;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public float currentSpeed;

    private Vector3 movementDirection;
    private bool gravityOn = true;
    private bool holdingCorpse;
    private GameObject heldCorpse;
    private ConfigurableJoint heldCorpseJoint;

    [Header("Melee Attack")] 
    public int meleeDamage = 10;
    [SerializeField] private float meleeAttackRate = 0.6f;
    [SerializeField] private float meleeAttackRange = 2f;
    private MeleeWeapon meleeWeapon;
    private bool canAttack = true;
    private Coroutine meleeCoroutine;

    [Header("Ranged Attack")] 
    [SerializeField] private float currentThrowForce;
    [SerializeField] private float throwChargeRate = 0.05f;
    [SerializeField] private int maxThrowForce = 30;
    private Vector3 throwVector;
    private bool chargingThrow;
    Coroutine throwChargeCoroutine;

    [Header("Blocking")]
    [SerializeField] private int blockCost;
    [SerializeField] private float blockConsumptionRate;
    [SerializeField] private float brokenBlockRegenDelay = 4;
    private Coroutine blockCoroutine;
    private bool isBlocking;
    
    [Header("HUD")]
    [SerializeField] private HUD playerHUD;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterAnimator = GetComponent<Animator>();
        playerInput  = GetComponent<PlayerInput>();
        staminaComponent = GetComponent<StaminaComponent>();

        MeleeWeapon[] meleeWeapons = GetComponentsInChildren<MeleeWeapon>();
        meleeWeapon = meleeWeapons[0];
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentSpeed = maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealthState != EHealthStates.Dead)
        {
            if (chargingThrow)
            {
                Debug.Log(currentThrowForce);
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
        if (gravityOn)
        {
            characterController.Move(Time.deltaTime * 20f * Vector3.down);
        }

        if (currentHealthState != EHealthStates.Dead)
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //Debug.Log(animationDirectionRotation);
            characterController.Move(currentSpeed * Time.deltaTime * movementDirection);
            
            Quaternion moveRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            Vector3 animationDirectionRotation = mesh.gameObject.transform.rotation.eulerAngles - moveRotation.eulerAngles;
            if (animationDirectionRotation.y > 180)
            {
                animationDirectionRotation.y -= 360;
            }
            characterAnimator.SetFloat("Rotation", animationDirectionRotation.y);
            Debug.Log(animationDirectionRotation.y);
            
            Vector3 groundVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
            characterAnimator.SetFloat("Speed", groundVelocity.magnitude * 100);
        }
    }
    
    public void Look(InputAction.CallbackContext context)
    {
        //Debug.Log("LOOKING");
        if (context.performed)
        {
            if (currentHealthState != EHealthStates.Dead)
            {
                // Getting the screen mouse position when the mouse is moved
                Vector2 screenMousePosition = context.ReadValue<Vector2>();

                // 
                float cameraToPlayerDistance = Mathf.Abs(mainCamera.transform.position.y - transform.position.y);
                Vector3 mousePoint = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePosition.x,
                    screenMousePosition.y, cameraToPlayerDistance));
                mousePoint.y = transform.position.y;

                mesh.transform.LookAt(mousePoint);
            }
        }
    }

    public void Melee(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (canAttack)
            {
                Debug.Log("ATTACK!!");
                //characterAnimator.SetLayerWeight(1, 1);
                characterAnimator.SetTrigger("Attack");
                canAttack = false;
            }
        }
    }

    public void EnableMeleeCollision()
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.EnableWeapon();
        }
    }

    public void DisableMeleeCollision()
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.DisableWeapon();
        }
    }
    
    public void EndMelee()
    {
        //characterAnimator.ResetTrigger("Attack");
        //characterAnimator.SetLayerWeight(1, 0);
        canAttack = true;
    }
    
    /*IEnumerator Melee()
    {
        // Attack stuff
        if (meleeAttackPoint != null)
        {
            canAttack = false;
            Collider[] meleeCollisions = Physics.OverlapSphere(meleeAttackPoint.transform.position, meleeAttackRange);
            foreach (Collider meleeCollision in meleeCollisions)
            {
                GameObject collidingObject = meleeCollision.gameObject;
                if (collidingObject.gameObject.GetComponent<IDamageable>() != null && collidingObject.gameObject.CompareTag("Player"))
                {
                    collidingObject.gameObject.GetComponent<IDamageable>().Damaged(meleeDamage);
                }
            }
            print("MELEE!");
            yield return new WaitForSeconds(meleeAttackRate);

            canAttack = true; 
        }
    }*/

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
            // Launch object
            if (heldCorpse != null)
            {
                throwVector = mesh.transform.forward * currentThrowForce * 10;
                throwVector.y = 40;
                print("THROWWWW : " + throwVector);
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
        while (staminaComponent.currentStamina > staminaComponent.negStaminaLimit)
        {
            Debug.Log("BLOCKING");
            staminaComponent.ConsumeStamina(blockCost);
            
            yield return new WaitForSeconds(blockConsumptionRate);
            yield return null;
        }
      
        StopBlocking();
    }
    
    void StopBlocking()
    {
        Debug.Log("Stopped blocking");
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
            gravityOn = false;
            characterController.Move(dashSpeed * Time.deltaTime * direction);
            
            yield return null;
        }
        ToggleInvincibility(false);
        characterController.velocity.Set(0, 0, 0);
        gravityOn = true;
    }
    
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Debug.Log("Pressed");
            if (currentHealthState == EHealthStates.Alive)
            {
                if (holdingCorpse)
                {
                    DropCorpse();
                    return;
                }
                
                RaycastHit hit;
                Physics.Raycast(transform.position, mesh.transform.forward * 2, out hit, interactableObjectLayer);
                Debug.DrawRay(transform.position, mesh.transform.forward * 2, Color.red, 0.5f);

                // if hit object is not null
                if (hit.collider)
                {   
                    if(hit.collider.gameObject != null)
                    {
                        if(hit.collider.gameObject.GetComponent<IInteractable>() != null)
                        {
                            hit.collider.gameObject.GetComponent<IInteractable>().Interact(gameObject);
                            if (hit.collider.gameObject.CompareTag("Corpse"))
                            {
                                PickupCorpse(hit.collider.gameObject);
                            }
                        }
                    }
                }
            }
        }
        if (context.performed) 
        {
            Debug.Log("Held");
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
        if (context.performed)
        {
            Debug.Log("Damaged self");
            Damaged(maxHealth);
        }
    }
    
    public void StartRevive()
    {
        Debug.Log("REVIVE!");
        currentHealthState = EHealthStates.Reviving;
        ToggleInvincibility(true);

        currentHealth = maxHealth;
        staminaComponent.currentStamina = staminaComponent.maxStamina;
        currentSpeed /= 2;
        playerHUD.SetReferencedPlayer(gameObject);
        
        StartCoroutine(ReviveCoroutine(Time.time));
    }

    IEnumerator ReviveCoroutine(float startTime)
    {
        while (Time.time < startTime + reviveDuration)
        {
            //Debug.Log("Reviving...");
            yield return null;
        }
        
        Revive();
    }

    void Revive()
    {
        Debug.Log("REVIVED!!");
        currentHealth = maxHealth;
        currentSpeed = maxSpeed;
        ToggleInvincibility(false);
        
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
    private void Die()
    {
        InputActionAsset playerInputAsset = playerInput.actions;
        playerInput.actions = null;
        
        // Creating new player
        GameObject newPlayer = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
        MainPlayerController newPlayerScript = newPlayer.GetComponent<MainPlayerController>();
        if (newPlayerScript != null)
        {
            Debug.Log("Spawn new player");
            newPlayerScript.StartRevive();
            newPlayerScript.playerInput.actions = playerInputAsset;
            newPlayerScript.lives = lives;
        }
        
        Debug.Log("Destroy scripts");

        // Disable this player
        Destroy(characterController);
        Destroy(characterAnimator);
        Destroy(playerInput);
        Destroy(staminaComponent);

        Destroy(mainCamera.gameObject);
        
        // Enable this corpse
        gameObject.GetComponent<CorpseController>().enabled = true;
        
        Debug.Log("Scripts destroyed and ragdoll enabled");
        
        
        Destroy(this);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            lives--;

            if (lives > 0)
            {
                Die();
            }
            else
            {
                Debug.Log("NO PLAYER LIVES REMAINING - Restart game!");
                currentHealthState = EHealthStates.Dead;
            }
        }
    }
    
    public void Damaged(int damage)
    {
        if (isBlocking)
        {
            if (heldCorpse != null)
            {
                IDamageable damageableCorpse = heldCorpse.GetComponent<IDamageable>();
                if (damageableCorpse != null)
                {
                    damageableCorpse.Damaged(damage);
                }
            }
            else
            {
                switch (staminaComponent.ConsumeStamina(damage))
                {
                    case StaminaComponent.EStaminaAbilityStrength.Full:
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Reduced:
                        TakeDamage(damage / 2);
                        break;
                    case StaminaComponent.EStaminaAbilityStrength.Zero:
                        Debug.Log("BLOCK BROKEN!!!");
                        StopBlocking();
                        staminaComponent.currentStamina = staminaComponent.negStaminaLimit;
                        TakeDamage(damage / 2);
                        staminaComponent.StartRegenDelay(brokenBlockRegenDelay);
                        break;
                    default:
                        TakeDamage(damage);
                        break;
                }
            }
        }
        else
        {
            TakeDamage(damage);
        }
    }
}
