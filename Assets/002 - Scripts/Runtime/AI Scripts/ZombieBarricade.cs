using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBarricade : Hazard
{
    [SerializeField] GameObject _model;
    [SerializeField] int _hitpoints;

    [SerializeField] List<GameObject> _planks = new List<GameObject>();







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

            _planks[0].SetActive(_hitpoints > 0);
            _planks[1].SetActive(_hitpoints > 1);
            _planks[2].SetActive(_hitpoints > 2);
            _planks[3].SetActive(_hitpoints > 3);
            _planks[4].SetActive(_hitpoints > 4);
        }
    }



    private void Awake()
    {
        _hitpoints = 5;
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
