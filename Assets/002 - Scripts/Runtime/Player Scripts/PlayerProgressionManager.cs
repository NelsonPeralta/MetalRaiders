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

    public static Dictionary<int, int> playerLevelToXpDic = new Dictionary<int, int>()
    {
        {1, 1000 }, {2, 1250 }, {3, 1750 }, {4, 2500 }, {5, 5000 },

        {6, 6000 }, {7, 7000 }, {8, 10000 }, {9, 13000 }, {10, 20000 },

        {11, 21000 }, {12, 21250 }, {13, 21750 }, {14, 22500 }, {15, 25000 },

        {16, 26000 }, {17, 27000 }, {18, 27500 }, {19, 28500 }, {20, 40000 },

        {21, 41000 }, {22, 41250 }, {23, 41750 }, {24, 42500 }, {25, 50000 },

        {26, 56000 }, {27, 57000 }, {28, 57500 }, {29, 58500 }, {30, 60000 },

        {31, 61000 }, {32, 61250 }, {33, 61750 }, {34, 62500 }, {35, 75000 },

        {36, 76000 }, {37, 77000 }, {38, 77500 }, {39, 78500 }, {40, 80000 },

        {41, 81000 }, {42, 81250 }, {43, 81750 }, {44, 82500 }, {45, 85000 },

        {46, 86000 }, {47, 87000 }, {48, 87500 }, {49, 88500 }, {50, 100000 }
    };
}
