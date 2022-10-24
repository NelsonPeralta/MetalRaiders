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
        //Debug.Log($"PlayerHitboxes OnModelAssigned {GetComponent<Player>().nickName}");
        playerHitboxes = GetComponentsInChildren<PlayerHitbox>();

        foreach (PlayerHitbox playerHitbox in playerHitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            playerHitbox.player = GetComponent<Player>();
            playerHitbox.gameObject.layer = 7;
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
