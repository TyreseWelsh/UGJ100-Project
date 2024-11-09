using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeleeWeapon
{
    public void EnableCollider();
    public void DisableCollider();
    public void ClearDamagedObjects();
}
