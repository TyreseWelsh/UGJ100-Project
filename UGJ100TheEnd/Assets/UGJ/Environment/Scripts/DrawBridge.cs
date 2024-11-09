using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBridge : MonoBehaviour, IInteractable
{
    [SerializeField] private float speed;
    private bool isOpen = false;
    private Vector3 StartRotation;
    private bool isRotating = false;

    private int activationCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact(GameObject interactingObj)
    {
        Debug.Log("Interacted");
        //gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y + 90, gameObject.transform.rotation.z);
        if (!isRotating)
        {
            if (isOpen)
            {
                StartCoroutine(doorClose());
            }
            else
            {
                StartCoroutine(doorOpen());
            }
        }
    }
    public void InteractHeld(GameObject interactingObj) { }

    public void BringDown()
    {
        activationCounter++;
        StartCoroutine(doorOpen());
    }

    public void BringUp()
    {
        activationCounter--;

        if (activationCounter <= 0)
        {
            StartCoroutine(doorClose());
        }
    }
    
    private IEnumerator doorOpen()
    {
        Quaternion startRotation = gameObject.transform.rotation;
        Quaternion endRotation;
        isRotating = true;
        Debug.Log("Door opening");
        endRotation = Quaternion.Euler(new Vector3(Mathf.Ceil(startRotation.x + 90), StartRotation.y, StartRotation.z));

        float time = 0;

        isOpen = true;
        while (time <= 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;

        }
        Debug.Log("Finished Rotating");
        isRotating = false;

    }
    private IEnumerator doorClose()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(StartRotation);
        isRotating = true;
        Debug.Log("Door closing");
        float time = 0;

        isOpen = false;

        while (time <= 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;

        }
        Debug.Log("Finished Rotating");
        isRotating = false;
    }
}
