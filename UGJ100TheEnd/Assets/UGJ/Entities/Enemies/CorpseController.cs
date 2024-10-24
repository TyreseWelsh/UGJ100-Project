using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
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
    [SerializeField] float jointBreakForce = 21000;
    [Header("Environment Collider")]
    private SphereCollider environmentCollider;
    [SerializeField] private float environmentColliderHeight;
    [SerializeField] private float environmentColliderRadius;

    private GameObject holdingObject;
    private Rigidbody[] childrenRigidbodies;

    private void Awake()
    {
        childrenRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //pickupJoint = GetComponent<ConfigurableJoint>();
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
    }

    void InitPickupJoint()
    {
        Destroy(gameObject.GetComponent<MainPlayerController>().pickupPosition);
        
        pickupJoint = gameObject.AddComponent<ConfigurableJoint>();
        pickupJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        pickupJoint.projectionDistance = 0.01f;
        pickupJoint.breakForce = jointBreakForce;
        pickupJoint.enablePreprocessing = false;
        
        InitCorpseRigidbody();
    }
    
    void InitCorpseRigidbody()
    {
        corpseRb = GetComponent<Rigidbody>();
        corpseRb.mass = 10;
        corpseRb.drag = 1;
        corpseRb.angularDrag = 5;
        corpseRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        corpseRb.includeLayers = LayerMask.GetMask("Default");
        corpseRb.includeLayers = LayerMask.GetMask("Bullet");
        corpseRb.includeLayers = LayerMask.GetMask("Enemy");
        
        meshMainJoint.connectedBody = corpseRb;
    }

    private void OnDisable()
    {
        DisableCorpseRagdoll();
    }

    void EnableCorpseRagdoll()
    {
        Debug.Log("EnableRagdoll");
        foreach (Rigidbody rb in childrenRigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    void DisableCorpseRagdoll()
    {
        Debug.Log("DisableRagdoll");
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
        Debug.Log("Picked up");
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

    public void Damaged(int damage)
    {
        bodyDurability -= damage;

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
            pickupJoint.breakForce = 21000;
            pickupJoint.enablePreprocessing = false;
        }
    }
}
