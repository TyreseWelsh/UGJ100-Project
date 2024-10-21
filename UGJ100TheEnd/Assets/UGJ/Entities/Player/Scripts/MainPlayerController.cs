using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerController : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject pickupPosition;
    [SerializeField] private LayerMask interactableObjectLayer;
    [SerializeField] private GameObject meleeAttackPoint;

    [Header("")] 
    private Rigidbody rb;
    private StaminaComponent staminaComponent;
    
    [Header("Basic Stats")]
    [SerializeField] public int maxHealth;
    [SerializeField] private float maxSpeed = 9;
    [SerializeField] private int lives;
    public enum EHealthStates {Alive, Reviving, Dead}
    public EHealthStates currentHealthState = EHealthStates.Alive;
    
    [Header("Dash Stats")]
    [SerializeField] private float dashSpeed = 100;
    [SerializeField] private float dashDuration = 0.05f;
    [SerializeField] private int dashCost = 40;
    
    [Header("")]
    [SerializeField] private float reviveDuration;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public float currentSpeed;

    private Vector3 movementDirection;
    private bool holdingCorpse;
    private GameObject heldCorpse;

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
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        staminaComponent = GetComponent<StaminaComponent>();
        
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
            rb.velocity = movementDirection * currentSpeed;
        }
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
            rb.AddForce(direction * dashSpeed, ForceMode.Force);
            
            yield return null;
        }
        ToggleInvincibility(false);
        rb.velocity = Vector3.zero;
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
                                heldCorpse = hit.collider.gameObject;
                                heldCorpse.transform.position = pickupPosition.transform.position;
                                heldCorpse.transform.SetParent(pickupPosition.transform);
                                pickupPosition.GetComponent<FixedJoint>().connectedBody = heldCorpse.GetComponent<Rigidbody>();
                                holdingCorpse = true;
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

    void DropCorpse()
    {
        pickupPosition.GetComponent<FixedJoint>().connectedBody = null;
        heldCorpse.transform.SetParent(null);
        heldCorpse = null;
        holdingCorpse = false;
    }
    
    void StartRevive()
    {
        currentHealthState = EHealthStates.Reviving;
        ToggleInvincibility(false);

        currentSpeed /= 2;
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
        currentHealth = maxHealth;
        currentSpeed = maxSpeed;
        ToggleInvincibility(true);
        
        currentHealthState = EHealthStates.Alive;
    }

    void ToggleInvincibility(bool isInvincible)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), isInvincible);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bullet"), isInvincible);
    }
    
    public void Damaged(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            lives--;

            if (lives > 0)
            {
                StartRevive();
            }
            else
            {
                Debug.Log("NO PLAYER LIVES REMAINING - Restart game!");
                currentHealthState = EHealthStates.Dead;
            }
        }
    }
}
