using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedal : MonoBehaviour
{
    public AudioClip clip { get { return _clip; } }
    [SerializeField] AudioClip _clip;
    private void Start()
    {
        Destroy(gameObject, 6);
    }
}
