using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerWorldUIMarker : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] int _controllerTarget;
    [SerializeField] Player _targetPlayer;

    [SerializeField] GameObject _holder;
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _redMarker;
    [SerializeField] GameObject _greenMarker;

    int damping = 1;

    private void Start()
    {
        StartCoroutine(LateStart());
    }

    private void Update()
    {
        if (!_targetPlayer)
            return;

        Vector3 targetPostition = new Vector3(_targetPlayer.transform.position.x,
                                        this.transform.position.y,
                                        _targetPlayer.transform.position.z);
        this.transform.LookAt(targetPostition);
    }

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        try
        {
            _targetPlayer = GameManager.instance.localPlayers[_controllerTarget];
            _text.text = _player.nickName;

            _targetPlayer.OnPlayerDeath -= OnPlayerDeath;
            _targetPlayer.OnPlayerDeath += OnPlayerDeath;
        }
        catch (System.Exception e) { Debug.LogWarning(e); gameObject.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            if (other.GetComponent<Player>() == _targetPlayer)
            {
                _holder.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
        }
    }

    private void OnTriggerStay(Collider other)
    {
        try
        {
            if (other.GetComponent<Player>() == _targetPlayer)
            {
                _holder.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    void OnPlayerDeath(Player player)
    {
        _holder.SetActive(false);
    }
}
