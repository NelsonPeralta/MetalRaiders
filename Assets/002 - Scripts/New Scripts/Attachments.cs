using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attachments : MonoBehaviour
{

    [Header("Attachments")]
    public GameObject[] attachments = new GameObject[6];

    public bool ironSights;
    public bool silencer;
    public bool scope1;
    public bool scope2;
    public bool scope3;
    public bool scope4;

    private int childCounter = 0;
    

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.tag == "Attachment")
            {

                attachments[childCounter] = child.gameObject;
                            
                childCounter = childCounter + 1;
                       
            }


        }
    }

    public void Update()
    {

        if (ironSights == true && attachments[0] != null)////////////////////Ironsights
        {
            attachments[0].gameObject.SetActive(true);
        }
        else
        {
            attachments[0].gameObject.SetActive(false);
        }


        if (silencer == true && attachments[1] != null)////////////////////Silencer
        {
            attachments[1].gameObject.SetActive(true);
        }
        else
        {
            attachments[1].gameObject.SetActive(false);
        }


        if (scope1 == true && attachments[2] != null)//////////////////Scope 1
        {
            attachments[2].gameObject.SetActive(true);
        }
        else
        {
            attachments[2].gameObject.SetActive(false);
        }


        if (scope2 == true && attachments[3] != null)/////////////////Scope 2
        {
            attachments[3].gameObject.SetActive(true);
        }
        else
        {
            attachments[3].gameObject.SetActive(false);
        }


        if (scope3 == true && attachments[4] != null)//////////////////Scope 3
        {
            attachments[4].gameObject.SetActive(true);
        }
        else
        {
            attachments[4].gameObject.SetActive(false);
        }


        if (scope4 == true && attachments[5] != null)////////////////////Scope 4
        {
            attachments[5].gameObject.SetActive(true);
        }
        else
        {
            attachments[5].gameObject.SetActive(false);
        }
    }
    
}
