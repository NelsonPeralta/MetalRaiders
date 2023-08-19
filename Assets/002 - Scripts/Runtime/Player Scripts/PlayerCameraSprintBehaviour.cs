using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSprintBehaviour : MonoBehaviour
{
    float hint
    {
        get { return _hint; }
        set
        {
            _hint = Mathf.Clamp(value, -_fovSubstractionTarget, 0);
        }
    }

    [SerializeField] Player _player;
    [SerializeField] float _hint;

    int _fovSubstractionTarget = 10;

    private void Update()
    {
        if (_player.playerController.isAiming)
        {
            hint = 0;
            return;
        }


        if (_player.playerController.isSprinting)
        {
            hint -= Time.deltaTime * _fovSubstractionTarget * 0.8f;
        }
        else
        {
            hint += Time.deltaTime * _fovSubstractionTarget * 5;
        }

        //_player.mainCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(Camera.VerticalToHorizontalFieldOfView(_player.mainCamera.fieldOfView, _player.mainCamera.aspect) + hint, _player.mainCamera.aspect);

        _player.mainCamera.fieldOfView = _player.defaultVerticalFov + hint;
    }
}
