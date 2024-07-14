using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCameraFarFix : MonoBehaviour
{
    Camera _cam;
    int _c;


    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_cam)
        {
            if (_c % 2 == 0)
                _cam.farClipPlane = 2.9f;
            else
                _cam.farClipPlane = 3.1f;


            _c++;
            if (_c > 9) _c = 0;
        }
    }
}
