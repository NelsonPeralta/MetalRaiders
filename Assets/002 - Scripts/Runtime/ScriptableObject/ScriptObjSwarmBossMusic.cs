using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjSwarmBossMusic", menuName = "ScriptableObjects/SwarmBossMusic", order = 2)]

public class ScriptObjSwarmBossMusic : ScriptableObject
{
    public AudioClip intro, loop, outro;
}
