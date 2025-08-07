using UnityEngine;

public class OpenCarnageReportHolder : MonoBehaviour
{
    [SerializeField] CarnageReportMenu _carnageReportMenu;
    [SerializeField] GameObject _openCarnageReportBtn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _openCarnageReportBtn.gameObject.SetActive(_carnageReportMenu.carnageReportStrucs.Count > 0);
    }
}
