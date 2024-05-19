using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    public List<Player> collidingPlayers
    {
        get { return _collidingPlayers; }
    }

    [SerializeField] List<Player> _collidingPlayers = new List<Player>();

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.activeSelf || !other.gameObject.activeInHierarchy)
            return;

        if (other.GetComponent<Player>())
        {
            Player p = other.GetComponent<Player>();
            if (!_collidingPlayers.Contains(p))
                _collidingPlayers.Add(p);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            _collidingPlayers.Remove(other.GetComponent<Player>());
            other.GetComponent<PlayerUI>().HideInformer();

        }
        catch { }
    }

    private void Update()
    {
        if (_collidingPlayers.Count > 0)
            for (int i = 0; i < _collidingPlayers.Count; i++)
                if (!_collidingPlayers[i].gameObject.activeSelf || !_collidingPlayers[i].gameObject.activeInHierarchy)
                {
                    try
                    {
                        _collidingPlayers[i].GetComponent<PlayerUI>().HideInformer();
                        _collidingPlayers.Remove(_collidingPlayers[i]);
                    }
                    catch { }
                }
    }
}
