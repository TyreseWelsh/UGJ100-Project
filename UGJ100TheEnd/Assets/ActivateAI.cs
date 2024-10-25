using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAI : MonoBehaviour
{
    [SerializeField] private GameObject[] activatableAI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            foreach(GameObject AI in activatableAI)
            {
                AI.GetComponent<IInteractable>().Interact(gameObject);
            }
        }
    }
}
