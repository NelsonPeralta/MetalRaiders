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
        GameTime.instance.OnGameTimeChanged += OnGameTimeChange;
    }

    void OnGameTimeChange(GameTime gameTime)
    {
        _timer.text = $"{(GameTime.instance.totalTime / 60).ToString("00")}:{(GameTime.instance.totalTime % 60).ToString("00")}";
    }
}
