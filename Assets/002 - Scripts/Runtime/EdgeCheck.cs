using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeCheck : MonoBehaviour
{
    public Player player;
    public GameObject touch
    {
        get { return _touch; }
        private set
        {

        }
    }
    public Vector3 point { get { return _point; } }

    [SerializeField] GameObject _touch;

    Vector3 _point = Vector3.zero;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.root.gameObject != player.gameObject && !collision.gameObject.GetComponent<ManCannon>())
        {
            _point = collision.contacts[0].point;
            _touch = collision.gameObject;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        _point = Vector3.zero;
        _touch = null;
    }

    private void Update()
    {
        if (_touch != null && !_touch.activeSelf)
        {
            _point = Vector3.zero;
            _touch = null;
        }
    }
}
