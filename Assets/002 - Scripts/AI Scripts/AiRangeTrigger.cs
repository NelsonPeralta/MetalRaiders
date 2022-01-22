using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRangeTrigger : MonoBehaviour
{
    public delegate void RangeEvent(AiRangeTrigger aiRangeCollider);
    public RangeEvent OnRangeTriggerEnter, OnRangeTriggerExit;
    public AiAbstractClass.PlayerRange range;

    public List<Player> playersInRange = new List<Player>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Add(other.GetComponent<Player>());
            OnRangeTriggerEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {

            OnRangeTriggerExit?.Invoke(this);
            playersInRange.Remove(other.GetComponent<Player>());
        }
    }
}
