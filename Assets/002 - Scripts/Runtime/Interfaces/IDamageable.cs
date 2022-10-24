using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDamageable
{
    void Damage(int damage);
    void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId);
    void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null, bool isGroin = false);
}
