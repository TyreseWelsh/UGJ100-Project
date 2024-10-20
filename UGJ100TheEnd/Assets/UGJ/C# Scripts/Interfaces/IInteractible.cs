using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractible
{
    public void Interact(GameObject interactingObj);
    public void InteractHeld(GameObject interactingObj);
}