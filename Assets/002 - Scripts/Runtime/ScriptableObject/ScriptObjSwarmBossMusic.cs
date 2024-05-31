using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjSwarmBossMusic", menuName = "ScriptableObjects/SwarmBossMusic", order = 50)]

public class ScriptObjSwarmBossMusic : ScriptableObject
{
    public AudioClip intro, loop, outro;
}
