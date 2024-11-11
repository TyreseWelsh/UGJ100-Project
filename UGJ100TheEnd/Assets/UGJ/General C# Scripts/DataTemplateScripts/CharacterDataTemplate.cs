using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character Data Template")]
public class CharacterDataTemplate : ScriptableObject
{
    [Header("Character Data")]
    public int health;
    public float moveSpeed;
    
    public GameObject weapon;
    public ComboDataTemplate basicComboData;
    
    [Header("Damaged Data")] 
    public Material damageFlashMaterial;
    public Material originalMaterial;
    public float damageFlashDuration;
    public float knockbackForce;
}
