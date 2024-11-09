using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanHoldCorpse
{
    public void PickupCorpse(GameObject corpse);
    public void DropCorpse();
}
