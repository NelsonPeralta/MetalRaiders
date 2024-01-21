using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePool : MonoBehaviour
{
    public static GrenadePool instance { get { return _instance; } }

    [SerializeField] GameObject _fragGrenadePrefab, _stickyGrenadePrefab;
    [SerializeField] List<GameObject> _fragGrenadePool = new List<GameObject>();
    [SerializeField] List<GameObject> _stickyGrenadePool = new List<GameObject>();


    static GrenadePool _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }



        for (int i = 0; i < 1000; i++)
        {
            _fragGrenadePool.Add(Instantiate(_fragGrenadePrefab, transform));
            _stickyGrenadePool.Add(Instantiate(_stickyGrenadePrefab, transform));

            _fragGrenadePool[i].SetActive(false); _stickyGrenadePool[i].SetActive(false);
            _fragGrenadePool[i].transform.SetParent(this.transform); _stickyGrenadePool[i].transform.SetParent(this.transform);
        }
    }


    public static int GetAvailableGrenadeIndex(bool isFrag, int photonRoomIndex)
    {
        for (int i = (photonRoomIndex - 1) * 10; i < (photonRoomIndex * 10) - 1; i++)
        {
            if (isFrag)
            {
                if (!_instance._fragGrenadePool[i].activeInHierarchy) return i;
            }
            else
                if (!_instance._stickyGrenadePool[i].activeInHierarchy) return i;
        }

        return -1;
    }

    public static GameObject GetGrenade(bool isFrag, int index)
    {
        if (isFrag) return _instance._fragGrenadePool[index];
        else return _instance._stickyGrenadePool[index];
    }
}
