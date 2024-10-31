using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider corpseDurabilitySlider;
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private RawImage staminaFill;
    [SerializeField] private TextMeshProUGUI livesText;
    private MainPlayerController playerScript;
    private StaminaComponent staminaScript;
    private int newMaxStamina;

    private void Awake()
    {
        InitialisePlayerHUD();
        
        MainPlayerController.onPlayerDeath += FindNewPlayer;
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

        livesText.text = playerScript.lives.ToString();

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

    public void FindNewPlayer(GameObject newPlayer)
    {
        playerCharacter = newPlayer;
        InitialisePlayerHUD();
    }
}
