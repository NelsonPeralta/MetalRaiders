using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerThirdPersonModel : MonoBehaviour
{
    [SerializeField] Transform _playerCapsuleFeet;

    Vector3 _vect;
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            transform.position = new Vector3(_playerCapsuleFeet.transform.position.x, _playerCapsuleFeet.transform.position.y, _playerCapsuleFeet.transform.position.z);
        }
        catch { }
    }
}
