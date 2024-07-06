using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonScript : MonoBehaviour
{
    public Player player;
    public List<SkinnedMeshRenderer> meshes = new List<SkinnedMeshRenderer>();
    public Transform handBone;


    public Animator animator { get { return _animator; } }


    Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        //if (!player.PV.IsMine)
        //    meshes[0].gameObject.layer = 0;
        //else
        //{
        //    int l = 31 - (6 - player.playerRewiredID);
        //    List<int> ignoreList = new List<int>();
        //    ignoreList.Add(LayerMask.NameToLayer("Hitbox"));

        //    GameManager.SetLayerRecursively(gameObject, l, ignoreList);
        //}
    }

    private void Update()
    {
        _animator.SetFloat("Vertical", player.movement.correctedZInput, 1f, Time.deltaTime * 10f);
        _animator.SetFloat("Horizontal", player.movement.correctedXInput, 1f, Time.deltaTime * 10f);

        //_animator.SetFloat("Vertical", ((Mathf.Abs(player.movement.correctedZInput) > PlayerMovement.STICK_DEADZONES) ? 0 : player.movement.correctedZInput), 1, 10);
        //_animator.SetFloat("Horizontal", ((Mathf.Abs(player.movement.correctedXInput) > PlayerMovement.STICK_DEADZONES) ? 0 : player.movement.correctedXInput), 1, 10);


        //_animator.SetFloat("Vertical", 0);
        //_animator.SetFloat("Horizontal", 0);

        //if (player.movement.movementDirection == PlayerMovement.PlayerMovementDirection.Forward)
        //    _animator.SetFloat("Vertical", 1, 1, 10);
        //if (player.movement.movementDirection == PlayerMovement.PlayerMovementDirection.Backwards)
        //    _animator.SetFloat("Vertical", -1, 1, 10);
        //if (player.movement.movementDirection == PlayerMovement.PlayerMovementDirection.Left)
        //    _animator.SetFloat("Horizontal", -1, 1, 10);
        //if (player.movement.movementDirection == PlayerMovement.PlayerMovementDirection.Right)
        //    _animator.SetFloat("Horizontal", -1, 1, 10);
    }

    public void EnableSkinnedMeshes()
    {
        foreach (SkinnedMeshRenderer smr in meshes)
            smr.enabled = true;
    }
    public void DisableSkinnedMeshes()
    {
        foreach (SkinnedMeshRenderer smr in meshes)
            smr.enabled = false;
    }
}