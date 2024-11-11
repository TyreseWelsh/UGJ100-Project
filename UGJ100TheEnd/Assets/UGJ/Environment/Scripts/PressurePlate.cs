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
            /*if (!other.gameObject.GetComponent<CorpseController>()?.holdingObject)
            {*/
                DrawBridge drawBridgeScript = InteractedObject.GetComponent<DrawBridge>();
                if (drawBridgeScript != null)
                {
                    print("Draw bridge");
                    drawBridgeScript.BringDown();
                }
                else
                {
                    InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
                }
            //}
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Corpse"))
        {
            /*if (!other.gameObject.GetComponent<CorpseController>()?.holdingObject)
            {*/
                DrawBridge drawBridgeScript = InteractedObject.GetComponent<DrawBridge>();
                if (drawBridgeScript != null)
                {
                    drawBridgeScript.BringUp();
                }
                else
                {
                    InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
                }
            //}
        }
    }
}
