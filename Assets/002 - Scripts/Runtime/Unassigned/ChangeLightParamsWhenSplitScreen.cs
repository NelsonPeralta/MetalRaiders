using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLightParamsWhenSplitScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.nbLocalPlayersPreset > 1)
        {
            if (GetComponent<Light>())
            {
                GetComponent<Light>().shadows = LightShadows.None;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
