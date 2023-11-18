using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] List<SceneDataBase> scenesData;
    [SerializeField] int startLevel = 1;

    [Header("Data")]
    [SerializeField] int currentBottlesFullFill = 0;
    [SerializeField] List<GameObject> bottleList;

    [Header("References")]
    [SerializeField] Canvas objectCanvas;

    [Header("References UI")]
    [SerializeField] TextMeshProUGUI txtCurrentLevel;
    [SerializeField] GameObject txtPauseDis;
    [SerializeField] GameObject btnPause;
    [SerializeField] GameObject btnContinue;


    // /// <summary>
    // /// SingleTon Parttern
    // /// </summary>
    // private static GameManager inst;
    // public static GameManager Inst { get => inst; }

    // Start is called before the first frame update
    void Start()
    {
        //ManagerSingleTon();

        if (startLevel > 0)
            IntitialObjectInScene(startLevel);
        else
            Debug.LogError($"Start Level must be > 0");
    }

    // void ManagerSingleTon()
    // {
    //     if (inst != null)
    //     {
    //         gameObject.SetActive(false);
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         inst = this;
    //         DontDestroyOnLoad(gameObject);
    //     }
    // }

    private void IntitialObjectInScene(int currentLevel)
    {
        if (currentLevel > scenesData.Count)
        {
            Debug.LogError($"Level Over!!");
            return;
        }

        txtCurrentLevel.text = $"LEVEL {currentLevel}";

        SceneDataBase scene = scenesData[currentLevel - 1];
        if (scene.bottleList.Count > 0)
        {
            for (int i = 0; i < scene.bottleList.Count; i++)
            {
                GameObject bottle = Instantiate(scene.bottleList[i], objectCanvas.transform);
                bottle.GetComponent<RectTransform>().localPosition = scene.posofBottlesInCavas[i];
                bottleList.Add(bottle);
            }
        }
        else
            Debug.LogError($"Scene at {currentLevel} level has bottleList empty!");
    }

    public void CheckConditionToWin()
    {
        currentBottlesFullFill++;
        if (currentBottlesFullFill == scenesData[startLevel - 1].conditionToWin)
            StartCoroutine(WaitSecondAndNextLevel());
    }

    IEnumerator WaitSecondAndNextLevel()
    {
        yield return new WaitForSeconds(2.5f);
        NextLevel();
    }

    //For btnNext
    public void NextLevel()
    {
        foreach (GameObject bottle in bottleList)
            Destroy(bottle);

        bottleList.Clear();
        currentBottlesFullFill = 0;
        startLevel++;
        IntitialObjectInScene(startLevel);
    }

    //For btnPrevious
    public void PreviousLevel()
    {
        if (startLevel == 1)
            return;

        foreach (GameObject bottle in bottleList)
            Destroy(bottle);

        bottleList.Clear();
        currentBottlesFullFill = 0;
        startLevel--;
        IntitialObjectInScene(startLevel);
    }

    //For btnReload
    public void ReloadAtCurrentLevel()
    {
        ContinueGame();
        foreach (GameObject bottle in bottleList)
            Destroy(bottle);

        bottleList.Clear();
        currentBottlesFullFill = 0;
        IntitialObjectInScene(startLevel);

    }

    //For btnPause
    public void PauseGame()
    {
        Time.timeScale = 0;
        btnPause.SetActive(false);
        txtPauseDis.SetActive(true);
        btnContinue.SetActive(true);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
        btnPause.SetActive(true);
        txtPauseDis.SetActive(false);
        btnContinue.SetActive(false);
    }
}
