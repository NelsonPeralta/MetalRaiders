using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterXSeconds : MonoBehaviour
{
    public float countdown;

    float _countdown;

    private void Awake()
    {
        if (countdown <= 0) countdown = 1;
    }


    private void OnEnable()
    {
        _countdown = countdown;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_countdown > 0)
        {
            _countdown -= Time.deltaTime;

            if (_countdown < 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
