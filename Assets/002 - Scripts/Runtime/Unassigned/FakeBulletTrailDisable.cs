using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeBulletTrailDisable : MonoBehaviour
{
    public static int Speed = 240;


    public Player player;
    public float timeBeforeDisabling;


    private void Update()
    {
        if(timeBeforeDisabling > 0)
        {
            timeBeforeDisabling -= Time.deltaTime;

            if(timeBeforeDisabling <= 0)
            {
                TriggerDisable();
            }
            //else
            //{
            //    transform.localScale = Vector3.one;
            //    transform.Translate(Vector3.forward * Time.deltaTime * Speed);
            //}
        }
    }

    public void TriggerDisable()
    {
        transform.parent = player.playerInventory.bulletTrailHolder;

        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;


        if (GameManager.instance.connection == GameManager.NetworkType.Local)
        {
            if (player.rid == 0) transform.GetChild(0).gameObject.layer = 25;
            else if (player.rid == 1) transform.GetChild(0).gameObject.layer = 27;
            else if (player.rid == 2) transform.GetChild(0).gameObject.layer = 29;
            else if (player.rid == 3) transform.GetChild(0).gameObject.layer = 31;
        }


        gameObject.SetActive(false);
    }
}
