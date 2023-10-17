using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PropHitbox : Hitbox, IDamageable
{
    public void Damage(int d)
    {
        Debug.Log("PropHitbox Damage");
        //hitPoints.hitPoints -= d;
    }

    public void Damage(int d, bool h, int pid)
    {
    }

    public void Damage(int d, bool h, int pid, Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, bool isGroin = false, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        //hitPoints.hitPoints -= d;
    }
}
