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
    }

    void OnAiDeath(AiAbstractClass aiAbstractClass)
    {
        gameObject.SetActive(false);
    }

    void OnAiPrepare(AiAbstractClass aiAbstractClass)
    {
        gameObject.SetActive(true);
    }
}
