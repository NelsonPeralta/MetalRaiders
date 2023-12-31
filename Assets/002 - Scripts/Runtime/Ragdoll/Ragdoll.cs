using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

abstract public class Ragdoll : MonoBehaviour
{
    public Transform head;
    public Transform hips;

    [SerializeField] protected List<AudioClip> _deathClips = new List<AudioClip>();
    [SerializeField] protected List<AudioClip> _collisionClips = new List<AudioClip>();
    [SerializeField] AudioSource _deathClipAudioSource, _collisionAudioSource;

    [SerializeField] protected List<RagdollLimbCollisionDetection> _limbCollisionDetection = new List<RagdollLimbCollisionDetection>();
    [SerializeField] float _timeSinceLastThud;
    int _ran;

    private void OnEnable()
    {
        _ran = Random.Range(0, _deathClips.Count);

        _deathClipAudioSource.clip = _deathClips[_ran];
        _deathClipAudioSource.Play();
    }

    private void Awake()
    {
        foreach (RagdollLimbCollisionDetection rb2 in GetComponentsInChildren<RagdollLimbCollisionDetection>().ToList())
        {
            _limbCollisionDetection.Add(rb2.GetComponent<RagdollLimbCollisionDetection>());
            rb2.GetComponent<RagdollLimbCollisionDetection>().ragdoll = this;
        }
    }
    private void Update()
    {
        _timeSinceLastThud = Mathf.Clamp(_timeSinceLastThud + Time.deltaTime, 0, 1);
        ChildUpdate();
    }

    public void HandleCollision(Collision collision)
    {
        if (_timeSinceLastThud > 0.3f
            && collision.gameObject.transform.root != transform.root)
        {
            Debug.Log($"Ragdoll collision {collision.gameObject.name}");
            GameObjectPool.instance.SpawnWeaponSmokeCollisionObject(hips.transform.position);

            _timeSinceLastThud = 0;

            if (!collision.gameObject.GetComponent<PlayerCapsule>() && !collision.gameObject.GetComponent<Player>())
                if (!_deathClipAudioSource.isPlaying)
                {
                    _ran = Random.Range(0, _collisionClips.Count);
                    _collisionAudioSource.clip = _collisionClips[_ran];

                    _collisionAudioSource.Play();
                }
        }

    }



    public virtual void ChildUpdate() { }
}
