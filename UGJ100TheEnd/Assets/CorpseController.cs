using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseController : MonoBehaviour, IInteractible
{
    public CorpseType ECorpse;

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
        Debug.Log("Corpse not eaten");
    }
    public void InteractHeld(GameObject interactingObj) 
    {
        if(interactingObj.tag == "Player")
        {
            MainPlayerController playerScript = interactingObj.GetComponent<MainPlayerController>();
           
            if(playerScript.currentHealth + 10 > playerScript.maxHealth)
            {
                playerScript.currentHealth = playerScript.maxHealth;
            }
            else
            {
                playerScript.currentHealth = playerScript.currentHealth + 10;
            }
            Destroy(this.gameObject);
        }
    }

    public enum CorpseType
    {
        Ranged,
        Melee,
        Player
    };
}
