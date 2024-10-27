using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject doorMesh;
    [SerializeField]private float rotationSpeed;
    private bool isOpen = false;
    private Vector3 StartRotation;
    private bool isRotating = false;
    
    [SerializeField] Vector3 closedRotation = new Vector3(0, 0, 0);
    [SerializeField] Vector3 openRotation = new Vector3(0, -90, 0);
    Coroutine openCoroutine;
    Coroutine closeCoroutine;

    public void Interact(GameObject interactingObj)
    {
        //gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y + 90, gameObject.transform.rotation.z);
        //if (!isRotating)
        //{
            if (isOpen)
            {
                if (openCoroutine != null)
                {
                    StopCoroutine(openCoroutine);
                }
                closeCoroutine = StartCoroutine(DoorClose());
            }
            else
            {
                if (closeCoroutine != null)
                {
                    StopCoroutine(closeCoroutine);
                }
                openCoroutine = StartCoroutine(DoorOpen());
            }
        //} 
    }

    public void InteractHeld(GameObject interactingObj)
    {
    }
    
    private IEnumerator DoorOpen()
    {
        Debug.Log("Open door");

        Quaternion startRotation = transform.rotation;
        float time = 0;
        isOpen = true;
        
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(openRotation), time);
            yield return null;
            time += Time.deltaTime * rotationSpeed;
            
        }
        
        isRotating = false;

    }
    private IEnumerator DoorClose()
    {
        Debug.Log("Close door");
        Quaternion startRotation = transform.rotation;
        float time = 0;
        isOpen = false;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(closedRotation), time);
            yield return null;
            time += Time.deltaTime * rotationSpeed;
            
        }
        
        //isRotating = false;
    }
}
