using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] protected List<AudioClip> _deathClips = new List<AudioClip>();
    [SerializeField] AudioSource _deathClipAudioSource, _collisionAudioSource;

    List<RagdollLimbCollisionDetection> _limbCollisionDetection = new List<RagdollLimbCollisionDetection>();
    float _timeSinceLastThud;
    int _ran;

    private void OnEnable()
    {
        _ran = Random.Range(0, _deathClips.Count);

        _deathClipAudioSource.clip = _deathClips[_ran];
        _deathClipAudioSource.Play();
    }
    private void Start()
    {
        List<Rigidbody> rb = new List<Rigidbody>();
        rb = GetComponentsInChildren<Rigidbody>().ToList();

        foreach (Rigidbody rb2 in rb)
        {
            rb2.gameObject.AddComponent<RagdollLimbCollisionDetection>();
            _limbCollisionDetection.Add(rb2.GetComponent<RagdollLimbCollisionDetection>());
            rb2.GetComponent<RagdollLimbCollisionDetection>().ragdoll = this;
        }
    }

    private void Update()
    {
        _timeSinceLastThud = Mathf.Clamp(_timeSinceLastThud + Time.deltaTime, 0, 1);

    }

    public void HandleCollision(Collision collision)
    {
        if (_timeSinceLastThud > 0.3f
            && collision.gameObject.transform.root != transform.root)
        {
            Debug.Log($"Ragdoll collision {collision.gameObject.name}");
            _timeSinceLastThud = 0;
            //AudioDirector.Instance.PlayPooledAudioClipAtPosition(PlayerDeathRagdollAudioSettings.ThudAudioClipDefinitions, this.transform.position);
            _collisionAudioSource.Play();
        }

    }
}
