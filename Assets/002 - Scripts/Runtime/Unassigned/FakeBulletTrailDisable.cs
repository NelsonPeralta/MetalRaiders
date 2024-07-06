using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeBulletTrailDisable : MonoBehaviour
{
    public Player player;
    float _c;

    private void OnEnable()
    {
        _c = 0.1f;
    }

    private void Update()
    {
        if(_c > 0)
        {
            _c -= Time.deltaTime;

            if(_c <= 0)
            {
                transform.parent = player.playerInventory.bulletTrailPool;

                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;


                if (GameManager.instance.connection == GameManager.Connection.Local)
                {
                    if (player.rid == 0) transform.GetChild(0).gameObject.layer = 25;
                    else if (player.rid == 1) transform.GetChild(0).gameObject.layer = 27;
                    else if (player.rid == 2) transform.GetChild(0).gameObject.layer = 29;
                    else if (player.rid == 3) transform.GetChild(0).gameObject.layer = 31;
                }


                gameObject.SetActive(false);
            }
        }
    }
}
