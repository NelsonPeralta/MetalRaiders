using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public delegate void GroundCheckEvent(GroundCheck groundCheck);
    public GroundCheckEvent OnGrounded;

    public Player player;
    public bool isGrounded;

    private void Start()
    {
        OnGrounded += player.GetComponent<PlayerImpactReceiver>().OnGrounded_Event;
    }

    private void OnTriggerExit(Collider other)
    {
        isGrounded = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject != player.gameObject)
        {
            isGrounded = true;
            OnGrounded?.Invoke(this);
        }
    }
}
