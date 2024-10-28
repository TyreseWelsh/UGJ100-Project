using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void Damaged(int damage, GameObject attacker);

    public IEnumerator DamageFlash(SkinnedMeshRenderer meshRender, Material originalMaterial, Material damageFlashMaterial, float flashTime);
}
