using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class ActorDropshipAnimatorEvents : MonoBehaviour
{
    public ActorDropship actorDropship;

    public void SpawnActorsFromDropship()
    {
        SwarmManager.instance.SpawnActorsFromDropship(actorDropship);
    }

    public void DisableDropship()
    {
        actorDropship.gameObject.SetActive(false);
    }
}
