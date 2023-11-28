using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Hitboxes : MonoBehaviour
{
    public List<Hitbox> hitboxes { get { return _hitboxes; } }
    [SerializeField] protected List<Hitbox> _hitboxes = null;

    private void Awake()
    {
        _hitboxes = GetComponentsInChildren<Hitbox>().ToList();
        foreach (Hitbox hb in _hitboxes)
            hb.hitboxesScript = this;
    }
}
