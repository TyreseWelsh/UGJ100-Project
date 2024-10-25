using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    private CapsuleCollider collider;
    private MainPlayerController playerScript;
    
    [SerializeField] int damage;
    private GameObject[] damagedEnemies;

    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        playerScript = GetComponentInParent<MainPlayerController>();
        if (playerScript != null)
        {
            damage = playerScript.meleeDamage;
        }
    }

    public void EnableWeapon()
    {
        collider.enabled = true;
    }

    public void DisableWeapon()
    {
        collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (!damagedEnemies.Contains(other.gameObject))
            {
                damagedEnemies.Append(other.gameObject);
                IDamageable damageableInterface = other.gameObject.GetComponent<IDamageable>();
                if (damageableInterface != null)
                {
                    damageableInterface.Damaged(damage);
                }
            }
        }
    }
}
