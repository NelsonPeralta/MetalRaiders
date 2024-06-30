using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressionManager : MonoBehaviour
{
    public static PlayerProgressionManager instance;
    public static int xpGainPerMatch
    {
        get
        {
            int r = UnityEngine.Random.Range(170, 230); // 500 games

            return r;
        }
    }

    public static int honorGainPerMatch // 500 games
    {
        get
        {
            if (CurrentRoomManager.instance.nbPlayersJoined == 1) return 0;
            return 2;
        }
    }
    public List<Sprite> rankSprites { get { return _rankImages; } }
    public List<Rank> ranks { get { return _ranks; } }

    [SerializeField] List<Sprite> _rankImages;

    List<Rank> _ranks = new List<Rank>();


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


            _ranks.Add(new Rank(0, "r", "Recruit", "invisible"));

            _ranks.Add(new Rank(10, "pvt", "Private", "brown"));
            _ranks.Add(new Rank(20, "cpl", "Corporal", "brown"));

            _ranks.Add(new Rank(40, "mcpl", "Master Corporal", "brown"));
            _ranks.Add(new Rank(80, "sgt", "Sergeant", "brown"));

            _ranks.Add(new Rank(120, "wo", "Warrant Officer", "black"));
            _ranks.Add(new Rank(160, "mwo", "Master Warrant Officer", "black"));
            _ranks.Add(new Rank(200, "cwo", "Chief Warrant Officer", "black"));

            _ranks.Add(new Rank(250, "2lt", "Second Lieutenant", "white"));
            _ranks.Add(new Rank(300, "lt", "Lieutenant", "white"));
            _ranks.Add(new Rank(350, "capt", "Captain", "white"));

            _ranks.Add(new Rank(400, "maj", "Major", "lightblue"));
            _ranks.Add(new Rank(500, "cmd", "Commander", "lightblue"));
            _ranks.Add(new Rank(600, "col", "Colonel", "lightblue"));

            _ranks.Add(new Rank(800, "bg", "Brigadier", "lightyellow"));
            _ranks.Add(new Rank(1000, "gen", "General", "lightyellow"));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    public static Rank[] GetClosestAndNextRank(int h)
    {
        Rank[] r = new Rank[2];
        for (int i = 0; i < instance._ranks.Count; i++)
        {
            //Debug.Log($"Rank search: {xp} {h} \n {rank.lvlRequired} {rank.honorRequired}");
            if (h >= instance._ranks[i].honorRequired)
            {
                r[0] = instance._ranks[i];
                try { r[1] = instance._ranks[i + 1]; } catch { }
            }
            else
                break;
        }

        return r;
    }

    public static int GetMinHonorForRank(string na)
    {
        int m = -1;

        for (int i = 0; i < instance._ranks.Count; i++)
            if (instance._ranks[i].cleanName.Equals(na))
                m = instance._ranks[i].honorRequired;

        if (m < 0)
        {
            m = 99999999;
            Debug.LogError("THAT RANK DOES NOT EXIST");
        }

        return m;
    }

    public static Dictionary<int, int> playerLevelToXpDic = new Dictionary<int, int>()
    {
        //{1, 0 }, {2, 500 }, {3, 1000 }, {4, 1500 }, {5, 2500 }, // 500; diff 1000

        //{6, 3000 }, {7, 3500 }, {8, 4000 }, {9, 4500 }, {10, 5500 }, // 500 ; d 1000

        //{11, 6250 }, {12, 7000 }, {13, 7750 }, {14, 8500 }, {15, 10000 }, // 750; d1500

        //{16, 11000 }, {17, 12000 }, {18, 13000 }, {19, 14000 }, {20, 16000 }, // 1000 d2000

        //{21, 17500 }, {22, 19000 }, {23, 20500 }, {24, 22000 }, {25, 24000 }, // 1500 d2000

        //{26, 25500 }, {27, 27000 }, {28, 28500 }, {29, 30000 }, {30, 32500 }, // 1500 d2500

        //{31, 34500 }, {32, 36500 }, {33, 38500 }, {34, 40500 }, {35, 43000 }, // 2000 d2500

        //{36, 45000 }, {37, 47000 }, {38, 49000 }, {39, 51000 }, {40, 54000 }, // 2000 d3000

        //{41, 57000 }, {42, 60000 }, {43, 63000 }, {44, 66000 }, {45, 70000 }, // 3000 d4000

        //{46, 74000 }, {47, 78000 }, {48, 82000 }, {49, 86000 }, {50, 100000 } // 4000 d10000



        {1, 0 }, {2, 600 }, {3, 1200 }, {4, 1800 }, {5, 2600 }, // 600; diff 800

        {6, 3300 }, {7, 4000 }, {8, 4700 }, {9, 5400 }, {10, 6350 }, // 700 ; d 950

        {11, 7200 }, {12, 8050 }, {13, 8900 }, {14, 9750 }, {15, 10850 }, // 850; d1100

        {16, 11900 }, {17, 12950 }, {18, 14000 }, {19, 15050 }, {20, 16400 }, // 1050 d1350

        {21, 17700 }, {22, 19000 }, {23, 20300 }, {24, 21600 }, {25, 23250 }, // 1300 d1650

        {26, 24850 }, {27, 26450 }, {28, 28050 }, {29, 29650 }, {30, 31650 }, // 1600 d2000

        {31, 33650 }, {32, 35650 }, {33, 37650 }, {34, 39650 }, {35, 42100 }, // 2000 d2450

        {36, 44600 }, {37, 47100 }, {38, 49600 }, {39, 52100 }, {40, 55100 }, // 2500 d3000

        {41, 58200 }, {42, 61300 }, {43, 64400 }, {44, 67500 }, {45, 71200 }, // 3100 d3700

        {46, 75900 }, {47, 80600 }, {48, 85300 }, {49, 90000 }, {50, 100000 } // 4700
    };


















    public struct Rank
    {

        public int honorRequired { get { return _honorRequired; } }
        public string spriteName { get { return _spriteName; } }
        public string cleanName { get { return _cleanName; } }
        public string color { get { return _color; } }



        int _honorRequired;
        string _spriteName, _cleanName, _color;


        public Rank(int hReq, string n, string cn, string color)
        {
            _honorRequired = hReq;
            _spriteName = n;
            _cleanName = cn;
            _color = color;
        }
    }
}
