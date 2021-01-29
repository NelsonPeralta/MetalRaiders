using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonLookAt : MonoBehaviour
{
    public Animator anim;
    public Transform lookAtGO;
    public Transform chest;
    public Movement movement;

    public int directionIndicator;

    [Header("Offsets")]
    public Vector3 currentOffset;
    public Vector3 iddleOffset;
    [Space(15)]
    public Vector3 LeftOffset;
    public Vector3 LeftForwardOffset;
    public Vector3 ForwardOffset;
    public Vector3 RightForwardOffest;
    public Vector3 RightOffset;
    public Vector3 RightBackwardsOffset;
    public Vector3 BackwardsOffset;
    public Vector3 LeftBackwardsOffset;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if(!chest)
            chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        currentOffset = iddleOffset;
    }

    private void LateUpdate()
    {
        //UpdateOffset(movement.directionIndicator);
        chest.LookAt(lookAtGO.position);
        chest.rotation = chest.rotation * Quaternion.Euler(currentOffset);
    }

    public void UpdateOffset(int directionIndicator)
    {
        if (directionIndicator == 0) // Idle
        {
            currentOffset = iddleOffset;
        }
        else if (directionIndicator == 1) // Left
        {
            currentOffset = LeftOffset;
        }
        else if (directionIndicator == 2) // Left Forward
        {
            currentOffset = LeftForwardOffset;
        }
        else if (directionIndicator == 3) // Forward
        {
            currentOffset = ForwardOffset;
        }
        else if (directionIndicator == 4)// Right Forward
        {
            currentOffset = RightForwardOffest;
        }
        else if (directionIndicator == 5)// Right
        {
            currentOffset = RightOffset;
        }
        else if (directionIndicator == 6)// Right Backwards
        {
            currentOffset = RightBackwardsOffset;
        }
        else if (directionIndicator == 7)// Backwards
        {
            currentOffset = BackwardsOffset;
        }
        else if (directionIndicator == 8)// Left Backwards
        {
            currentOffset = LeftBackwardsOffset;
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
        anim.SetLookAtPosition(lookAtGO.transform.position);
        gameObject.transform.LookAt(lookAtGO.transform);

    }

    private void OnAnimatorIK(int layerIndex)
    {
        Transform head = anim.GetBoneTransform(HumanBodyBones.Head);
        Vector3 forward = (lookAtGO.transform.position - head.position).normalized;
        Vector3 up = Vector3.Cross(forward, transform.right);
        Quaternion rotation = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(forward, up);
        anim.SetBoneLocalRotation(HumanBodyBones.Head, rotation);
    }
    */
}
