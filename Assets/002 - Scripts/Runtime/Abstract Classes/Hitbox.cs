using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitbox : MonoBehaviour
{
    public Biped biped;
    public Hitboxes hitboxesScript;
    public HitPoints hitPoints;
    public bool isHead, isGroin, ignoreForAimAssistList;
}
