using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSHandLookAt : MonoBehaviour
{
    public Animator anim;
    public PlayerController pController;
    public Transform lookAtGO;
    public Transform hand;
    public Movement movement;

    private void LateUpdate()
    {
        anim = pController.anim;
        hand = anim.GetBoneTransform(HumanBodyBones.LeftHand);

        //UpdateOffset(movement.directionIndicator);
        //directionIndicator = movement.directionIndicator;
        hand.LookAt(lookAtGO.position);
        //hand.rotation = hand.rotation * Quaternion.Euler(currentOffset);
    }
    
}
