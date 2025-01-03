using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField]
    private GameObject InteractedObject;
    private bool isPressed = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Body")
        {
            if (!isPressed)
            {
                isPressed = true;
                InteractedObject.GetComponent<IInteractable>().Interact(gameObject);
            }
            

        }
    }
}
