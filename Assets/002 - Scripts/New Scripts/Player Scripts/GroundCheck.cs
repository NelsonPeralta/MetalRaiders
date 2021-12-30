using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerProperties player;
    public bool isGrounded;

    //public List<GameObject> objectsCollidingWith = new List<GameObject>();
    //private void OnTriggerEnter(Collider other)
    //{
    //    //Debug.Log($"Ground Check, Root: {other.transform.root}");
    //    if (other.transform.root.gameObject != player.gameObject){
    //        objectsCollidingWith.Add(other.gameObject);
    //        isGrounded = true;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    for(int i = 0; i < objectsCollidingWith.Count; i++)
    //        if (other.gameObject == objectsCollidingWith[i])
    //            objectsCollidingWith.RemoveAt(i);

    //    if (objectsCollidingWith.Count == 0)
    //        isGrounded = false;
    //}

    private void OnTriggerExit(Collider other)
    {
        isGrounded = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject != player.gameObject)
        {
            Debug.Log(other.name);
            isGrounded = true;
        }
    }
}
