using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHitboxes : Hitboxes
{
    public void OnModelAssigned(PlayerThirdPersonModelManager playerThirdPersonModelManager)
    {

        _hitboxes = GetComponentsInChildren<Hitbox>().ToList();
        print($"PlayerHitboxes OnModelAssigned {transform.name} {_hitboxes.Count}");

        foreach (PlayerHitbox playerHitbox in _hitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            playerHitbox.player = GetComponent<Player>();
            playerHitbox.biped = playerHitbox.player;
            playerHitbox.gameObject.layer = 7;
        }
    }
}
