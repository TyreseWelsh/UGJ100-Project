using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerController : MonoBehaviour, IDamagable
{
    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mesh;
    [SerializeField] private Transform corpseLoc;
    [SerializeField] private LayerMask interactibleObj;
    
    private CharacterController controller;
    private StaminaComponent staminaComponent;
    
    [Header("Basic Stats")]
    [SerializeField] public int maxHealth;
    [SerializeField] private float maxSpeed;
    [SerializeField] private int lives;

    [Header("Dash Stats")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private int dashCost;
    
    [Header("")]
    [SerializeField] private float reviveDuration;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public float currentSpeed;

    private Vector3 movementDirection;
    private bool holdingCorpse = false;
    private GameObject heldCorpse;

    public enum EHealthStates {Alive, Reviving, Dead}
    [Header("")]
    public EHealthStates currentHealthState = EHealthStates.Alive;
    
    
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        staminaComponent = GetComponent<StaminaComponent>();
        
        currentHealth = maxHealth;
        currentSpeed = maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealthState != EHealthStates.Dead)
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(currentSpeed * Time.deltaTime * movementDirection);
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
            controller.Move(dashSpeed * Time.deltaTime * direction);
            
            yield return null;
        }

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        controller.velocity.Set(0, 0, 0);
    }

    // This function should work with any stamina ability. Simply call this function in a switch statement and have the switch cases be the enum options

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Debug.Log("Pressed");
            if (currentHealthState == EHealthStates.Alive)
            {
                RaycastHit hit;
                // temp: testing revive

                //

                if (holdingCorpse)
                {
                    heldCorpse.transform.SetParent(null);
                    holdingCorpse = false;
                    return;
                }
                
                Physics.Raycast(transform.position, mesh.transform.forward * 2, out hit, interactibleObj);
                Debug.DrawRay(transform.position, mesh.transform.forward * 2, Color.red, 0.5f);

                // if hit object is not null
                if (hit.collider)
                {   
                    if(hit.collider.gameObject != null)
                    {
                        if(hit.collider.gameObject.GetComponent<IInteractible>() != null)
                        {
                            hit.collider.gameObject.GetComponent<IInteractible>().Interact(gameObject);
                            if (hit.collider.gameObject.tag == "Corpse")
                            {
                                heldCorpse = hit.collider.gameObject;
                                heldCorpse.transform.SetParent(corpseLoc);
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
                hit.collider.gameObject.GetComponent<IInteractible>().InteractHeld(gameObject);

            }
        }
        
    }
    

    void StartRevive()
    {
        currentHealthState = EHealthStates.Reviving;
        currentSpeed /= 2;
        StartCoroutine(ReviveCoroutine(Time.time));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
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
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        currentHealthState = EHealthStates.Alive;
        //Debug.Log("REVIVED");
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
