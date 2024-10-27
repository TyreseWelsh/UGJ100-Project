using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField]
    private GameObject InteractedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Corpse"))
        {
            Debug.Log(other.gameObject.name + " entered...");
            InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Corpse"))
        {
            Debug.Log(other.gameObject.name + " exited...");
            InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
        }
    }
}
