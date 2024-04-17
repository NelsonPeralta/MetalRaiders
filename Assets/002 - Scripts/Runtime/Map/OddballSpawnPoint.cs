using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class OddballSpawnPoint : MonoBehaviour
{
    [SerializeField] OddballSkull _oddball;

    private void Awake()
    {
        _oddball.thisRoot.parent = null;
    }

    private void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Oddball)
            SpawnOddball();
        else _oddball.DisableOddball();
    }

    public void SpawnOddball()
    {
        print("SpawnOddball");
        StartCoroutine(SpawnOddball_Coroutine());
    }


    IEnumerator SpawnOddball_Coroutine()
    {
        _oddball.DisableOddball();
        _oddball.rb.velocity = Vector3.zero;
        _oddball.rb.angularVelocity = Vector3.zero;

        _oddball.transform.root.rotation = Quaternion.identity;
        _oddball.transform.root.position = transform.position;

        yield return new WaitForSeconds(1);

        print("SpawnOddball_Coroutine");
        _oddball.thisRoot.gameObject.SetActive(true);
    }
}
