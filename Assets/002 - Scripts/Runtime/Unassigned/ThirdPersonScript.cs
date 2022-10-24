using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonScript : MonoBehaviour
{
    public Player player;
    public List<SkinnedMeshRenderer> meshes = new List<SkinnedMeshRenderer>();
    public Transform handBone;

    private void Start()
    {
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
        GetComponent<Animator>().SetFloat("Vertical", player.GetComponent<Movement>().zDirection, 1f, Time.deltaTime * 10f);
        GetComponent<Animator>().SetFloat("Horizontal", player.GetComponent<Movement>().xDirection, 1f, Time.deltaTime * 10f);
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