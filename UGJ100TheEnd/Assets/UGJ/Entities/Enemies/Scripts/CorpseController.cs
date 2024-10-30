using System;
using System.Collections;
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
    private CapsuleCollider corpseProjectileCollider;

    [SerializeField] private int bodyDurability = 100;
    [SerializeField] float jointBreakForce = 150000;
    
    [Header("Environment Collider")]
    private SphereCollider environmentCollider;
    [SerializeField] private float environmentColliderHeight;
    [SerializeField] private float environmentColliderRadius;

    [Header("Damaged")]
    [SerializeField] private Material damageFlashMaterial;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    private SkinnedMeshRenderer[] limbMeshes;

    
    [SerializeField] private float minDamageVelocity;
    
    private GameObject holdingObject;
    private Rigidbody[] childrenRigidbodies;


    private void Awake()
    {
        childrenRigidbodies = GetComponentsInChildren<Rigidbody>();
        limbMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        //Debug.Log("Corpse velocity = " + corpseRb.velocity.magnitude);
        if (corpseRb.velocity.magnitude > minDamageVelocity)
        {
            
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
        environmentCollider.includeLayers = LayerMask.GetMask("Enemy");
        environmentCollider.excludeLayers = LayerMask.GetMask("Player");
        
        InitPickupJoint();
        InitCorpseRigidbody();
        InitProjectileCollider();
    }

    void InitPickupJoint()
    {
        Debug.Log("Initpickup");
        
        Debug.Log("IF NOT PROBLEM");
        pickupJoint = gameObject.AddComponent<ConfigurableJoint>();
        pickupJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        pickupJoint.projectionDistance = 0.01f;
        pickupJoint.breakForce = jointBreakForce;
        pickupJoint.enablePreprocessing = false;
    }
    
    void InitCorpseRigidbody()
    {
        corpseRb = GetComponent<Rigidbody>();
        corpseRb.mass = 10;
        corpseRb.drag = 1;
        corpseRb.angularDrag = 5;
        corpseRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        corpseRb.freezeRotation = false;
        corpseRb.includeLayers = LayerMask.GetMask("Default", "Bullet", "Enemy");
        corpseRb.excludeLayers = LayerMask.GetMask("Player", "Corpse", "Limb");
        
        meshMainJoint.connectedBody = corpseRb;
    }

    void InitProjectileCollider()
    {
        /*corpseProjectileCollider = gameObject.AddComponent<CapsuleCollider>();
        corpseProjectileCollider.isTrigger = true;
        corpseProjectileCollider.radius = 0.5f;
        corpseProjectileCollider.height = 1.4f;
        corpseProjectileCollider.excludeLayers = LayerMask.GetMask("Player", "Corpse", "Limb");
        
        corpseProjectileCollider.enabled = false;*/
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
        Debug.Log("Corpse not eaten");
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
        Debug.Log("Break force = " + breakForce);

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
                Debug.Log("Joint Broken");
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
        Debug.Log(other.gameObject.name);
        if (corpseRb.velocity.magnitude > 25f)
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damaged(Mathf.FloorToInt(corpseRb.velocity.magnitude), holdingObject);
            }  
        }
    }
}
