using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRangeTrigger : MonoBehaviour
{
    public delegate void RangeEvent(AiRangeTrigger aiRangeCollider);
    public RangeEvent OnRangeTriggerEnter, OnRangeTriggerExit;
    public AiAbstractClass.PlayerRange range;

    public List<Player> playersInRange = new List<Player>();
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Player>() && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            if (other.GetComponent<Player>().isDead)
                return;
            playersInRange.Add(other.GetComponent<Player>());
            OnRangeTriggerEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            if (other.GetComponent<Player>().isDead)
                return;
            OnRangeTriggerExit?.Invoke(this);
            playersInRange.Remove(other.GetComponent<Player>());
        }
    }
}
