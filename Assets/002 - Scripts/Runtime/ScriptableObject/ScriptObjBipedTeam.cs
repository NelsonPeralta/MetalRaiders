using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjBipedTeam", menuName = "ScriptableObjects/Biped Team", order = 30)]
public class ScriptObjBipedTeam : ScriptableObject
{
    public string playerName;
    public GameManager.Team team;
}
