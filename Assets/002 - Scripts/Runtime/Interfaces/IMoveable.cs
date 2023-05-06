using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PushSource { Grenade, ManCannon }
interface IMoveable
{
    public void Push(Vector3 direction, int power, PushSource ps, bool blockMovement);
}