using UnityEngine;
using System.Collections;

public class MouseDebug : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Log.Print(() => hit.collider.name);
        }

        OnMouseOver();
    }

    void OnMouseOver()
    {
        Log.Print(() => gameObject.name);
    }
}
