using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StaminaComponent : MonoBehaviour
{
    public enum EStaminaAbilityStrength {Full, Reduced, Zero}

    [Header("Stamina Stats")]
    [SerializeField] public int maxStamina = 120;
    [HideInInspector] public float currentStamina;
    [SerializeField] public int negStaminaLimit = -60;
    
    [SerializeField] private float regenDelayTime = 3f;
    [SerializeField] private float regenAmount = 0.5f;
    [SerializeField] private float regenRate = 0.05f;
    private Coroutine delayCoroutine;
    private Coroutine regenCoroutine;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = false;

    private void Start()
    {
        currentStamina = maxStamina;
        negStaminaLimit = -(maxStamina / 2);
    }

    public void StartRegenDelay(float delay)
    {
        if (debugEnabled)
        {
            print("Stamina is now = " + currentStamina);
        }
        
        // Stopping previous running coroutines
        StopRegen();

        if (delayCoroutine == null)
        {
            delayCoroutine = StartCoroutine(RegenDelay(delay));
        }
    }
    
    IEnumerator RegenDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenStamina());
        }
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
        StopCoroutine(regenCoroutine);
        regenCoroutine = null;
    }

    void StopRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }
    
    // This function should be called from the gameobject script that handles the stamina ability
    // It will handle reducing the gameobjects stamina based on the given cost and start a delay for its stamina regen
    public EStaminaAbilityStrength ConsumeStamina(int staminaCost)
    {
        // If player has enough stamina, use ability at full strength
        if (currentStamina >= staminaCost)
        {
            currentStamina -= staminaCost;
            StartRegenDelay(regenDelayTime);
            return EStaminaAbilityStrength.Full;
        }

        // If the stamina cost would reduce player stamina to negaitve half of max stamina, the ability wont be used
        // Allowing the player to go into negative stamina gives them a bit of leeway, however there is a limit to how far negative they can go
        if (currentStamina - staminaCost < negStaminaLimit)
        {
            return EStaminaAbilityStrength.Zero;
        }

        // If the above "if" statements dont apply, use ability at reduced strength
        currentStamina -= staminaCost;
        StartRegenDelay(regenDelayTime);
        return EStaminaAbilityStrength.Reduced;
    }
}
