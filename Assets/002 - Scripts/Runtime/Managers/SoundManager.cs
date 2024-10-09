using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;




    public AudioClip concretePunchHit, successfulPunch, failedPunch, successfulSwordSlash;
    public AudioClip weaponCollision;
    public AudioClip ragdollCollision;




    [SerializeField] AudioSource _prefab;
    [SerializeField] List<AudioSource> _audioSources = new List<AudioSource>();






    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            AudioSource obj = Instantiate(_prefab, transform.position, transform.rotation);
            obj.gameObject.SetActive(false);
            _audioSources.Add(obj);
            obj.transform.parent = gameObject.transform;
        }
    }


    public void PlayAudioClip(Vector3 pos, AudioClip ac)
    {
        foreach (AudioSource obj in _audioSources)
            if (!obj.gameObject.activeSelf)
            {
                obj.transform.position = pos;
                obj.GetComponent<AudioSource>().clip = ac;
                obj.gameObject.SetActive(true);

                StartCoroutine(DisableObjectAfterTime(obj.gameObject));
                break;
            }
    }



    public IEnumerator DisableObjectAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(2);
        obj.SetActive(false);
    }
}
