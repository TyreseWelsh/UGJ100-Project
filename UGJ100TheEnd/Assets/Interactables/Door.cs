using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractible
{
    [SerializeField]private float speed;
    private bool isOpen = false;
    private Vector3 StartRotation;
    private bool isRotating = false;

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

    private IEnumerator doorOpen()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation;
        isRotating = true;

        endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - 90, 0));

        float time = 0;

        isOpen = true;
        while (time < 1)
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

        float time = 0;

        isOpen = false;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
            
        }
        Debug.Log("Finished Rotating");
        isRotating = false;
    }
}
