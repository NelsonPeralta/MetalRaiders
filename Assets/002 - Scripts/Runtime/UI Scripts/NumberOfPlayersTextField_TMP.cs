using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumberOfPlayersTextField_TMP : MonoBehaviour
{
    delegate void NumberOfPlayersTextField_TMP_Event();

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_InputField>().text = GameManager.instance.NbPlayers.ToString();
        GetComponent<TMP_InputField>().onValueChanged.AddListener(GameManager.instance.ChangeNbLocalPlayers);
    }
}
