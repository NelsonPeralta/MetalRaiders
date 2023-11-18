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