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
    [SerializeField] private GameObject pickupPosition;
    [SerializeField] private LayerMask interactableObjectLayer;
    [SerializeField] private GameObject meleeAttackPoint;

    [Header("")] 
    //private Rigidbody rb;
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
    private bool holdingCorpse;
    private GameObject heldCorpse;
    private ConfigurableJoint heldCorpseJoint;

    [Header("Melee Attack")]
    private bool canAttack = true;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeAttackRate = 0.6f;
    [SerializeField] private float meleeAttackRange = 2f;

    [Header("Ranged Attack")] 
    [SerializeField] private float currentThrowForce;
    [SerializeField] private float throwChargeRate = 0.05f;
    [SerializeField] private int maxThrowForce = 30;
    private Vector3 throwVector;
    private bool chargingThrow;
    Coroutine throwChargeCoroutine;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        characterAnimator = GetComponent<Animator>();
        playerInput  = GetComponent<PlayerInput>();
        staminaComponent = GetComponent<StaminaComponent>();
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
        if (currentHealthState != EHealthStates.Dead)
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //rb.velocity = movementDirection * currentSpeed;
            characterController.Move(currentSpeed * Time.deltaTime * movementDirection);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        Debug.Log("LOOKING");
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

    public void StartMelee(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (canAttack)
            {
                // Attack stuff
                StartCoroutine(Melee());
            }
        }
    }

    IEnumerator Melee()
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
    }

    public void StartThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            chargingThrow = true;
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
            //rb.AddForce(direction * dashSpeed, ForceMode.Force);
            characterController.Move(dashSpeed * Time.deltaTime * direction);
            
            yield return null;
        }
        ToggleInvincibility(false);
        //rb.velocity = Vector3.zero;
        characterController.velocity.Set(0, 0, 0);
    }
    
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            //
            Damaged(100);
            //
            
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
        //pickupPosition.GetComponent<FixedJoint>().connectedBody = null;
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
    }
    
    public void StartRevive()
    {
        currentHealthState = EHealthStates.Reviving;
        ToggleInvincibility(true);

        currentSpeed /= 2;
        StartCoroutine(ReviveCoroutine(Time.time));
    }

    IEnumerator ReviveCoroutine(float startTime)
    {
        while (Time.time < startTime + reviveDuration)
        {
            Debug.Log("Reviving...");
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
        // Creating new player
        GameObject newPlayer = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
        MainPlayerController mainPlayerScript = newPlayer.GetComponent<MainPlayerController>();
        if (mainPlayerScript != null)
        {
            mainPlayerScript.StartRevive();
        }
        
        // Disable this player
        Destroy(characterController);
        Destroy(characterAnimator);
        Destroy(playerInput);
        Destroy(staminaComponent);

        Destroy(mainCamera.gameObject);
        
        // Enable this corpse
        gameObject.GetComponent<CorpseController>().enabled = true;
        
        Destroy(this);
    }
    
    public void Damaged(int damage)
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
}
