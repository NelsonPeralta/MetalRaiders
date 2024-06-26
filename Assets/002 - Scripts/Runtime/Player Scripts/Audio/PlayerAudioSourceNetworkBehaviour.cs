using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioSourceNetworkBehaviour : MonoBehaviour
{
    enum AudioBehaviour { World, UI }

    Player _player;

    private void Start()
    {
        _player = transform.root.GetComponent<Player>();

        if (GameManager.instance.connection == GameManager.Connection.Local)
        {
            GetComponent<AudioSource>().spatialBlend = 0;
        }
        else
        {
            GetComponent<AudioSource>().spatialBlend = 0.98f;
        }
    }
}
