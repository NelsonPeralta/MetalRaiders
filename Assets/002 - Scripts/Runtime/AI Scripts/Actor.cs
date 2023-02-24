using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Actor : MonoBehaviour
{
    public Transform target { get { return _target; } set { _target = value; } }
    public Vector3 destination { get { return _destination; } set { _destination = value; } }
    public Transform losSpawn { get { return _losSpawn; } set { _losSpawn = value; } }
    public virtual FieldOfView fieldOfView { get { return _fieldOfView; } private set { _fieldOfView = value; } }
    public NavMeshAgent nma { get { return _nma; } private set { _nma = value; } }

    public int longRange { get { return _longRange; } }
    public int midRange { get { return _midRange; } }
    public int closeRange { get { return _closeRange; } }


    [SerializeField] Transform _target;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;
    [SerializeField] FieldOfView _fieldOfView;
    [SerializeField] NavMeshAgent _nma;
    [SerializeField] protected Animator _animator;

    [SerializeField] int _closeRange, _midRange, _longRange;

    [SerializeField] float _analyzeNextActionCooldown;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _analyzeNextActionCooldown = 0.5f;

        _fieldOfView = GetComponent<FieldOfView>();
        _nma = GetComponent<NavMeshAgent>();

        if (_closeRange <= 0)
            _closeRange = 2;

        if (_midRange <= 0)
            _midRange = 12;

        if (_longRange <= 0)
            _longRange = 20;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_analyzeNextActionCooldown > 0)
        {
            _analyzeNextActionCooldown -= Time.deltaTime;

            if (_analyzeNextActionCooldown <= 0)
            {
                AnalyzeNextAction();
                _analyzeNextActionCooldown = 0.5f;
            }
        }
    }

    private void OnEnable()
    {

    }




    public abstract void AnalyzeNextAction();
}
