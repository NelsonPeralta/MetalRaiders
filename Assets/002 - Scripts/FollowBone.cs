using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBone : MonoBehaviour
{
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
