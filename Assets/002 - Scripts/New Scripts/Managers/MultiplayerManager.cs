using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerManager : MonoBehaviour
{
    //public SplitScreenManager ssManager;
    public PlayerManager pManager;
    public bool FFA = false;
    public bool teamSlayer = false;

    [Header("FFA Scoring")]
    public int player0Score;
    public int player1Score;
    public int player2Score;
    public int player3Score;

    [Header("Team Slayer Scoring")]
    public int blueTeamScore;
    public int redTeamScore;

    [Header("Score Text (MANUAL LINKING")]
    public Text player0ScoreText1;
    public Text player0ScoreText2;
    public Text player1ScoreText1;
    public Text player1ScoreText2;
    public Text player2ScoreText1;
    public Text player2ScoreText2;
    public Text player3ScoreText1;
    public Text player3ScoreText2;

    public Text blueTeamScoreText;
    public Text redTeamScoreText;

    [Header("Spawns (NEEDS TO BE FILLED FOR RESPAWNS TO WORK")]
    public GameObject[] GenericSpawns = new GameObject[10];

    // Start is called before the first frame update
    void Start()
    {
        pManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<PlayerManager>();
        //ssManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<SplitScreenManager>();
        //ssManager.numberOfPlayers = numberOfPlayers;

        player0Score = 0;
        player1Score = 0;
        player2Score = 0;
        player3Score = 0;

        player0ScoreText1.text = player0Score.ToString();
        player0ScoreText2.text = player0Score.ToString();
        player1ScoreText1.text = player1Score.ToString();
        player1ScoreText2.text = player1Score.ToString();
        player2ScoreText1.text = player2Score.ToString();
        player2ScoreText2.text = player2Score.ToString();
        player3ScoreText1.text = player3Score.ToString();
        player3ScoreText2.text = player3Score.ToString();

        GivePlayersThis();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetTeams()
    {
        if (FFA)
        {
            PlayerManager pManager = GetComponent<PlayerManager>();
            //pManager.
        }
    }

    void GivePlayersThis()
    {
        foreach (GameObject player in pManager.allPlayers)
        {
            player.GetComponent<PlayerProperties>().multiplayerManager = this;
        }
    }

    public void AddToScore(int player)
    {
        if (FFA)
        {
            if (player == 0 && player0ScoreText1 != null)
            {
                player0Score = player0Score + 1;
                player0ScoreText1.text = player0Score.ToString();

            }
            else if (player == 1 && player1ScoreText1 != null)
            {
                player1Score = player1Score + 1;
                player1ScoreText1.text = player1Score.ToString();
            }
            else if (player == 2 && player2ScoreText1 != null)
            {
                player2Score = player2Score + 1;
                player2ScoreText1.text = player2Score.ToString();
            }
            else if (player == 3 && player3ScoreText1 != null)
            {
                player3Score = player3Score + 1;
                player3ScoreText1.text = player3Score.ToString();
            }

            ///
            ///   Update 2nd Text
            ///

            if (player0ScoreText2 != null)
            {
                int greaterEnnemiScorePlayer0 = Mathf.Max(player1Score, player2Score, player3Score);
                player0ScoreText2.text = greaterEnnemiScorePlayer0.ToString();
            }
            if (player1ScoreText2 != null)
            {
                int greaterEnnemiScorePlayer1 = Mathf.Max(player0Score, player2Score, player3Score);
                player1ScoreText2.text = greaterEnnemiScorePlayer1.ToString();
            }
            if (player2ScoreText2 != null)
            {
                int greaterEnnemiScorePlayer2 = Mathf.Max(player1Score, player0Score, player3Score);
                player2ScoreText2.text = greaterEnnemiScorePlayer2.ToString();
            }
            if (player3ScoreText2 != null)
            {
                int greaterEnnemiScorePlayer3 = Mathf.Max(player1Score, player2Score, player0Score);
                player3ScoreText2.text = greaterEnnemiScorePlayer3.ToString();
            }

        }
    }
}
