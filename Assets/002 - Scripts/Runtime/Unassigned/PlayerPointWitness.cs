using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPointWitness : MonoBehaviour
{
    public TMP_Text _text;
    float countdown;
    int tempPoints;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (countdown > 0)
        {
            countdown -= Time.deltaTime;

            if (countdown < 0)
            {
                tempPoints = 0;
                _text.gameObject.SetActive(false);
            }
        }
    }



    public void Add(int p)
    {
        countdown = PlayerMedals.MEDAL_TTL;
        tempPoints += p;
        _text.text = $"+{tempPoints}";
        _text.gameObject.SetActive(true);
    }
}
