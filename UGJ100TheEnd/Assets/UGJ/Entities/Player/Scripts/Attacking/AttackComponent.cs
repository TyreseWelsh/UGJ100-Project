using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    private Animator ownerAnimator;
    
    [Serializable]
    public class Attack
    {
        public AttackDataTemplate baseData; 

        [HideInInspector] public string animationName;
        [HideInInspector] public int damage;
        [HideInInspector] public float animationSpeed;
    }

    [Serializable]
    public class Combo
    {
        public List<Attack> comboAttacks;
        public float comboResetDelay;
    }

    public Combo currentCombo;
    private int comboAttackCounter;
    private bool canAttack = true;

    private Coroutine nextAttackCoroutine;
    private Coroutine resetComboCoroutine;
    
    private void Awake()
    {
        ownerAnimator = GetComponent<Animator>();
        InitAttackData();
    }

    private void InitAttackData()
    {
        foreach(Attack attack in currentCombo.comboAttacks)
        {
            attack.animationName = attack.baseData.animationName;
            attack.damage = attack.baseData.damage;
            attack.animationSpeed = attack.baseData.animationSpeed;
        }
    }

    public void StartAttack()
    {
        if (canAttack)
        {
            if (comboAttackCounter < currentCombo.comboAttacks.Count)
            {
                StopResetComboTimer();

                canAttack = false;
                ownerAnimator.SetFloat("AnimationSpeed", currentCombo.comboAttacks[comboAttackCounter].animationSpeed);
                ownerAnimator.Play (currentCombo.comboAttacks[comboAttackCounter].animationName);
            }
        }
    }

    public void IncreaseCombo()
    {
        comboAttackCounter++;

        StopResetComboTimer();
        resetComboCoroutine = StartCoroutine(ResetComboTimer());
        
        // If not at the end of combo
        if (comboAttackCounter < currentCombo.comboAttacks.Count)
        {
            canAttack = true;
        }
    }
    
    /*public void StartNextAttackDelay()
    {
        if (nextAttackCoroutine != null)
        {
            StopCoroutine(nextAttackCoroutine);
        }
        nextAttackCoroutine = StartCoroutine(NextAttackTimer());
    }*/

    /*private IEnumerator NextAttackTimer()
    {
        float startTime = Time.time;

        while (Time.time < startTime + basicAttacks[comboAttackCounter].nextAttackDelay)
        {
            yield return null;
        }

        comboAttackCounter++;
        canAttack = true;
        
        StopCoroutine(nextAttackCoroutine);
        nextAttackCoroutine = null;
    }*/
    
    // Entire combo reset
    /*public void StartComboResetTimer()
    {
        if (resetComboCoroutine != null)
        {
            StopCoroutine(resetComboCoroutine);
        }
        resetComboCoroutine = StartCoroutine(ResetComboTimer());
    }*/
    
    private IEnumerator ResetComboTimer()
    {
        float startTime = Time.time;

        while (Time.time < startTime + currentCombo.comboResetDelay)
        {
            yield return null;
        }
        
        //print("Attack component attached");

        ResetCombo();
        StopResetComboTimer();
    }

    private void ResetCombo()
    {
        comboAttackCounter = 0;
        canAttack = true;
    }

    private void StopResetComboTimer()
    {
        if (resetComboCoroutine != null)
        {
            StopCoroutine(resetComboCoroutine);
            resetComboCoroutine = null;
        }
    }
}
