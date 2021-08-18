using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public int defaultPlayerSlope;
    public int defaultPlayerStepOffset;
    public int defaultPlayerMovementSpeed;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<CharacterController>())
            return;
        if(defaultPlayerSlope == 0)
            defaultPlayerSlope = (int)other.GetComponent<CharacterController>().slopeLimit;
        if (defaultPlayerStepOffset == 0)
            defaultPlayerStepOffset = (int)other.GetComponent<CharacterController>().stepOffset;
        if (defaultPlayerMovementSpeed == 0)
            defaultPlayerMovementSpeed = (int)other.GetComponent<Movement>().defaultSpeed;

            other.GetComponent<CharacterController>().slopeLimit = 180;
        other.GetComponent<CharacterController>().stepOffset = 1;
        other.GetComponent<Movement>().defaultSpeed = other.GetComponent<Movement>().defaultSpeed / 3;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<CharacterController>())
            return;


        other.GetComponent<CharacterController>().slopeLimit = defaultPlayerSlope;
        other.GetComponent<CharacterController>().stepOffset = defaultPlayerStepOffset;
        other.GetComponent<Movement>().defaultSpeed = defaultPlayerMovementSpeed;
    }
}
