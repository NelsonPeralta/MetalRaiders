using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHitboxes : Hitboxes
{
    public void OnModelAssigned(PlayerThirdPersonModelManager playerThirdPersonModelManager)
    {
        Debug.Log($"PlayerHitboxes OnModelAssigned {GetComponent<Player>().nickName}");
        _hitboxes = GetComponentsInChildren<Hitbox>().ToList();

        foreach (PlayerHitbox playerHitbox in _hitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            playerHitbox.player = GetComponent<Player>();
            playerHitbox.biped = playerHitbox.player;
            playerHitbox.gameObject.layer = 7;
        }
    }
}
