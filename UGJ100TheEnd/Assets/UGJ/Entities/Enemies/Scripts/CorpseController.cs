using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class CorpseController : MonoBehaviour, IInteractable, IDamageable
{
    public enum CorpseType
    {
        Ranged,
        Melee,
        Player
    };
    public CorpseType ECorpse;

    private Rigidbody corpseRb;
    private ConfigurableJoint pickupJoint;
    [SerializeField] private FixedJoint meshMainJoint;

    [SerializeField] private int bodyDurability = 100;
    [SerializeField] float jointBreakForce = 150000;
    
    [Header("Environment Collider")]
    private SphereCollider environmentCollider;
    [SerializeField] private float environmentColliderHeight;
    [SerializeField] private float environmentColliderRadius;

    [Header("Corpse Projectile")]
    private float minDamageVelocity = 15f;
    private float currentVelocity = 0;
    private List<GameObject> hitObjects = new List<GameObject>(); 
    
    [Header("Damaged")]
    [SerializeField] private Material damageFlashMaterial;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    private SkinnedMeshRenderer[] limbMeshes;
    
    private GameObject holdingObject;
    private Rigidbody[] childrenRigidbodies;


    private void Awake()
    {
        corpseRb = GetComponent<Rigidbody>();

        childrenRigidbodies = GetComponentsInChildren<Rigidbody>();
        limbMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        if (corpseRb != null)
        {
            currentVelocity = corpseRb.velocity.magnitude;
        }
    }

    private void OnEnable()
    {
        gameObject.tag = "Corpse";
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        EnableCorpseRagdoll();
        
        environmentCollider = gameObject.AddComponent<SphereCollider>();
        environmentCollider.center = new Vector3(0, environmentColliderHeight, 0);
        environmentCollider.radius = environmentColliderRadius;
        
        InitPickupJoint();
        InitCorpseRigidbody();
        
        if (corpseRb != null)
        {
            Debug.Log("Corpse RB set successfully");
        }
        else
        {
            print("Failure to set corpse rb");
        }
    }

    void InitPickupJoint()
    {
        pickupJoint = gameObject.AddComponent<ConfigurableJoint>();
        pickupJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        pickupJoint.projectionDistance = 0.01f;
        pickupJoint.breakForce = jointBreakForce;
        pickupJoint.enablePreprocessing = false;
    }
    
    void InitCorpseRigidbody()
    {
        corpseRb.mass = 10;
        corpseRb.drag = 1;
        corpseRb.angularDrag = 5;
        corpseRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        corpseRb.freezeRotation = false;
        
        meshMainJoint.connectedBody = corpseRb;
    }
    
    private void OnDisable()
    {
        DisableCorpseRagdoll();
    }

    void EnableCorpseRagdoll()
    {
        foreach (Rigidbody rb in childrenRigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    void DisableCorpseRagdoll()
    {
        foreach (Rigidbody rb in childrenRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public void Interact(GameObject interactingObj) 
    {
    }
    public void InteractHeld(GameObject interactingObj) 
    {
        if(interactingObj.CompareTag("Player"))
        {
            MainPlayerController playerScript = interactingObj.GetComponent<MainPlayerController>();
           
            if(playerScript.currentHealth + 10 > playerScript.maxHealth)
            {
                playerScript.currentHealth = playerScript.maxHealth;
            }
            else
            {
                playerScript.currentHealth += 10;
            }
            Destroy(gameObject);
        }
    }

    // MainObject = The object which we will need a reference to, to remove their reference to this corpse
    // PickingObject = The object which we will connect the corpses joint to (in the players case it is the PickupLocation)
    public void Pickup(GameObject mainObject, GameObject pickingObject)
    {
        holdingObject = mainObject;
        
        gameObject.transform.position = pickingObject.transform.position;
        
        pickupJoint.connectedBody = pickingObject.GetComponent<Rigidbody>();
        pickupJoint.xMotion = ConfigurableJointMotion.Locked;
        pickupJoint.yMotion = ConfigurableJointMotion.Locked;
        pickupJoint.zMotion = ConfigurableJointMotion.Locked;
        
        hitObjects.Clear();
    }
    
    public void Drop()
    {
        if (pickupJoint.connectedBody != null)
        {
            pickupJoint.connectedBody = null;
            pickupJoint.xMotion = ConfigurableJointMotion.Free;
            pickupJoint.yMotion = ConfigurableJointMotion.Free;
            pickupJoint.zMotion = ConfigurableJointMotion.Free;
        }
    }

    public IEnumerator DamageFlash(SkinnedMeshRenderer meshRender, Material startingMaterial, Material flashMaterial, float flashTime)
    {
        meshRender.material = flashMaterial;
        yield return new WaitForSeconds(flashTime);
        
        meshRender.material = originalMaterial;
    }
    
    public void Damaged(int damage, GameObject attacker)
    {
        bodyDurability -= damage;
        foreach (SkinnedMeshRenderer meshRenderer in limbMeshes)
        {
            StartCoroutine(DamageFlash(meshRenderer, originalMaterial, damageFlashMaterial, damageFlashDuration));
        }

        if (bodyDurability <= 0)
        {
            // Body destroyed
            if (holdingObject != null)
            {
                MainPlayerController playerScript = holdingObject.GetComponent<MainPlayerController>();
                if (playerScript != null)
                {
                    playerScript.DropCorpse();
                } 
            }

            Destroy(gameObject);
        }
    }

    void OnJointBreak(float breakForce)
    {
        // Reduce limb velocities to prevent body flying off
        foreach (Rigidbody rb in childrenRigidbodies)
        {
            if (rb != null)
            {
                /*Debug.Log("BEFORE: Limb vel = " + rb.velocity);
                rb.velocity = Vector3.Scale(rb.velocity, new Vector3(0.001f, 0.001f, 0.001f));
                Debug.Log("AFTER: Limb vel = " + rb.velocity);*/
                rb.velocity = Vector3.zero;
            }
        }
        
        if (holdingObject != null)
        {
            if (holdingObject.GetComponent<ICanHoldCorpse>() != null)
            {
                holdingObject.GetComponent<ICanHoldCorpse>().DropCorpse();
            }
        }

        pickupJoint = gameObject.AddComponent<ConfigurableJoint>();
        if (pickupJoint != null)
        {
            pickupJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            pickupJoint.projectionDistance = 0.01f;
            pickupJoint.breakForce = jointBreakForce;
            pickupJoint.enablePreprocessing = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Corpse") && corpseRb != null)
        {
            if (Mathf.FloorToInt(currentVelocity) >= minDamageVelocity)
            {
                if (!hitObjects.Contains(other.gameObject))
                {
                    IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        hitObjects.Add(other.gameObject);
                        damageable.Damaged(Mathf.FloorToInt(currentVelocity * 2), holdingObject);
                    }  
                }
            }
        }
    }
}
