using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAnimatorInXSeconds : MonoBehaviour
{
    public float t;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (t > 0)
        {
            t -= Time.deltaTime;

            if (t <= 0)
            {
                GetComponent<Animator>().enabled = true;
            }
        }
    }
}
