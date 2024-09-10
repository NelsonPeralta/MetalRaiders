using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBarricade : Hazard
{
    [SerializeField] GameObject _model, _repairHolder;
    [SerializeField] int _hitpoints;

    [SerializeField] List<GameObject> _planks = new List<GameObject>();







    public int hitpoints { get { return _hitpoints; } }









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

            _repairHolder.SetActive(_hitpoints < 5);
        }
    }

    public void Repair()
    {
        if (_hitpoints < 5)
        {
            _hitpoints += 1;

            _model.SetActive(_hitpoints > 0);

            _planks[0].SetActive(_hitpoints > 0);
            _planks[1].SetActive(_hitpoints > 1);
            _planks[2].SetActive(_hitpoints > 2);
            _planks[3].SetActive(_hitpoints > 3);
            _planks[4].SetActive(_hitpoints > 4);

            _repairHolder.SetActive(_hitpoints < 5);
        }
    }



    private void Awake()
    {
        _hitpoints = 5;
        _repairHolder.SetActive(false);
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
