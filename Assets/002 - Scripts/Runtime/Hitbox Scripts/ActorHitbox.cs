using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ActorHitbox : Hitbox, IDamageable
{
    public Actor actor;
    public void Damage(int d)
    {

    }

    public void Damage(int d, bool h, int pid)
    {
        actor.Damage((int)d, pid, isHeadshot: h);
    }

    public void Damage(int d, bool h, int pid, Vector3? impactPos = null, Vector3? impactDir = null,
        string damageSource = null, bool isGroin = false, int weaponIndx = -1,
        [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        actor.Damage((int)d, pid, isHeadshot: h ,weIndx: weaponIndx);
    }
}
