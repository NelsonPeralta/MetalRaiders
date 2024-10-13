using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxDetector : MonoBehaviour
{
    public List<GameObject> collidingHitboxes
    {
        get { return _collidingHitboxes; }
    }


    [SerializeField] Player player;
    [SerializeField] List<GameObject> _collidingHitboxes;


    [SerializeField] GameObject _collidingHitbox;


    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.activeSelf || !other.gameObject.activeInHierarchy)
            return;

        if (other.GetComponent<Hitbox>())
            if (!_collidingHitboxes.Contains(other.gameObject) && other.gameObject.transform.root != player.transform)
                _collidingHitboxes.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_collidingHitboxes.Contains(other.gameObject))
            _collidingHitboxes.Remove(other.gameObject);
    }

    private void Update()
    {
        if (_collidingHitboxes.Count > 0)
        {

            //for (int i = 0; i < _collidingHitboxes.Count; i++)
            //    if (!_collidingHitboxes[i].gameObject.activeSelf || !_collidingHitboxes[i].gameObject.activeInHierarchy)
            //        _collidingHitboxes.Remove(_collidingHitboxes[i]);


            for (int i = _collidingHitboxes.Count; i-- > 0;)
            {
                if (collidingHitboxes[i] == null) // if a player leaves while in the list this WILL cause errors for the code below
                {
                    collidingHitboxes.Remove(collidingHitboxes[i]);
                }
                else
                {
                    if (!collidingHitboxes[i].gameObject.activeSelf || !collidingHitboxes[i].gameObject.activeInHierarchy)
                        collidingHitboxes.Remove(collidingHitboxes[i]);
                }
            }
        }
    }
}
