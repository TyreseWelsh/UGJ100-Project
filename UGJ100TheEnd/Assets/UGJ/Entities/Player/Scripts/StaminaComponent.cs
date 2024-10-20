using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StaminaComponent : MonoBehaviour
{
    public enum EStaminaAbilityStrength {Full, Reduced, Zero}

    [Header("Stamina Stats")]
    [SerializeField] private int maxStamina = 120;
    [HideInInspector] public int currentStamina;
    
    [SerializeField] private float regenDelayTime = 2f;
    [SerializeField] private int regenAmount = 2;
    [SerializeField] private float regenRate = 0.1f;
    private Coroutine delayCoroutine;
    private Coroutine regenCoroutine;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = false;

    private void Start()
    {
        currentStamina = maxStamina;
    }

    void StartRegenDelay()
    {
        if (debugEnabled)
        {
            print("Stamina is now = " + currentStamina);
        }

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }
        StartCoroutine(RegenDelay());
    }
    
    IEnumerator RegenDelay()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        yield return new WaitForSeconds(regenDelayTime);
        
        regenCoroutine = StartCoroutine(RegenStamina());
    }
    
    IEnumerator RegenStamina()
    {
        while (currentStamina < maxStamina)
        {
            currentStamina += regenAmount;
            if (debugEnabled)
            {
                print("Stamina = " + currentStamina);
            }
            
            yield return new WaitForSeconds(regenRate);
            yield return null;
        }
        
        currentStamina = maxStamina;
    }
    
    // This function should be called from the gameobject script that handles the stamina ability
    // It will handle reducing the gameobjects stamina based on the given cost and start a delay for its stamina regen
    public EStaminaAbilityStrength ConsumeStamina(int staminaCost)
    {
        // If player has enough stamina, use ability at full strength
        if (currentStamina >= staminaCost)
        {
            currentStamina -= staminaCost;
            StartRegenDelay();
            return EStaminaAbilityStrength.Full;
        }

        // If the stamina cost would reduce player stamina to negaitve half of max stamina, the ability wont be used
        // Allowing the player to go into negative stamina gives them a bit of leeway, however there is a limit to how far negative they can go
        if (currentStamina - staminaCost < -(maxStamina / 2))
        {
            return EStaminaAbilityStrength.Zero;
        }

        // If the above "if" statements dont apply, use ability at reduced strength
        currentStamina -= staminaCost;
        StartRegenDelay();
        return EStaminaAbilityStrength.Reduced;
    }
}
