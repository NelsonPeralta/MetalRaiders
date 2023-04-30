using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGroundCheck : MonoBehaviour
{
    public delegate void GroundCheckEvent(NewGroundCheck groundCheck);
    public GroundCheckEvent OnGrounded;

    public Transform player;
    public bool isGrounded;

    public GameObject touch { get { return _touch; } }

    [SerializeField] GameObject _touch;

    private void Start()
    {
    }

    private void OnTriggerExit(Collider other)
    {
        _touch = null;
        isGrounded = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject != player.gameObject && !other.GetComponent<ManCannon>())
        {
            _touch = other.gameObject;
            isGrounded = true;
            OnGrounded?.Invoke(this);
        }
    }

    private void Update()
    {
        if (!_touch)
            isGrounded = false;
    }
}
