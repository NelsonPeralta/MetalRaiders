using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public delegate void GroundCheckEvent(GroundCheck groundCheck);
    public GroundCheckEvent OnGrounded;

    public Player player;
    public bool isGrounded;

    [SerializeField] GameObject _touch;

    private void Start()
    {
        OnGrounded += player.GetComponent<PlayerImpactReceiver>().OnGrounded_Event;
    }

    private void OnTriggerExit(Collider other)
    {
        _touch = null;
        isGrounded = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject != player.gameObject)
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
