using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioSourceNetworkBehaviour : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.instance.connection == GameManager.NetworkType.Local || GameManager.instance.nbLocalPlayersPreset > 1)
        {
            GetComponent<AudioSource>().spatialBlend = 0;
        }
        else
        {
            GetComponent<AudioSource>().spatialBlend = 0.98f;

            if (transform.root.GetComponent<Player>() && !transform.root.GetComponent<Player>().isMine)
                GetComponent<AudioSource>().volume = 0.5f;
        }
    }
}
