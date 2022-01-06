using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Announcer : MonoBehaviour
{
    public GameTime gametime;
    public float latestTimeStamp;
    public AudioSource announcerAudioSource;

    [Header("Multi Kill Settings")]
    public bool multikillInProgress;
    public float multiKillResetTimeDefault;
    public float multiKillResetTime;
    public int currentMultikill;

    [Header(" Double Kill")]
    public AudioClip doubleKillClip;
    public float doubleKillTimeStamp;
    public bool doubleKillHasTimeStamp;

    [Header("Triple Kill")]
    public AudioClip tripleKillClip;
    public float tripleKillTimeStamp;
    public bool tripleKillHasTimeStamp;

    [Header("Over Kill")]
    public AudioClip overKillClip;
    public float overKillTimeStamp;
    public bool overKillHasTimeStamp;

    [Header("Killtacular")]
    public AudioClip killtacularClip;
    public float killtacularTimeStamp;
    public bool killtacularHasTimeStamp;

    [Header("Killtrocity")]
    public AudioClip killtrocityClip;
    public float killtrocityTimeStamp;
    public bool killtrocityHasTimeStamp;

    [Header("Killamanjaro")]
    public AudioClip killamanjaroClip;
    public float killamanjaroTimeStamp;
    public bool killamanjaroHasTimeStamp;

    [Header("Killtastrophe")]
    public AudioClip killtastropheClip;
    public float killtastropheTimeStamp;
    public bool killtastropheHasTimeStamp;

    [Header("Killpocalypse")]
    public AudioClip killpocalypseClip;
    public float killpocalypseTimeStamp;
    public bool killpocalypseHasTimeStamp;

    [Header("Killionaire")]
    public AudioClip killionaireClip;
    public float killionaireTimeStamp;
    public bool killionaireHasTimeStamp;

    [Header("Other")]
    public AudioClip gameOverClip;


    private void Start()
    {
        multiKillResetTime = multiKillResetTimeDefault;
    }
    private void Update()
    {
        if (gametime != null)
        {
            ResetMultikill();
            CheckForTimeStamps();
        }
    }

    public void PlayGameOverClip()
    {
        announcerAudioSource.clip = gameOverClip;
        announcerAudioSource.Play();
    }

    public void AddToMultiKill()
    {
        if (gametime != null)
        {
            currentMultikill = currentMultikill + 1;
            multiKillResetTime = multiKillResetTimeDefault;

            if (currentMultikill >= 1)
            {
                multikillInProgress = true;
            }

            if (currentMultikill == 2 && !doubleKillHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Double Kill
                {
                    latestTimeStamp = gametime.totalGameTime;
                    doubleKillTimeStamp = gametime.totalGameTime;
                    doubleKillHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    doubleKillTimeStamp = latestTimeStamp;
                    doubleKillHasTimeStamp = true;
                }
            }

            if (currentMultikill == 3 && !tripleKillHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Triple Kill
                {
                    latestTimeStamp = gametime.totalGameTime;
                    tripleKillTimeStamp = gametime.totalGameTime;
                    tripleKillHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    tripleKillTimeStamp = latestTimeStamp;
                    tripleKillHasTimeStamp = true;
                }
            }

            if (currentMultikill == 4 && !overKillHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Triple Kill
                {
                    latestTimeStamp = gametime.totalGameTime;
                    overKillTimeStamp = gametime.totalGameTime;
                    overKillHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    overKillTimeStamp = latestTimeStamp;
                    overKillHasTimeStamp = true;
                }
            }

            if (currentMultikill == 5 && !killtacularHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killtacular
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killtacularTimeStamp = gametime.totalGameTime;
                    killtacularHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killtacularTimeStamp = latestTimeStamp;
                    killtacularHasTimeStamp = true;
                }
            }

            if (currentMultikill == 6 && !killtrocityHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killtrocity
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killtrocityTimeStamp = gametime.totalGameTime;
                    killtrocityHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killtrocityTimeStamp = latestTimeStamp;
                    killtrocityHasTimeStamp = true;
                }
            }

            if (currentMultikill == 7 && !killamanjaroHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killamanjaro
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killamanjaroTimeStamp = gametime.totalGameTime;
                    killamanjaroHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killamanjaroTimeStamp = latestTimeStamp;
                    killamanjaroHasTimeStamp = true;
                }
            }

            if (currentMultikill == 8 && !killtastropheHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killtastrophe
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killtastropheTimeStamp = gametime.totalGameTime;
                    killtastropheHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killtastropheTimeStamp = latestTimeStamp;
                    killtastropheHasTimeStamp = true;
                }
            }

            if (currentMultikill == 9 && !killpocalypseHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killpocalypse
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killpocalypseTimeStamp = gametime.totalGameTime;
                    killpocalypseHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killpocalypseTimeStamp = latestTimeStamp;
                    killpocalypseHasTimeStamp = true;
                }
            }

            if (currentMultikill == 10 && !killionaireHasTimeStamp)
            {
                if (!announcerAudioSource.isPlaying) // Killionnaire
                {
                    latestTimeStamp = gametime.totalGameTime;
                    killionaireTimeStamp = gametime.totalGameTime;
                    killionaireHasTimeStamp = true;
                }
                else
                {
                    latestTimeStamp = gametime.totalGameTime + 1.5f;
                    killionaireTimeStamp = latestTimeStamp;
                    killionaireHasTimeStamp = true;
                }
            }
        }
    }

    void CheckForTimeStamps()
    {
        if (doubleKillHasTimeStamp)
        {
            if (gametime.totalGameTime >= doubleKillTimeStamp)
            {
                announcerAudioSource.clip = doubleKillClip;
                announcerAudioSource.Play();
                doubleKillHasTimeStamp = false;
            }
        }

        if (tripleKillHasTimeStamp)
        {
            if (gametime.totalGameTime >= tripleKillTimeStamp)
            {
                announcerAudioSource.clip = tripleKillClip;
                announcerAudioSource.Play();
                tripleKillHasTimeStamp = false;
            }
        }

        if (overKillHasTimeStamp)
        {
            if (gametime.totalGameTime >= overKillTimeStamp)
            {
                announcerAudioSource.clip = overKillClip;
                announcerAudioSource.Play();
                overKillHasTimeStamp = false;
            }
        }

        if (killtacularHasTimeStamp)
        {
            if (gametime.totalGameTime >= killtacularTimeStamp)
            {
                announcerAudioSource.clip = killtacularClip;
                announcerAudioSource.Play();
                killtacularHasTimeStamp = false;
            }
        }

        if (killtrocityHasTimeStamp)
        {
            if (gametime.totalGameTime >= killtrocityTimeStamp)
            {
                announcerAudioSource.clip = killtrocityClip;
                announcerAudioSource.Play();
                killtrocityHasTimeStamp = false;
            }
        }

        if (killamanjaroHasTimeStamp)
        {
            if (gametime.totalGameTime >= killamanjaroTimeStamp)
            {
                announcerAudioSource.clip = killamanjaroClip;
                announcerAudioSource.Play();
                killamanjaroHasTimeStamp = false;
            }
        }

        if (killtastropheHasTimeStamp)
        {
            if (gametime.totalGameTime >= killtastropheTimeStamp)
            {
                announcerAudioSource.clip = killtastropheClip;
                announcerAudioSource.Play();
                killtastropheHasTimeStamp = false;
            }
        }

        if (killpocalypseHasTimeStamp)
        {
            if (gametime.totalGameTime >= killamanjaroTimeStamp)
            {
                announcerAudioSource.clip = killpocalypseClip;
                announcerAudioSource.Play();
                killpocalypseHasTimeStamp = false;
            }
        }

        if (killionaireHasTimeStamp)
        {
            if (gametime.totalGameTime >= killionaireTimeStamp)
            {
                announcerAudioSource.clip = killionaireClip;
                announcerAudioSource.Play();
                killionaireHasTimeStamp = false;
            }
        }
    }

    void ResetMultikill()
    {
        if (multikillInProgress)
        {
            multiKillResetTime -= Time.deltaTime;

            if (multiKillResetTime <= 0)
            {
                multiKillResetTime = multiKillResetTimeDefault;
                multikillInProgress = false;
                currentMultikill = 0;
            }
        }
    }

    IEnumerator PlayMultikillSound(int delay)
    {
        yield return new WaitForSeconds(delay);
    }
}
