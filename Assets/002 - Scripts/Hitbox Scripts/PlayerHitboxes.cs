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
        {
            Debug.Log(playerHitboxes.Length);

            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            playerHitbox.player = GetComponent<Player>();
            playerHitbox.gameObject.layer = 7;
            Debug.Log(playerHitbox.gameObject.layer);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            foreach (PlayerHitbox playerHitbox in playerHitboxes)
                try
                {
                    playerHitbox.GetComponent<MeshRenderer>().enabled = !playerHitbox.GetComponent<MeshRenderer>().enabled;
                }
                catch { }
    }
}
