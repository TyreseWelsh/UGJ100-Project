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
    
    private CharacterController controller;
    private StaminaComponent staminaComponent;
    
    [Header("Basic Stats")]
    [SerializeField] private int maxHealth;
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
                        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
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
        
        gameObject.layer = LayerMask.NameToLayer("Player");
        controller.velocity.Set(0, 0, 0);
    }

    // This function should work with any stamina ability. Simply call this function in a switch statement and have the switch cases be the enum options

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentHealthState == EHealthStates.Alive)
            {
                // temp: testing revive
                Damaged(2);
                //
            
                RaycastHit hit;
                Physics.Raycast(transform.position, mesh.transform.forward * 2, out hit);
                Debug.DrawRay(transform.position, mesh.transform.forward * 2, Color.red, 0.5f);

                // if hit object is not null
                if (hit.collider)
                {
                    hit.collider.gameObject.GetComponent<IInteractible>().Interact(gameObject);
                }
            }
        }
    }

    void StartRevive()
    {
        currentHealthState = EHealthStates.Reviving;
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
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
        gameObject.layer = LayerMask.NameToLayer("Player");
        
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
