using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Announcer : MonoBehaviour
{
    Player player { get { return transform.root.GetComponent<Player>(); } }
    protected List<AudioClip> clips
    {
        get { return _clips; }
        set
        {
            Debug.Log(_clips.Count);
            int preCount = _clips.Count - 1;
            Debug.Log(preCount);
            _clips = value;

            if (_clips.Count > 0)
            {
                try
                {
                    StartCoroutine(PlayClip_Coroutine(_clipTimeRemaining, _clips.Last()));
                    _clipTimeRemaining += _clips.Last().length + 0.1f;
                    StartCoroutine(RemoveClip_Coroutine(_clipTimeRemaining * 0.95f, _clips.Last()));
                }
                catch { }

            }
        }
    }

    [SerializeField] List<AudioClip> _clips;
    [SerializeField] AudioClip gameOverClip;

    int _defaultDelay;
    float _clipTimeRemaining;

    private void Start()
    {
        _defaultDelay = 2;
        player.OnPlayerRespawned += OnPlayerRespwan;
    }

    private void Update()
    {
        if (_clipTimeRemaining > 0)
        {
            _clipTimeRemaining -= Time.deltaTime;
            if (_clipTimeRemaining <= 0)
                _clipTimeRemaining = 0;
        }
    }

    public void PlayGameOverClip()
    {
        CurrentRoomManager.instance.gameOver = true;


        if (clips.Count > 0)
        {
            List<AudioClip> c = new List<AudioClip>();
            c.Add(clips[0]);
            c.Add(gameOverClip);
            clips = c;
        }
        else
        {
            AddClip(gameOverClip);
        }
    }

    public void AddClip(AudioClip ac)
    {
        List<AudioClip> c = clips;
        c.Add(ac);
        clips = c;
    }

    IEnumerator PlayClip_Coroutine(float delay, AudioClip clip)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
        }
        catch (System.Exception ex) { Debug.Log(ex); }

    }

    IEnumerator RemoveClip_Coroutine(float delay, AudioClip clip)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            clips.Remove(clip);
        }
        catch (System.Exception ex) { Debug.Log(ex); }
    }

    void OnPlayerRespwan(Player player)
    {
        clips.Clear();
    }
}
