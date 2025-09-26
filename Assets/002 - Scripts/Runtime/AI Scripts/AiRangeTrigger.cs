using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRangeTrigger : MonoBehaviour
{
    public AiAbstractClass AiAbstractClass;
    public delegate void RangeEvent(AiRangeTrigger aiRangeCollider, Collider triggerObj = null);
    public RangeEvent OnRangeTriggerEnter, OnRangeTriggerExit;
    public AiAbstractClass.PlayerRange range;

    public List<Player> playersInRange = new List<Player>();
    private void OnTriggerStay(Collider other)
    {
        //try
        //{
        //    if (other.GetComponent<Transform>() == AiAbstractClass.targetPlayer)
        //    {
        //        //Log.Print(() =>"Arrived to empty target");
        //        Log.Print(() =>gameObject);
        //        OnRangeTriggerEnter?.Invoke(this, other);
        //    }
        //}
        //catch { }
        if (other.GetComponent<Player>() && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            if (other.GetComponent<Player>().isDead)
                return;
            playersInRange.Add(other.GetComponent<Player>());
            OnRangeTriggerEnter?.Invoke(this, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Transform>() == AiAbstractClass.targetPlayer)
        {
            OnRangeTriggerExit?.Invoke(this);
        }
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            if (other.GetComponent<Player>().isDead)
                return;
            OnRangeTriggerExit?.Invoke(this, other);
            playersInRange.Remove(other.GetComponent<Player>());
        }
    }
}
