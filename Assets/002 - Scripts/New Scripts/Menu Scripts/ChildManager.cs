using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildManager : MonoBehaviour
{

    public List<GameObject> allChildren;
    public void GetChildRecursive(GameObject obj)
    {
        if (null == obj)
            return;
        foreach(Transform child in obj.transform)
        {
            if (null == child)
                continue;
            allChildren.Add(child.gameObject);
            GetChildRecursive(child.gameObject);
        }
    }

    private void Awake()
    {
        GetChildRecursive(gameObject);
    }

    public GameObject FindChildWithTag(string param1)
    {
        foreach(GameObject childWithTag in allChildren)
        {
            if (childWithTag.tag != null)
            {

                if (childWithTag.tag == param1)
                {

                    return childWithTag.gameObject;

                }
            }        
        }
        return null;
    }

    public GameObject FindChildWithTagScript(string param1)
    {

        foreach (GameObject child in allChildren)
        {
            if (child.GetComponent<Tags>() != null)
            {
                foreach (string tag in child.GetComponent<Tags>().tags)
                {
                    if (tag == param1)
                    {
                        return child.gameObject;
                    }
                }
            }
        }

        return null;
    }
}
