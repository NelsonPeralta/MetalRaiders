using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldUIMarkerHolder : MonoBehaviour
{
    public delegate void PlayerWorldUIMarkerHolderEvent(PlayerWorldUIMarkerHolder playerWorldUIMarkerHolder);
    public PlayerWorldUIMarkerHolderEvent OnEnabledThis;

    private void OnEnable()
    {
        OnEnabledThis?.Invoke(this);
    }
}
