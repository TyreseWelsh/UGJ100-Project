using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboData", menuName = "Combo Data Template")]
//[Serializable]
public class ComboDataTemplate : ScriptableObject
{
    [Serializable]
    public class Attack
    {
        public AttackDataTemplate baseData; 

        [HideInInspector] public string animationName;
        [HideInInspector] public int damage;
        [HideInInspector] public float animationSpeed;
    }
    
    public List<Attack> comboAttacks;
    [Tooltip("Delay until owner can use combo again. Acts as attack speed for single attack combos.")]
    public float comboResetDelay;
    
    public void InitAttackData()
    {
        foreach(Attack attack in comboAttacks)
        {
            attack.animationName = attack.baseData.animationName;
            attack.damage = attack.baseData.damage;
            attack.animationSpeed = attack.baseData.animationSpeed;
        }
    }
}
