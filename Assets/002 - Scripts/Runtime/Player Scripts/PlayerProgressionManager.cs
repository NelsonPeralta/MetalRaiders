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
            int r = UnityEngine.Random.Range(160, 240); // Reach Credits divided by 5

            DateTime today = DateTime.Now;
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
                r *= 2;

            if (CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private)
                r = 0;
            return r;
        }
    }

    public static int honorGainPerMatch
    {
        get
        {
            int h = 1;

            DateTime today = DateTime.Now;
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
                h *= 2;

            if (CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private)
                h = 0;
            return h;
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

            _ranks.Add(new Rank(0, 0, "r", "Recruit", "invisible"));

            _ranks.Add(new Rank(5, 5, "pvt", "Private", "brown"));
            _ranks.Add(new Rank(10, 10, "cpl", "Corporal", "brown"));
            _ranks.Add(new Rank(10, 15, "mcpl", "Master Corporal", "brown"));
            _ranks.Add(new Rank(15, 20, "sgt", "Sergeant", "brown"));

            _ranks.Add(new Rank(20, 30, "wo", "Warrant Officer", "black"));
            _ranks.Add(new Rank(22, 40, "mwo", "Master Warrant Officer", "black"));
            _ranks.Add(new Rank(25, 50, "cwo", "Chief Warrant Officer", "black"));

            _ranks.Add(new Rank(30, 60, "2lt", "Second Lieutenant", "white"));
            _ranks.Add(new Rank(32, 65, "lt", "Lieutenant", "white"));
            _ranks.Add(new Rank(35, 75, "capt", "Captain", "white"));

            _ranks.Add(new Rank(40, 100, "maj", "Major", "lightblue"));
            _ranks.Add(new Rank(42, 125, "cmdt", "Commander", "lightblue"));
            _ranks.Add(new Rank(45, 150, "col", "Colonel", "lightblue"));

            _ranks.Add(new Rank(45, 200, "bg", "Brigadier", "lightyellow"));
            _ranks.Add(new Rank(50, 250, "gen", "General", "lightyellow"));

        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    public static Rank GetClosestRank(int xp, int h)
    {
        Rank r = instance._ranks[0];
        foreach (Rank rank in instance._ranks)
        {
            //Debug.Log($"Rank search: {xp} {h} \n {rank.lvlRequired} {rank.honorRequired}");
            if (xp >= rank.lvlRequired && h >= rank.honorRequired)
                r = rank;
            else
                break;
        }

        return r;
    }

    public static Dictionary<int, int> playerLevelToXpDic = new Dictionary<int, int>()
    {
        {1, 0 }, {2, 1000 }, {3, 1750 }, {4, 2500 }, {5, 5000 },

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


















    public struct Rank
    {

        public int lvlRequired { get { return _lvlRequired; } }
        public int honorRequired { get { return _honorRequired; } }
        public string spriteName { get { return _spriteName; } }
        public string cleanName { get { return _cleanName; } }
        public string color { get { return _color; } }



        int _lvlRequired, _honorRequired;
        string _spriteName, _cleanName, _color;


        public Rank(int xpReq, int hReq, string n, string cn, string color)
        {
            _lvlRequired = xpReq;
            _honorRequired = hReq;
            _spriteName = n;
            _cleanName = cn;
            _color = color;
        }
    }
}
