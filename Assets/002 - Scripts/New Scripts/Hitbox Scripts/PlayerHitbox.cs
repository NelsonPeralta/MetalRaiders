using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public bool isHead = false;

    public PlayerProperties player;
    public GameObject boneToFollow;

    private void Update()
    {
        if (boneToFollow != null)
        {
            gameObject.transform.position = boneToFollow.transform.position;
            gameObject.transform.rotation = boneToFollow.transform.rotation;
        }
    }
}
