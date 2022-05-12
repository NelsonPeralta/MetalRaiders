using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticuleFriction : MonoBehaviour
{
    public AiAbstractClass ai;
    public Player player;

    private void Start()
    {
        if (ai)
        {
            ai.OnDeath -= OnAiDeath;
            ai.OnDeath += OnAiDeath;

            ai.OnPrepareEnd -= OnAiPrepare;
            ai.OnPrepareEnd += OnAiPrepare;
        }

        if (player)
        {
            player.OnPlayerDeath -= OnPlayerRespawn;
            player.OnPlayerDeath += OnPlayerRespawn;

            player.OnPlayerRespawned -= OnPlayerRespawn;
            player.OnPlayerRespawned += OnPlayerRespawn;
        }
    }

    void OnAiDeath(AiAbstractClass aiAbstractClass)
    {
        gameObject.SetActive(false);
    }

    void OnAiPrepare(AiAbstractClass aiAbstractClass)
    {
        gameObject.SetActive(true);
    }

    void OnPlayerDeath(Player player)
    {
        gameObject.SetActive(false);
    }

    void OnPlayerRespawn(Player player)
    {
        gameObject.SetActive(true);
    }
}
