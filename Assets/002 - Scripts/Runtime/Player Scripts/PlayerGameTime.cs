using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameTime : MonoBehaviour
{
    [SerializeField] Text _timer;

    // Start is called before the first frame update
    void Start()
    {
        GameTime.instance.OnGameTimeRemainingChanged -= OnGameTimeChange;
        GameTime.instance.OnGameTimeRemainingChanged += OnGameTimeChange;
    }

    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeRemainingChanged -= OnGameTimeChange;
    }

    void OnGameTimeChange(GameTime gameTime)
    {
        //_timer.text = TimeSpan.FromSeconds(GameTime.instance.totalTime).ToString("mm:ss");
        _timer.text = $"{(GameTime.instance.timeRemaining / 60).ToString("00")}:{(GameTime.instance.timeRemaining % 60).ToString("00")}";
    }
}
