using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHitPointsWitness : MonoBehaviour
{
    [SerializeField] Player player;
    // Update is called once per frame
    void Update()
    {
        try
        {
            GetComponent<Text>().text = player.hitPoints.ToString();
        }
        catch { }
    }
}
