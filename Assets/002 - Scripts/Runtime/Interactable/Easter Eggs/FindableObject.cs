using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindableObject : MonoBehaviour
{
    public delegate void FindableObjectEvent(FindableObject findableObject);
    public FindableObjectEvent OnFound;

    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    [SerializeField] bool _found;
    public bool found
    {
        get { return _found; }
        private set
        {
            bool previousValue = _found;
            _found = value;

            if (value && !previousValue)
            {
                OnFound?.Invoke(this);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (found)
            return;
        if (other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Add(other.GetComponent<Player>());
            other.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
            other.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Remove(other.GetComponent<Player>());
        }
    }

    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (!found)
        {
            playersInRange.Remove(playerController.GetComponent<Player>());

            found = true;
        }
    }
}
