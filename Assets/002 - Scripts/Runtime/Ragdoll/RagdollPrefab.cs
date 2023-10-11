using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPrefab : MonoBehaviour
{
    public Rigidbody ragdollRigidBody { get { return _ragdollRigidBody; } }
    public Transform cameraHolderParent { get { return _cameraHolderParent; } }

    public Transform ragdollHead;
    public Transform ragdollChest;
    public Transform ragdollHips;
    [Space(10)]
    public Transform ragdollUpperArmLeft;
    public Transform ragdollUpperArmRight;
    [Space(10)]
    public Transform ragdollLowerArmLeft;
    public Transform ragdollLowerArmRight;
    [Space(10)]
    public Transform ragdollUpperLegLeft;
    public Transform ragdollUpperLegRight;
    [Space(10)]
    public Transform ragdollLowerLegLeft;
    public Transform ragdollLowerLegRight;

    public List<AudioClip> deathClips;


    [SerializeField] Rigidbody _ragdollRigidBody;
    [SerializeField] Transform _cameraHolderParent;
    [SerializeField] GameObject _cameraHolder;
    [SerializeField] GameObject _cameraHolderHorizontalAxis;
    [SerializeField] GameObject _cameraHolderVerticalAxis;

    private void Start()
    {
        int randomSound = Random.Range(0, deathClips.Count);
        GetComponent<AudioSource>().clip = deathClips[randomSound];
        GetComponent<AudioSource>().Play();
    }

    public void SetPlayerCamera(PlayerCamera playerCameraScript, Camera playerMainCamera)
    {
        //playerCameraScript.ragdollPrefab = this;

        playerMainCamera.transform.parent = _cameraHolder.transform;

        playerCameraScript.transform.parent = _cameraHolder.transform;
        playerCameraScript.horizontalAxisTarget = _cameraHolderHorizontalAxis.transform;
        playerCameraScript.verticalAxisTarget = _cameraHolderVerticalAxis.transform;
    }
}