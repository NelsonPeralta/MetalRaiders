using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

public class FakeBulletTrailDisable : MonoBehaviour
{
    public static int Speed = 300;


    public Player player;
    public float timeBeforeDisabling;

    private void OnEnable()
    {
        timeBeforeDisabling = 0.1f;
    }

    private void Update()
    {
        if(timeBeforeDisabling > 0)
        {
            timeBeforeDisabling -= Time.deltaTime;

            if(timeBeforeDisabling <= 0)
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
            //else
            //{
            //    transform.localScale = Vector3.one;
            //    transform.Translate(Vector3.forward * Time.deltaTime * Speed);
            //}
        }
    }
}
