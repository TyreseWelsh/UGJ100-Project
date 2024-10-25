using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField]
    private GameObject InteractedObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Corpse")
        {
            InteractedObject.GetComponent<IInteractable>().Interact(gameObject);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Corpse")
        {
            InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
     
        }
    }
}
