using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfter4Seconds : MonoBehaviour
{
    public float countdown;



    private void OnEnable()
    {
        countdown = 4;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(countdown > 0)
        {
            countdown -= Time.deltaTime;

            if(countdown < 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
