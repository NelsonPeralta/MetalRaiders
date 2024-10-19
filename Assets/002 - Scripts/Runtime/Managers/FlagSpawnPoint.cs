using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagSpawnPoint : MonoBehaviour
{
    public GameManager.Team team;


    [SerializeField] Flag _flag;
    [SerializeField] GameObject _canvasHolder;


    private void Awake()
    {
        if (GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            _flag.spawnPoint = this;
            _flag.scriptRoot.parent = null;

            _canvasHolder.SetActive(true);
        }
        else
            Destroy(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.CTF)
            SpawnFlagAtStand();
        else _flag.scriptRoot.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SpawnFlagAtStand()
    {
        print("SpawnFlag");
        StartCoroutine(SpawnFlagAtStand_Coroutine());
    }

    IEnumerator SpawnFlagAtStand_Coroutine()
    {
        _flag.ChangeState(Flag.State.atbase);
        _flag.scriptRoot.gameObject.SetActive(false);
        _flag.rb.velocity = Vector3.zero;
        _flag.rb.angularVelocity = Vector3.zero;
        _flag.rb.mass = 999;

        _flag.transform.root.rotation = transform.rotation;
        _flag.transform.root.position = transform.position + (Vector3.up * 1.5f);

        yield return new WaitForSeconds(1);

        print("SpawnFlag_Coroutine");
        _flag.scriptRoot.gameObject.SetActive(true);
    }
}
