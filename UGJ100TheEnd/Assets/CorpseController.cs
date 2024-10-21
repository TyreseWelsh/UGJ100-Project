using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CorpseController : MonoBehaviour, IInteractable
{
    public enum CorpseType
    {
        Ranged,
        Melee,
        Player
    };
    public CorpseType ECorpse;

    private GameObject holdingObject;
    private ConfigurableJoint pickupJoint;
    
    // Start is called before the first frame update
    void Start()
    {
        pickupJoint = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(GameObject interactingObj) 
    {
        Debug.Log("Corpse not eaten");
    }
    public void InteractHeld(GameObject interactingObj) 
    {
        if(interactingObj.tag == "Player")
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
        transform.SetParent(pickingObject.transform);
        
        pickupJoint.connectedBody = pickingObject.GetComponent<Rigidbody>();
        pickupJoint.xMotion = ConfigurableJointMotion.Locked;
        pickupJoint.yMotion = ConfigurableJointMotion.Locked;
        pickupJoint.zMotion = ConfigurableJointMotion.Locked;
    }
    
    public void Drop()
    {
        transform.SetParent(null);
        pickupJoint.connectedBody = null;
        pickupJoint.xMotion = ConfigurableJointMotion.Free;
        pickupJoint.yMotion = ConfigurableJointMotion.Free;
        pickupJoint.zMotion = ConfigurableJointMotion.Free;
    }

    void OnJointBreak(float breakForce)
    {
            if (holdingObject != null)
            {
                if (holdingObject.GetComponent<ICanHoldCorpse>() != null)
                {
                    Debug.Log("Joint Broken");
                    holdingObject.GetComponent<ICanHoldCorpse>().DropCorpse();
                }
            }

            pickupJoint = gameObject.AddComponent<ConfigurableJoint>();
            pickupJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            pickupJoint.projectionDistance = 0.01f;
            pickupJoint.breakForce = 21000;
            pickupJoint.enablePreprocessing = false;
    }
}
