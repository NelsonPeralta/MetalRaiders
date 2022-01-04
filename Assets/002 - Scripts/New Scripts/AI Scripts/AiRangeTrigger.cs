using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRangeTrigger : MonoBehaviour
{
    public delegate void RangeEvent(AiRangeTrigger aiRangeCollider);
    public RangeEvent OnRangeTriggerEnter, OnRangeTriggerExit;
    public AiAbstractClass.PlayerRange range;

    public List<PlayerProperties> playersInRange = new List<PlayerProperties>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerProperties>() && !playersInRange.Contains(other.GetComponent<PlayerProperties>()))
        {
            playersInRange.Add(other.GetComponent<PlayerProperties>());
            OnRangeTriggerEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerProperties>() && playersInRange.Contains(other.GetComponent<PlayerProperties>()))
        {

            playersInRange.Remove(other.GetComponent<PlayerProperties>());
            OnRangeTriggerExit?.Invoke(this);
        }
    }
}
