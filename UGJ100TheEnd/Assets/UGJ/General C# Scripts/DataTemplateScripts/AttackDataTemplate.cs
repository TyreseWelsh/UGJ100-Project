using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Attack Data Template")]
public class AttackDataTemplate : ScriptableObject
{
    public string animationName;
    public int damage;
    public float animationSpeed;
}
