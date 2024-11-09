using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    private Animator ownerAnimator;
    
    protected CharacterDataTemplate characterData;
    [SerializeField] protected GameObject weaponPosition;
    private GameObject characterWeapon;
    private List<GameObject> damagedTargets = new List<GameObject>();
    
    private ComboDataTemplate comboData;
    private int comboAttackCounter;
    [HideInInspector] public bool canAttack = true;

    private Coroutine nextAttackCoroutine;
    private Coroutine resetComboCoroutine;
    
    public void Start()
    {
        ownerAnimator = GetComponent<Animator>();
    }
    
    public void InitData(CharacterDataTemplate data)
    {
        characterData = data;
        comboData = characterData.basicComboData;
        comboData.InitAttackData();
        SpawnWeapon();
    }

    private void SpawnWeapon()
    {
        characterWeapon = Instantiate(characterData.weapon, weaponPosition.transform.position, Quaternion.identity);
        Vector3 weaponRotation = characterWeapon.transform.rotation.eulerAngles;
        characterWeapon.transform.parent = weaponPosition.transform;
        characterWeapon.transform.localRotation = Quaternion.Euler(weaponRotation);
        characterWeapon?.GetComponentInChildren<MeleeWeapon>()?.SetDamage(comboData.comboAttacks[comboAttackCounter].damage);
    }
    
    public void EnableWeaponCollider()
    {
        characterWeapon?.GetComponentInChildren<IMeleeWeapon>()?.EnableCollider();
    }

    public void DisableWeaponCollider()
    {
        characterWeapon?.GetComponentInChildren<IMeleeWeapon>()?.DisableCollider();
    }

    public void EndAttack()
    {
        characterWeapon?.GetComponentInChildren<IMeleeWeapon>()?.ClearDamagedObjects();
    }
    
    public void StartAttack()
    {
        if (canAttack)
        {
            if (comboAttackCounter < comboData.comboAttacks.Count)
            {
                StopResetComboTimer();

                canAttack = false;
                ownerAnimator.SetFloat("AnimationSpeed", comboData.comboAttacks[comboAttackCounter].animationSpeed);
                ownerAnimator.Play(comboData.comboAttacks[comboAttackCounter].animationName);
            }
        }
    }

    public void IncreaseCombo()
    {
        comboAttackCounter++;

        StopResetComboTimer();
        resetComboCoroutine = StartCoroutine(ResetComboTimer());
        
        // If not at the end of combo
        if (comboAttackCounter < comboData.comboAttacks.Count)
        {
            canAttack = true;
        }
    }
    
    private IEnumerator ResetComboTimer()
    {
        float startTime = Time.time;
        
        while (Time.time < startTime + comboData.comboResetDelay)
        {
            yield return null;
        }
        
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

    public void SetCanAttack(bool newCanAttack)
    {
        canAttack = newCanAttack;
    }
    
    public void SetCombo(ComboDataTemplate newCombo)
    {
        comboData = newCombo;
    }

    public GameObject GetCurrentWeapon()
    {
        return characterWeapon;
    }
}
