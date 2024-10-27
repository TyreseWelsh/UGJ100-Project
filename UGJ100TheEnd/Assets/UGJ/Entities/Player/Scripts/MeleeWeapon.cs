using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    private CapsuleCollider weaponCollider;
    private MainPlayerController playerScript;
    
    [SerializeField] int damage;
    private List<GameObject> damagedEnemies = new List<GameObject>();

    private void Awake()
    {
        weaponCollider = GetComponent<CapsuleCollider>();
        playerScript = GetComponentInParent<MainPlayerController>();
        if (playerScript != null)
        {
            damage = playerScript.meleeDamage;
        }
    }

    public void EnableWeapon()
    {
        weaponCollider.enabled = true;
    }

    public void DisableWeapon()
    {
        weaponCollider.enabled = false;
    }

    public void ClearDamagedEnemies()
    {
        if (damagedEnemies.Count > 0)
        {
            damagedEnemies.Clear();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (!damagedEnemies.Contains(other.gameObject))
            {
                damagedEnemies.Add(other.gameObject);
                IDamageable damageableInterface = other.gameObject.GetComponent<IDamageable>();
                if (damageableInterface != null)
                {
                    damageableInterface.Damaged(damage);
                }
            }
        }
    }
}
