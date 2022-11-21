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
                StartCoroutine(RemoveClip_Coroutine(preCount + 1 * _defaultDelay * 0.9f, _clips.Last()));
                StartCoroutine(PlayClip_Coroutine(preCount * _defaultDelay, _clips.Last()));
            }
        }
    }

    [SerializeField] List<AudioClip> _clips;
    [SerializeField] AudioClip gameOverClip;

    int _defaultDelay;

    private void Start()
    {
        _defaultDelay = 2;
        player.OnPlayerRespawned += OnPlayerRespwan;
    }

    public void PlayGameOverClip()
    {
        AddClip(gameOverClip);
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
        catch(System.Exception ex) { Debug.Log(ex); }

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
