using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDamageable
{
    void Damage(int damage);
    void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId);
}
