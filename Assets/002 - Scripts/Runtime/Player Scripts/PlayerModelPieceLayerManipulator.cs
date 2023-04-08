using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelPieceLayerManipulator : MonoBehaviour
{
    [SerializeField] Player player;
    void Start()
    {
        player = transform.root.GetComponent<Player>();

        if (!player.isMine)
        {
            gameObject.layer = 0;
            return;
        }

        int playerRewiredId = player.GetComponent<PlayerController>().rid;

        if (playerRewiredId == 0)
            gameObject.layer = 25;

        if (playerRewiredId == 1)
            gameObject.layer = 27;
    }
}
