using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLootVFXOrientation : MonoBehaviour
{
    Vector3 o = new Vector3(-90, 0, 0);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(o);
    }
}
