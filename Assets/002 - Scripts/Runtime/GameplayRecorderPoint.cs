using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayRecorderPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.gameplayRecorderPoints.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
