using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRagdoll : Ragdoll
{
    public bool isMine;
    public AudioSource respawnBeepAudioSource;


    [SerializeField] Transform _cameraHolder;
    [SerializeField] Transform _cameraAnchor;
    [SerializeField] Transform _cameraHorAxis;
    [SerializeField] Transform _cameraVerAxis;

    float _respawnCountdown;
    int _respawnBeepCount;


    Vector3 _v = new Vector3(0, 0.5f, 0);

    public override void ChildUpdate()
    {
        _cameraHolder.localPosition = hips.localPosition - _v;



        if (_respawnCountdown > 0)
        {
            _respawnCountdown -= Time.deltaTime;

            if (isMine)
                if (_respawnCountdown <= 3 && _respawnBeepCount == 0)
                {
                    _respawnBeepCount++;
                    respawnBeepAudioSource.Play();
                }
                else if (_respawnCountdown <= 2 && _respawnBeepCount == 1)
                {
                    _respawnBeepCount++;
                    respawnBeepAudioSource.Play();
                }
                else if (_respawnCountdown <= 1 && _respawnBeepCount == 2)
                {
                    _respawnBeepCount++;
                    respawnBeepAudioSource.Play();
                }
        }
    }

    public void SetPlayerCamera(PlayerCamera playerCameraScript, Camera playerMainCamera)
    {
        playerCameraScript.ragdollPrefab = this;

        playerMainCamera.transform.position = _cameraAnchor.transform.position;
        playerMainCamera.transform.rotation = _cameraAnchor.transform.rotation;
        playerMainCamera.transform.parent = _cameraAnchor.transform;

        playerCameraScript.transform.parent = _cameraAnchor.transform;
        playerCameraScript.horizontalAxisTarget = _cameraHorAxis.transform;
        playerCameraScript.verticalAxisTarget = _cameraVerAxis.transform;
    }


    public override void PlayerRagdollOnEnable()
    {
        _respawnCountdown = Player.RESPAWN_TIME;
        _respawnBeepCount = 0;
        if (GameManager.instance.connection == GameManager.Connection.Local || GameManager.instance.nbLocalPlayersPreset > 1)
        {
            _deathClipAudioSource.spatialBlend = .1f;
            _collisionAudioSource.spatialBlend = .1f;
        }
    }


    private void OnDisable()
    {
        GetComponent<Animator>().enabled = true;
        ResetRigidbodieVelocities();
    }

    public void ResetRigidbodieVelocities()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true)) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
    }
}
