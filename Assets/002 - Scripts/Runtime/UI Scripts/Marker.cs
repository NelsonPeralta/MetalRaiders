using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Transform lookAtThisTrans, localScale;



    // Update is called once per frame
    void Update()
    {
        if (lookAtThisTrans)
        {
            Vector3 targetPostition = new Vector3(lookAtThisTrans.transform.position.x,
                                        this.transform.position.y,
                                        lookAtThisTrans.transform.position.z);
            this.transform.LookAt(targetPostition);


            //print(Vector3.Distance(transform.position, lookAtThisTrans.position) / 100);
            //print(Mathf.Clamp(Vector3.Distance(transform.position, lookAtThisTrans.position) / 100, .01f, 1));
            localScale.localScale = Vector3.one * (Mathf.Clamp(Vector3.Distance(transform.position, lookAtThisTrans.position) / 30, .03f, 1));
        }
    }

    private void OnDisable()
    {
        lookAtThisTrans = null;
    }
}
