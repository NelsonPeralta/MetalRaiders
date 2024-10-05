using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Player targetPlayer;
    public Transform localScale;



    // Update is called once per frame
    void Update()
    {
        if (targetPlayer)
        {
            Vector3 targetPostition = new Vector3(targetPlayer.transform.position.x,
                                        this.transform.position.y,
                                        targetPlayer.transform.position.z);
            this.transform.LookAt(targetPostition);


            //print(Vector3.Distance(transform.position, lookAtThisTrans.position) / 100);
            //print(Mathf.Clamp(Vector3.Distance(transform.position, lookAtThisTrans.position) / 100, .01f, 1));
            localScale.localScale = Vector3.one * (Mathf.Clamp(Vector3.Distance(transform.position, targetPlayer.transform.position) / 30, .03f, 1));

            if (!targetPlayer.isAlive)
            {
                targetPlayer = null;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        targetPlayer = null;
    }
}
