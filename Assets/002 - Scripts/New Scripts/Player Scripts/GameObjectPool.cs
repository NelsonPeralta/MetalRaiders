using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public List<GameObject> pooledObject = new List<GameObject>();
    public GameObject objectPrefab;
    public int amountToPool;

    private void Start()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            //Debug.Log($"Spawning: {objectPrefab.name}");
            GameObject obj = Instantiate(objectPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            pooledObject.Add(obj);
        }
    }

    public GameObject SpawnPooledGameObject()
    {
        foreach (GameObject obj in pooledObject)
            if (!obj.activeSelf)
                return obj;
        return null;
    }
}
