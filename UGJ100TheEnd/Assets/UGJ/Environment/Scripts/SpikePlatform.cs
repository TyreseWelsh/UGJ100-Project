using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePlatform : MonoBehaviour
{
    private BoxCollider collider;
    [SerializeField] private int damage = 25;
    
    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != null)
        {
            IDamageable damageableInterface = other.gameObject.GetComponent<IDamageable>();
            if (damageableInterface != null)
            {
                damageableInterface.Damaged(damage);
            }
        }
    }
}
