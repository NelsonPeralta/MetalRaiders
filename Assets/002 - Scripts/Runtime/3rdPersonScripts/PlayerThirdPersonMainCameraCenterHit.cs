using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonMainCameraCenterHit : MonoBehaviour
{
    public Transform target { get { return _witness; } }


    [SerializeField] Player _player;
    [SerializeField] Transform _hitTransform, _witness;
    [SerializeField] int _minimumDistanceToRegister = 5;


    RaycastHit _hit;





    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On
            && CurrentRoomManager.instance.gameStarted)
        {

            if (_player.aimAssist.targetHitbox && Vector3.Distance(_player.mainCamera.transform.position, _player.aimAssist.targetHitbox.transform.position) > _minimumDistanceToRegister)
            {
                _hitTransform = _player.aimAssist.targetHitbox.transform;
                _witness.transform.position = _player.aimAssist.targetHitbox.transform.position;
            }
            else if (Physics.Raycast(transform.position, transform.forward, out _hit, 100, GameManager.instance.bulletLayerMask) && Vector3.Distance(_player.mainCamera.transform.position, _hit.point) > _minimumDistanceToRegister)
            {
                // Does the ray intersect any objects excluding the player layer


                _hitTransform = _hit.transform;
                _witness.transform.position = _hit.point;
            }
            else
            {
                _witness.transform.localPosition = Vector3.forward * 100;
                _hitTransform = _witness;
            }
        }
    }
}
