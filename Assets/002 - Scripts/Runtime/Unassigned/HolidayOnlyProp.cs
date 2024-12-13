using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolidayOnlyProp : MonoBehaviour
{
    enum holiday { valentine, st_pratick, easter, halloween, christmas }

    [SerializeField] holiday _holiday;


    // Start is called before the first frame update
    void Start()
    {
        if (transform.childCount == 1)
        {
            if (_holiday == holiday.valentine)
                transform.GetChild(0).gameObject.SetActive(DateTime.Now == new DateTime(DateTime.Now.Year, 2, 14));
            else if (_holiday == holiday.st_pratick)
                transform.GetChild(0).gameObject.SetActive(DateTime.Now == new DateTime(DateTime.Now.Year, 3, 17));
            else if (_holiday == holiday.easter)
                transform.GetChild(0).gameObject.SetActive(DateTime.Now == new DateTime(DateTime.Now.Year, 4, 20));
            else if (_holiday == holiday.halloween)
                transform.GetChild(0).gameObject.SetActive(DateTime.Now >= new DateTime(DateTime.Now.Year, 10, 1) &&
                    DateTime.Now < new DateTime(DateTime.Now.Year, 11, 1));
            else if (_holiday == holiday.christmas)
                transform.GetChild(0).gameObject.SetActive(DateTime.Now >= new DateTime(DateTime.Now.Year, 12, 1) &&
                    DateTime.Now <= new DateTime(DateTime.Now.Year, 12, 31));
            else
                Destroy(gameObject);
        }
        else
            transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
