using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBridge : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject bridgeMesh;
    [SerializeField] private float speed;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float targetRotationZ = 90;
    private int activationCounter;
    
    private Coroutine bridgeDownCoroutine;
    private Coroutine bridgeUpCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        startRotation = Quaternion.Euler(bridgeMesh.transform.rotation.eulerAngles);
        endRotation = Quaternion.Euler(new Vector3(startRotation.eulerAngles.x, startRotation.eulerAngles.y, startRotation.eulerAngles.z + Mathf.Ceil(targetRotationZ)));
    }
    
    public void Interact(GameObject interactingObj)
    {
        //Debug.Log("Interacted");
        //gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y + 90, gameObject.transform.rotation.z);
        /*if (!isRotating)
        {
            if (isOpen)
            {
                StartCoroutine(doorClose());
            }
            else
            {
                StartCoroutine(doorOpen());
            }
        }*/
    }
    public void InteractHeld(GameObject interactingObj) { }

    public void BringDown()
    {
        activationCounter++;
        if (bridgeUpCoroutine != null)
        {
            StopCoroutine(bridgeUpCoroutine);
            bridgeUpCoroutine = null;
        }
        bridgeDownCoroutine = StartCoroutine(BridgeDown());
    }

    public void BringUp()
    {
        activationCounter--;
        if (activationCounter <= 0)
        {
            if (bridgeDownCoroutine != null)
            {
                StopCoroutine(bridgeDownCoroutine);
                bridgeDownCoroutine = null;
            }
            bridgeUpCoroutine = StartCoroutine(BridgeUp());
        }
    }
    
    private IEnumerator BridgeDown()
    {
        Quaternion currentStartRotation = bridgeMesh.transform.rotation;
        float time = currentStartRotation.eulerAngles.z / targetRotationZ;

        while (bridgeMesh.transform.rotation != endRotation)
        {
            Debug.Log("Bridge going down...");
            
            //transform.rotation = Quaternion.Slerp(currentStartRotation, endRotation, time);
            bridgeMesh.transform.rotation = Quaternion.RotateTowards(currentStartRotation, endRotation, time);
            time += Time.deltaTime * speed;
            yield return null;
        }
        Debug.Log("Bridge now down!");
    }
    private IEnumerator BridgeUp()
    {
        Quaternion currentStartRotation = bridgeMesh.transform.rotation;
        float time = currentStartRotation.eulerAngles.z / targetRotationZ;
        
        while (bridgeMesh.transform.rotation != startRotation)
        {
            Debug.Log("Bridge going up...");

            //transform.rotation = Quaternion.Slerp(currentStartRotation, startRotation, time);
            bridgeMesh.transform.rotation = Quaternion.RotateTowards(currentStartRotation, startRotation, time);
            time += Time.deltaTime * speed;
            yield return null;
        }
        Debug.Log("Bridge now up!");
    }
}
