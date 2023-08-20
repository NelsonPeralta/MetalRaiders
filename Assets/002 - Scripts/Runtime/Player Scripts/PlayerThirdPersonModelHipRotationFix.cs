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
        if (_player.movement.speedRatio > 0)
            currentRotationFix -= Time.deltaTime * _rotationTarget * 5;
        else
            currentRotationFix += Time.deltaTime * _rotationTarget * 5;
        transform.localRotation = Quaternion.Euler(0, currentRotationFix, 0);
    }
}
