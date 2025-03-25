using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonBulletSpawnPointDetach : MonoBehaviour
{
    [SerializeField] Transform _followPosition;



    private void Awake()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
        {
            //transform.parent = null;
        }
        else
        {
            //this.enabled = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = _followPosition.position;
    }
}
