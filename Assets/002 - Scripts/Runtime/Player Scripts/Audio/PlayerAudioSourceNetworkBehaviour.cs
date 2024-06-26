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

        if (_player.PV.IsMine)
            GetComponent<AudioSource>().spatialBlend = 0;
        else
        {
            GetComponent<AudioSource>().spatialBlend = 0.95f;
            GetComponent<AudioSource>().volume = 1;
        }

    }
}
