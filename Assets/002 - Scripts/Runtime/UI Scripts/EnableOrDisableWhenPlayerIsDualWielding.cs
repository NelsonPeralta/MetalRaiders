using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOrDisableWhenPlayerIsDualWielding : MonoBehaviour
{
    [SerializeField] PlayerInventory _playerInventory;
    [SerializeField] bool _show;
    [SerializeField] GameObject _target;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _target.SetActive(_playerInventory.isDualWielding && _show);
    }
}
