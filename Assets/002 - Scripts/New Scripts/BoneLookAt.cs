using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLookAt : MonoBehaviour
{
    public Animator anim;
    public Transform lookAtGO;
    public Transform head;
    public bool disactive;

    public int directionIndicator;

    [Header("Offsets")]
    public Vector3 currentOffset;
    public Vector3 iddleOffset;

    private void Start()
    {
        head = anim.GetBoneTransform(HumanBodyBones.Neck);
        currentOffset = iddleOffset;
    }

    private void LateUpdate()
    {
        if (!disactive)
        {
            head.LookAt(lookAtGO.position);
            head.rotation = head.rotation * Quaternion.Euler(currentOffset);
        }
    }

    public void UpdateOffset()
    {
        currentOffset = iddleOffset;
    }
}
