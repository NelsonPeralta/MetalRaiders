using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxes : MonoBehaviour
{
    public PlayerHitbox[] playerHitboxes { get { return _playerHitboxes; } }
    [SerializeField] PlayerHitbox[] _playerHitboxes = null;
    private void Awake()
    {
    }

    public void OnModelAssigned(PlayerThirdPersonModelManager playerThirdPersonModelManager)
    {
        //Debug.Log($"PlayerHitboxes OnModelAssigned {GetComponent<Player>().nickName}");
        _playerHitboxes = GetComponentsInChildren<PlayerHitbox>();

        foreach (PlayerHitbox playerHitbox in _playerHitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            playerHitbox.player = GetComponent<Player>();
            playerHitbox.gameObject.layer = 7;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            foreach (PlayerHitbox playerHitbox in _playerHitboxes)
                try
                {
                    playerHitbox.GetComponent<MeshRenderer>().enabled = !playerHitbox.GetComponent<MeshRenderer>().enabled;
                }
                catch { }
    }
}
