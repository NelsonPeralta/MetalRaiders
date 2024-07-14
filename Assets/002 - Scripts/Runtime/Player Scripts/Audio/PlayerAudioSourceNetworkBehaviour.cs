using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioSourceNetworkBehaviour : MonoBehaviour
{
    private void Start()
    {
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
