using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomAISkinChooser : MonoBehaviour
{
    List<Transform> _skins = new List<Transform>();

    private void Awake()
    {
        _skins = GetComponentsInChildren<Transform>().ToList();
    }

    private void Start()
    {
        int ran = Random.Range(0, _skins.Count);

        _skins[0].gameObject.SetActive(false);
        _skins[ran].gameObject.SetActive(true);
    }
}
