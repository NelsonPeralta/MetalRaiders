using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBarricade : Hazard
{
    [SerializeField] GameObject _model;
    [SerializeField] int _hitpoints;







    public int hitpoints { get { return _hitpoints; }  }









    public void Damage()
    {
        print("ZombieBarracade Damage");
        if (_hitpoints > 0)
        {
            _hitpoints -= 1;

            if (_hitpoints == 0)
            {
                _model.SetActive(false);
            }
        }
    }



    private void Awake()
    {
        _hitpoints = 4;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentRoomManager.instance.spawnedMapAddOns++;
        GameManager.instance.hazards.Add(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
