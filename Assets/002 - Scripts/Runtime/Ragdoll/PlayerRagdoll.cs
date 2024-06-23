using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRagdoll : Ragdoll
{

    [SerializeField] Transform _cameraHolder;
    [SerializeField] Transform _cameraAnchor;
    [SerializeField] Transform _cameraHorAxis;
    [SerializeField] Transform _cameraVerAxis;


    Vector3 _v = new Vector3(0, 0.5f, 0);

    public override void ChildUpdate()
    {
        _cameraHolder.localPosition = hips.localPosition - _v;
    }

    public void SetPlayerCamera(PlayerCamera playerCameraScript, Camera playerMainCamera)
    {
        playerCameraScript.ragdollPrefab = this;

        playerMainCamera.transform.parent = _cameraAnchor.transform;

        playerCameraScript.transform.parent = _cameraAnchor.transform;
        playerCameraScript.horizontalAxisTarget = _cameraHorAxis.transform;
        playerCameraScript.verticalAxisTarget = _cameraVerAxis.transform;
    }


    public override void PlayerRagdollOnEnable()
    {
        if (GameManager.instance.connection == GameManager.Connection.Local)
        {
            _deathClipAudioSource.spatialBlend = .1f;
            _collisionAudioSource.spatialBlend = .1f;
        }
    }
}
