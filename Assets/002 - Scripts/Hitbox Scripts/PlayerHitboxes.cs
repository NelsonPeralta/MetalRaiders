using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxes : MonoBehaviour
{
    [SerializeField] PlayerHitbox[] playerHitboxes = null;
    private void Awake()
    {
    }

    public void OnModelAssigned(PlayerThirdPersonModelManager playerThirdPersonModelManager)
    {
        Debug.Log("PlayerHitboxes OnModelAssigned");
        playerHitboxes = GetComponentsInChildren<PlayerHitbox>();

        foreach (PlayerHitbox playerHitbox in playerHitboxes)
            playerHitbox.player = GetComponent<Player>();
    }
}
