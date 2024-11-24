using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOrEnableDependingOnGameMode : MonoBehaviour
{

    [SerializeField] GameManager.GameMode _gameMode;



    private void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        transform.GetChild(0).gameObject.SetActive(GameManager.instance.gameMode == _gameMode);
    }
}
