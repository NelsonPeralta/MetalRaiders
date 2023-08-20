using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonModelHipRotationFix : MonoBehaviour
{
    [SerializeField] Player _player;

    int _rotationTarget = 30;

    private void Update()
    {
        if (_player.movement.speedRatio == 0)
            transform.localRotation = Quaternion.Euler(0, _rotationTarget, 0);
        else
            transform.localRotation = Quaternion.Euler(0, _rotationTarget - _player.movement.speedRatio * _rotationTarget, 0);
    }
}
