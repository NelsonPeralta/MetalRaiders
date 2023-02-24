using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomAISkinChooser : MonoBehaviour
{
    public List<Transform> _skins = new List<Transform>();
    int lastRan;

    private void Awake()
    {
        _skins = GetComponentsInChildren<Transform>(true).ToList();
        lastRan = 1;
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        _skins[lastRan].gameObject.SetActive(false);

        int ran = Random.Range(1, _skins.Count);
        lastRan = ran;

        _skins[ran].gameObject.SetActive(true);
    }
}
