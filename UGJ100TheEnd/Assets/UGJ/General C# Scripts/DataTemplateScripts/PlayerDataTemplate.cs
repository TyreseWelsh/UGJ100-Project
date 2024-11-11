using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player Data Template")]
public class PlayerDataTemplate : CharacterDataTemplate
{
    [Header("Player Data")] 
    public int lives;
    
    [Header("Dash Data")]
    public float dashSpeed;
    public float dashDuration;
    public int dashCost;
    
    [Header("Revive Data")]
    public float reviveDuration;
    public Material reviveMaterial;
    
    [Header("Throw Data")]
    public float throwChargeRate;
    public int maxThrowForce;
    
    [Header("Block Data")] 
    public float parryDuration;
    public float parryStaminaGain;
    public int blockCost;
    public float blockConsumptionRate;
    public float brokenBlockRegenDelay;
}
