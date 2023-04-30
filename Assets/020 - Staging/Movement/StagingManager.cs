using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagingManager : MonoBehaviour
{
    [SerializeField] Transform _playerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
