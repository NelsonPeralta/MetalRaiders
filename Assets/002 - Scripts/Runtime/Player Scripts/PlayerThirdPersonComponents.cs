using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonComponents : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] PlayerThirdPersonMainCameraCenterHit _mainCameraCenterHit;
    [SerializeField]
    Transform _thirdPersonCameraWorldAnchor, _thirdPersonCameraPositionOffset,
        _tpsBulletSpawnPointParent, _tpsBulletSpawnPointRotationControl, _tpsFakeTrailsParent;
    [SerializeField] Vector3 _directionFromBulletSpawnPointToCameraCenterTarget;




    private void Awake()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
        {
            _player.playerController.melee.transform.parent = _player.playerInventory.transform;




            _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition =
           new Vector3(_player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.x,
               _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.y,
               _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.z - (PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z * 2));
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On && CurrentRoomManager.instance.gameStarted)
        {
            _thirdPersonCameraWorldAnchor.transform.position = _player.playerCamera.playerCameraHolderPosition;

            _tpsBulletSpawnPointRotationControl.LookAt(_mainCameraCenterHit.target.position);
        }
    }


    public void TriggerThirdPersonMode()
    {
        _thirdPersonCameraWorldAnchor.transform.parent = null;

        _player.playerController.gwProperties.bulletSpawnPoint.parent = _tpsBulletSpawnPointParent;
        _player.playerController.gwProperties.bulletSpawnPoint.rotation = Quaternion.identity;
        _player.playerController.gwProperties.bulletSpawnPoint.localPosition = Vector3.zero;

        _player.playerInventory.bulletTrailHolder.parent = _tpsFakeTrailsParent.transform;
        _player.playerInventory.bulletTrailHolder.rotation = Quaternion.identity;
        _player.playerInventory.bulletTrailHolder.localPosition = Vector3.zero;



        _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition =
            new Vector3(_player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.x,
                _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.y,
                _player.movement.thirdPersonLookAtScript.lookAtGO.localPosition.z - (PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z * 2));


        _player.playerCamera.EnableThirdPerson();
        _player.playerController.gwProperties.SetOriginalBulletLocalPositionAndRotation();
    }

    public void UpdateAnchorRotationFromCameraScript(float upDownRotation, float leftRightRotation)
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On && CurrentRoomManager.instance.gameStarted)
        {
            _thirdPersonCameraWorldAnchor.transform.localRotation = Quaternion.Euler(upDownRotation, leftRightRotation, 0f);
        }
    }


    public void UpdateCameraRotationAndPosition()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On && CurrentRoomManager.instance.gameStarted)
        {
            _player.mainCamera.transform.forward = _thirdPersonCameraWorldAnchor.transform.forward;
            _player.mainCamera.transform.position = _thirdPersonCameraPositionOffset.position;
        }
    }
}
