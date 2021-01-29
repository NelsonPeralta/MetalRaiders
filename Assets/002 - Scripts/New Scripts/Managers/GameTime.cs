using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour
{
    [Header("Info")]
    public int minutes;
    public int seconds;
    public float totalGameTime;

    [Header("MANUAL LINKING")]
    public Text minutesTextPlayer1;
    public Text minutesTextPlayer2;
    public Text minutesTextPlayer3;
    public Text minutesTextPlayer4;
    public Text secondsTexPlayer1;
    public Text secondsTexPlayer2;
    public Text secondsTexPlayer3;
    public Text secondsTexPlayer4;

    // Start is called before the first frame update
    void Start()
    {
        if (minutesTextPlayer1 != null)
        {
            minutesTextPlayer1.text = "00";
            secondsTexPlayer1.text = "00";
        }

        if (minutesTextPlayer2 != null)
        {
            minutesTextPlayer2.text = "00";
            secondsTexPlayer2.text = "00";
        }

        if (minutesTextPlayer3 != null)
        {
            minutesTextPlayer3.text = "00";
            secondsTexPlayer3.text = "00";
        }

        if (minutesTextPlayer4 != null)
        {
            minutesTextPlayer4.text = "00";
            secondsTexPlayer4.text = "00";
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        totalGameTime += Time.deltaTime;

        minutes = Mathf.RoundToInt(totalGameTime) / 60;
        seconds = Mathf.RoundToInt(totalGameTime) % 60;

        if (minutes >= 10)
        {
            if (minutesTextPlayer1 != null)
            {
                minutesTextPlayer1.text = minutes.ToString();
            }

            if (minutesTextPlayer2 != null)
            {
                minutesTextPlayer2.text = minutes.ToString();
            }

            if (minutesTextPlayer3 != null)
            {
                minutesTextPlayer3.text = minutes.ToString();
            }

            if (minutesTextPlayer4 != null)
            {
                minutesTextPlayer4.text = minutes.ToString();
            }
        }
        else if (minutes < 10)
        {
            if (minutesTextPlayer1 != null)
            {
                minutesTextPlayer1.text = "0" + minutes.ToString();
            }

            if (minutesTextPlayer2 != null)
            {
                minutesTextPlayer2.text = "0" + minutes.ToString();
            }

            if (minutesTextPlayer3 != null)
            {
                minutesTextPlayer3.text = "0" + minutes.ToString();
            }

            if (minutesTextPlayer4 != null)
            {
                minutesTextPlayer4.text = "0" + minutes.ToString();
            }
        }

        if (seconds >= 10)
        {
            if (minutesTextPlayer1 != null)
            {
                secondsTexPlayer1.text = seconds.ToString();
            }

            if (minutesTextPlayer2 != null)
            {
                secondsTexPlayer2.text = seconds.ToString();
            }

            if (minutesTextPlayer3 != null)
            {
                secondsTexPlayer3.text = seconds.ToString();
            }

            if (minutesTextPlayer4 != null)
            {
                secondsTexPlayer4.text = seconds.ToString();
            }
        }
        else if (seconds < 10)
        {
            if (minutesTextPlayer1 != null)
            {
                secondsTexPlayer1.text = "0" + seconds.ToString();
            }

            if (minutesTextPlayer2 != null)
            {
                secondsTexPlayer2.text = "0" + seconds.ToString();
            }

            if (minutesTextPlayer3 != null)
            {
                secondsTexPlayer3.text = "0" + seconds.ToString();
            }

            if (minutesTextPlayer4 != null)
            {
                secondsTexPlayer4.text = "0" + seconds.ToString();
            }
        }
    }
}
