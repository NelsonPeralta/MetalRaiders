using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonModelHipRotationFix : MonoBehaviour
{
    float currentRotationFix
    {
        get { return _currentRotationFix; }
        set
        {
            //_currentRotationFix = Mathf.RoundToInt(value / 10) * 10;

            _currentRotationFix = Mathf.Clamp(value, 0, 30);
        }
    }

    [SerializeField] Player _player;
    [SerializeField] float _currentRotationFix = 0;

    int _rotationTarget = 30;

    private void Update()
    {
        if (_player.movement.currentWorldSpeed > 0.1f)
        {
            if (_player.movement.isGrounded && !_player.playerController.isCrouching)
                currentRotationFix -= Time.deltaTime * _rotationTarget * 7;
            else
                currentRotationFix += Time.deltaTime * _rotationTarget * 7;
        }
        else
            currentRotationFix += Time.deltaTime * _rotationTarget * 7;
        transform.localRotation = Quaternion.Euler(0, currentRotationFix, 0);
    }
}
