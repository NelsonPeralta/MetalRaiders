using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : Hitbox
{
    public Player player;
    public GameObject boneToFollow;

    private void Update()
    {
        try
        {
            if (boneToFollow != null)
            {
                gameObject.transform.position = boneToFollow.transform.position;
                gameObject.transform.rotation = boneToFollow.transform.rotation;
            }
        }
        catch (System.Exception) { }
    }
}
