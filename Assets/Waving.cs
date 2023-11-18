using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waving : MonoBehaviour
{
    //For align
    RectTransform rtThis;
    //For apply color of wave
    [SerializeField] List<UnityEngine.UI.Image> imgWaveList;
    //For moving smooth
    [SerializeField] bool isBigWave = false;
    [SerializeField] float speedWaves = 1f;
    [SerializeField] RectTransform rtWaves;
    [SerializeField] float threshold = -21.5f;
    [SerializeField] float widthWave;

    // Start is called before the first frame update
    void Start()
    {
        rtWaves = GetComponent<RectTransform>();
        rtThis = GetComponent<RectTransform>();
        widthWave = this.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta.x;
    }

    void FixedUpdate()
    {
        if (isBigWave)
            MoveBigWave();
        else
            MoveWave();
    }

    void MoveWave()
    {
        Vector3 newPos = rtWaves.transform.localPosition;
        newPos.x -= speedWaves * Time.deltaTime;
        rtWaves.transform.localPosition = newPos;
        if (rtWaves.transform.localPosition.x <= threshold)
        {
            newPos.x = threshold + widthWave;
            rtWaves.transform.localPosition = newPos;
        }
    }

    void MoveBigWave()
    {
        Vector3 newPos = rtWaves.transform.localPosition;
        newPos.x -= speedWaves * Time.deltaTime;
        rtWaves.transform.localPosition = newPos;
        if (rtWaves.transform.localPosition.x <= threshold)
        {
            newPos.x = 0;
            rtWaves.transform.localPosition = newPos;
        }
    }

    //Aplly a common color to all waves
    public void SetColorWaves(Color32 argColor)
    {
        foreach (var imgChild in imgWaveList)
            imgChild.color = argColor;
    }
}
