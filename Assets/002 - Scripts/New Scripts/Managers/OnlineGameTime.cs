using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class OnlineGameTime : MonoBehaviour
{
    [Header("Singleton")]
    public static OnlineGameTime onlineGameTimeInstance;

    [Header("Info")]
    public PhotonView PV;
    public int totalTime = 0;
    public List<Text> playerTimerTexts;
    Coroutine timerCoroutine;
    ExitGames.Client.Photon.Hashtable timerCustomProperties = new ExitGames.Client.Photon.Hashtable();


    private void Awake()
    {
        if (onlineGameTimeInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        onlineGameTimeInstance = this;
    }

    private void Start()
    {
        timerCoroutine = StartCoroutine(Timer());
    }

    IEnumerator Timer(float delay = 1)
    {
        float newDelay = 1;
        yield return new WaitForSeconds(delay);
        if (PhotonNetwork.IsMasterClient)
        {
            totalTime++;
            if (timerCustomProperties.ContainsKey("totaltime"))
                timerCustomProperties.Remove("totaltime");
            timerCustomProperties.Add("totaltime", totalTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(timerCustomProperties);
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("totaltime"))
            {
                totalTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["totaltime"];
                Debug.Log($"Not Master Client Timer time: {totalTime}");
                newDelay = 0.1f;
            }
            else Debug.Log("No Custom Properties");
        }

        if (playerTimerTexts.Count > 0 && playerTimerTexts.Count > 0)
        {
            foreach (Text text in playerTimerTexts)
                text.text = $"{(totalTime / 60).ToString("00")}:{(totalTime % 60).ToString("00")}";
        }

        timerCoroutine = StartCoroutine(Timer(newDelay));
    }
}
