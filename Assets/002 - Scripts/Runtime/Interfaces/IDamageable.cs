using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

interface IDamageable
{
    void Damage(int d);
    void Damage(int d, bool h, int pid);
    void Damage(int d, bool h, int pid, 
        Vector3? impactPos = null, 
        Vector3? impactDir = null, 
        string damageSource = null, 
        bool isGroin = false, 
        int weaponIndx = -1, 
        WeaponProperties.KillFeedOutput kfo = WeaponProperties.KillFeedOutput.Unassigned,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}
