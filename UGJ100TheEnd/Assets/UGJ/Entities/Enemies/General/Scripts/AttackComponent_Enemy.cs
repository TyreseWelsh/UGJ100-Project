using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AttackComponent_Enemy : AttackComponent
{
    public void FireProjectile()
    {
        EnemyDataTemplate enemyData = characterData as EnemyDataTemplate;
        if (enemyData?.projectile)
        {
            Vector3 direction = gameObject.GetComponent<AIController>().GetMesh().transform.forward;
            direction.Normalize();
            GameObject currentProjectile = Instantiate(enemyData.projectile, weaponPosition.transform.position, Quaternion.identity);
            Rigidbody projectileRigibody = currentProjectile.GetComponent<Rigidbody>();
            if (projectileRigibody)
            {
                projectileRigibody.velocity = direction * 12f;
            }
        }
    }
}
