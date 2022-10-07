using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitboxes : MonoBehaviour
{
    public PlayerHitbox[] playerHitboxes;
    public AIHitbox[] AIHitboxes;

    private void Start()
    {
        playerHitboxes = GetComponentsInChildren<PlayerHitbox>();
        AIHitboxes = GetComponentsInChildren<AIHitbox>();

        try
        {
            foreach (AIHitbox aIHitbox in AIHitboxes)
                aIHitbox.aiAbstractClass = GetComponent<AiAbstractClass>();
        }
        catch { }
    }
}
