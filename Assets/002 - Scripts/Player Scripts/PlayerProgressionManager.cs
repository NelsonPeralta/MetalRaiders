using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressionManager : MonoBehaviour
{
    public static PlayerProgressionManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int baseXpGainPerMatch
    {
        get
        {
            int r = Random.Range(160, 240); // Reach Credits divided by 5
            return r;
        }
    }

    public Dictionary<int, int> playerLevelToXpDic = new Dictionary<int, int>()
    {
        {1, 1000 }, {2, 1250 }, {3, 1750 }, {4, 2000 }, {5, 2500 },

        {6, 4000 }, {7, 5000 }, {8, 7800 }, {9, 11100 }, {10, 14400 },

        {11, 21000 }, {12, 23300 }, {13, 25600 }, {14, 27900 }, {15, 32500 },

        {16, 37500 }, {17, 40000 }, {18, 48000 }, {19, 51000 }, {20, 54000 }
    };
}
