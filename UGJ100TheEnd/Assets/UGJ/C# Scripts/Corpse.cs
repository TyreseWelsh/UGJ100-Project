using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour, IInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(GameObject interactingObj)
    {
        //Increase player health here.
        Destroy(gameObject);
    }
    public void InteractHeld(GameObject interactingObj) { }
}
