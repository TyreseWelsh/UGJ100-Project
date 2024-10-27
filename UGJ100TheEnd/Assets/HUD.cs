using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private RawImage staminaFill;
    private MainPlayerController playerScript;
    private StaminaComponent staminaScript;
    private int newMaxStamina;
    // Start is called before the first frame update
    void Start()
    {
        InitialisePlayerHUD();
    }

    // Update is called once per frame
    void Update()
    {
        staminaSlider.value =  staminaScript.currentStamina + (newMaxStamina - staminaScript.maxStamina);
        if(staminaScript.currentStamina < 0)
        {
            //staminaFill.color = new Color(210,98,0,255);      // Supposed to be orange (doesnt work for some reason)
            staminaFill.color = Color.grey;

        }
        else
        {
            staminaFill.color = Color.green;
        }
        healthSlider.value = playerScript.currentHealth;
        

    }

    void InitialisePlayerHUD()
    {
        if(playerCharacter != null)
        {
            playerScript = playerCharacter.GetComponent<MainPlayerController>();
            staminaScript = playerCharacter.GetComponent<StaminaComponent>();

            healthSlider.maxValue = playerScript.maxHealth;
            newMaxStamina = staminaScript.maxStamina + Mathf.Abs(staminaScript.negStaminaLimit);
            staminaSlider.maxValue = newMaxStamina;
        }
    }
    
    public void SetReferencedPlayer(GameObject newPlayer)
    {
        playerCharacter = newPlayer;
        
        InitialisePlayerHUD();
    }
}
