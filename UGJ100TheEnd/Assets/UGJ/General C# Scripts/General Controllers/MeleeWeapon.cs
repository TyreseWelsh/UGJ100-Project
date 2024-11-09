using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IMeleeWeapon
{
    [SerializeField] private CapsuleCollider weaponCollider;
    
    int damage;
    private List<GameObject> damagedEnemies = new List<GameObject>();

    private void Awake()
    {
        weaponCollider = GetComponent<CapsuleCollider>();
        /*playerScript = GetComponentInParent<MainPlayerController>();
        if (playerScript != null)
        {
            damage = playerScript.meleeDamage;
        }*/
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void EnableCollider()
    {
        print("Enable collider");
        weaponCollider.enabled = true;
    }

    public void DisableCollider()
    {
        print("disable collider");

        weaponCollider.enabled = false;
    }

    public void ClearDamagedObjects()
    {
        if (damagedEnemies.Count > 0)
        {
            damagedEnemies.Clear();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name + " hit!");
        IDamageable damageableInterface = other.gameObject.GetComponent<IDamageable>();
        if (damageableInterface != null)
        {
            if (!damagedEnemies.Contains(other.gameObject))
            {
                damagedEnemies.Add(other.gameObject);
                damageableInterface.Damaged(damage, gameObject);
            }
        }
    }
}
