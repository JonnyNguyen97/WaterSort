using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    [Header("Setting before run")]
    [SerializeField] List<UtilityManager.LiquidColor> layersOfLiquid;
    [SerializeField] public GameObject pouredLiquidAtsBottle;

    [Header("Information")]
    [SerializeField] bool pouredLiquidMoveUpDone = false;
    [SerializeField] public UtilityManager.StateType stateBottle = UtilityManager.StateType.Idlle;
    [SerializeField] public List<GameObject> liquidObjects;
    [SerializeField] public UtilityManager.LiquidColor surfaceLiquidColor;
    [SerializeField] public RectTransform rtSurfaceLiquid;
    [SerializeField] public UtilityManager.LiquidColor lastLiquidColor;

    [Header("For Moving Up & Down")]
    [SerializeField] float moveTimeUpAndDown = 0.3f;
    [SerializeField] public bool isSelected = false;
    [SerializeField] Vector3 moveUpPos;
    [SerializeField] Vector3 startLocalPosBottle;
    [SerializeField] AnimationCurve moveCurve;

    [Header("For Move And Pouring Step1")]
    [SerializeField] public FlowController flowController;
    [SerializeField] public float moveAndPourTimeStep1 = 2f;
    [SerializeField] public Vector3 rawPosForPour;
    [SerializeField] Vector3 startPosLiquidsParent;
    [SerializeField] Vector3 targetPosLiquidsParent;
    [SerializeField] Vector3 startPosLiquid;
    [SerializeField] Vector3 targetPosLiquid;
    [SerializeField] Quaternion startRotation;
    [SerializeField] Quaternion targetDegrees;
    [SerializeField] Quaternion threshholdToPouredLiquidMoveUp;
    [SerializeField] AnimationCurve moveAndPourCurve;
    [SerializeField] AnimationCurve rotateCurveStep1;
    [SerializeField] AnimationCurve liquidsParentMoveUpStep1;
    [SerializeField] AnimationCurve liquidsParentMoveDownStep2;
    [SerializeField] AnimationCurve liquidMoveStep1;
    [SerializeField] AnimationCurve pouredLiquidMoveUp;
    [SerializeField] public Vector3Int targetLocalPosPouredLqInt;

    [Header("For Move And Pouring Step2")]
    [SerializeField] float moveAndPourTimeStep2 = 0.7f;
    [SerializeField] AnimationCurve rotateCurveStep2;

    [Header("References")]
    [SerializeField] public GameObject maskFlow;
    [SerializeField] public GameObject maskLiquids;
    [SerializeField] public GameObject childBigWaveSecondBottle;
    [SerializeField] public RectTransform rtTopPointLq;
    [SerializeField] public RectTransform rtBottomPointLq;

    //To SetParent for Liquid
    [SerializeField] public GameObject liquids;
    [SerializeField] public SoundManager soundManager;
    [SerializeField] UtilityManager uManager;
    [SerializeField] GameManager gameManager;
    [SerializeField] public RectTransform rtBottle;
    public Coroutine MoveUpAndDownCorouVar;

    [Header("References")]
    [SerializeField] public ParticleSystem parBottomAirBubbles;

    // Start is called before the first frame update
    void Start()
    {
        uManager = FindObjectOfType<UtilityManager>();
        soundManager = FindObjectOfType<SoundManager>();
        gameManager = FindObjectOfType<GameManager>();

        uManager.InitialLayerColorInBottle(layersOfLiquid, this);

        InitialSetPara();
    }

    void OnMouseDown()
    {
        if (uManager.firstBottleClicked == null && liquidObjects.Count > 0 && isSelected == false
        && stateBottle == UtilityManager.StateType.Idlle)
        {
            uManager.firstBottleClicked = this;
            MoveUp();
        }

        //Next, choosing second-bottle
        else if (uManager.firstBottleClicked != null)
        {
            if (uManager.firstBottleClicked != this && stateBottle == UtilityManager.StateType.Idlle)
            {
                // if (uManager.bottlesList[0] == this)
                // {
                //     Debug.LogError("Click twice on the same object!");
                //     return;
                // }

                BottleController firstBottle = uManager.firstBottleClicked;
                BottleController secondBottle = this;

                //Check conditions: (second-Bottle has same color with first-Bottl || second-Bottle is empty bottle) 
                //&& (number layer emty of second-Bottle > number layer will be same color on surface of first-Bottle)
                bool validityPour = (surfaceLiquidColor == firstBottle.surfaceLiquidColor || liquidObjects.Count == 0)
                                    && (4 - liquidObjects.Count) >= uManager.CountLayerSameTypeOnSurface(firstBottle.liquidObjects);
                if (validityPour)
                {
                    //Set active collier all liquid for detect flow touch in surface
                    uManager.SetActiveLiquidCollider(secondBottle.liquids, true);

                    //Handles moving and dumping of water
                    firstBottle.MoveAndPouringTo(secondBottle);

                    //De-selected first bottle
                    firstBottle.isSelected = false;

                    //Clear bottle list for already other bottles
                    uManager.firstBottleClicked = null;
                }
            }
        }
    }

    //For initialization and pouring water at the end
    public void InitialSetPara()
    {
        startLocalPosBottle = rtBottle.localPosition;
        moveUpPos = new Vector3(startLocalPosBottle.x
                                , startLocalPosBottle.y + (rtBottle.rect.height * rtBottle.localScale.y * 0.25f)
                                , startLocalPosBottle.z);

        rawPosForPour = new Vector3(startLocalPosBottle.x
                                , startLocalPosBottle.y + (rtBottle.rect.height * rtBottle.localScale.y * 0.25f)
                                , startLocalPosBottle.z);

        startRotation = this.rtBottle.localRotation;
    }

    public void SetParaForPouring(Vector3 argStartPosLiquidsParent, Vector3 argTargetPosLiquidsParent, Vector3 startPosLq, Vector3 targetPosLq
                                , Quaternion targetDeg, Quaternion thresholdDegress
                                , AnimationCurve argliquidMoveStep1
                                , AnimationCurve argLiquidsParentMoveUpStep1, AnimationCurve argLiquidsParentMoveDownStep2)
    {
        startPosLiquidsParent = argStartPosLiquidsParent;
        targetPosLiquidsParent = argTargetPosLiquidsParent;

        startPosLiquid = startPosLq;
        targetPosLiquid = targetPosLq;

        targetDegrees = targetDeg;
        threshholdToPouredLiquidMoveUp = thresholdDegress;
        liquidMoveStep1 = argliquidMoveStep1;
        liquidsParentMoveUpStep1 = argLiquidsParentMoveUpStep1;
        liquidsParentMoveDownStep2 = argLiquidsParentMoveDownStep2;
    }

    public void MoveAndPouringTo(BottleController secondBottle)
    {
        if (MoveUpAndDownCorouVar != null)
        {
            StopCoroutine(MoveUpAndDownCorouVar);
            MoveUpAndDownCorouVar = null;
        }
        MoveUpAndDownCorouVar = StartCoroutine(MoveAndPouringCoroutine(secondBottle));
    }

    public void MoveUp()
    {
        if (MoveUpAndDownCorouVar == null)
        {
            MoveUpAndDownCorouVar = StartCoroutine(MoveUpAndDownCoroutine(startLocalPosBottle, moveUpPos));
            isSelected = true;
        }
    }

    public void MoveDown()
    {
        if (MoveUpAndDownCorouVar == null)
        {
            MoveUpAndDownCorouVar = StartCoroutine(MoveUpAndDownCoroutine(moveUpPos, startLocalPosBottle));
            isSelected = false;
        }
    }

    IEnumerator MoveUpAndDownCoroutine(Vector3 start, Vector3 end)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);

        //Move up bottle when it's selected
        float elapsedTime = 0;
        while (elapsedTime < moveTimeUpAndDown)
        {
            float progress = elapsedTime / moveTimeUpAndDown;
            float curvedProgress = moveCurve.Evaluate(progress);
            rtBottle.localPosition = Vector3.Lerp(start, end, curvedProgress);
            elapsedTime += Time.deltaTime;
            yield return waitForSeconds;
        }
        MoveUpAndDownCorouVar = null;
    }

    IEnumerator MoveAndPouringCoroutine(BottleController secondBottle)
    {
        //First, set state of fist-Bottle
        stateBottle = UtilityManager.StateType.Pouring;
        //Determine the direction and location to pour
        Vector3 posForPour = uManager.CalculateToPassParaToPouring(this, secondBottle);

        //Set boolen for flow exits or not
        bool flowExist = false;
        pouredLiquidMoveUpDone = false;

        ////Create flow of liquid
        //StartCoroutine(uManager.CreateFlowLiquid(this, secondBottle));
        Vector3 startPosBottle = transform.localPosition;
        WaitForSeconds waitForSeconds1 = new WaitForSeconds(0.005f);

        //Move firstBottle to the last in Hierachy to its sprite overlap all other sprites
        transform.SetAsLastSibling();

        //number layer will be pour from first-Bottle to second-Bottle
        int numberLayerWillBePour = uManager.CountLayerSameTypeOnSurface(liquidObjects);

        //Determine surface wave & SetActive
        GameObject surfaceWave = rtSurfaceLiquid.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        surfaceWave.SetActive(true);

        //Create poured Liquid in second-Bottle
        pouredLiquidAtsBottle = Instantiate(uManager.liquidPrefab, secondBottle.transform);
        //pouredLiquidList = pouredLiquid ;

        targetLocalPosPouredLqInt = CreatePouredLiquidAndReturnTargetLocalPos(secondBottle, pouredLiquidAtsBottle);

        GameObject lastLiquid = null;
        //Adding pouredLiquid in liquid-Objects
        if (secondBottle.liquidObjects.Count > 0)
        {
            lastLiquid = secondBottle.liquidObjects[secondBottle.liquidObjects.Count - 1];
            if (lastLiquid.GetComponent<LiquidController>().typeLiquidColor == pouredLiquidAtsBottle.GetComponent<LiquidController>().typeLiquidColor)
            {
                secondBottle.liquidObjects[secondBottle.liquidObjects.Count - 1] = null;
                StartCoroutine(DestroyLastLiquid(lastLiquid));
            }
        }
        //Add layers will be pour from liquidObjects in second-Bottle
        for (int i = 0; i < numberLayerWillBePour; i++)
        {
            if (i == numberLayerWillBePour - 1)
                secondBottle.liquidObjects.Add(pouredLiquidAtsBottle);
            else
                secondBottle.liquidObjects.Add(null);
        }
        print($"targetPosLiquidsParent {targetPosLiquidsParent.ToString("F2")}");

        float elapsedTime = 0;
        while (elapsedTime < moveAndPourTimeStep1)
        {
            secondBottle.isSelected = true;
            float progressMove = elapsedTime / moveAndPourTimeStep1;

            ////-----HANDLE FIRST-BOTTLE----
            //For move first-bottle -> Move the first pitcher of water to the second pitcher to pour
            float curvedProgressMoveBottle = moveAndPourCurve.Evaluate(progressMove);
            rtBottle.localPosition = Vector3.Lerp(startPosBottle, posForPour, curvedProgressMoveBottle);

            //For rotate first-bottle -> When approaching the second jar
            //, you will start to tilt the first jar of water to pour
            float curvedProgressRotate = rotateCurveStep1.Evaluate(progressMove);
            rtBottle.rotation = Quaternion.Lerp(startRotation, targetDegrees, curvedProgressRotate);

            //For rotate liquid -> Keep globalRotation of liquid when the first-bottle rotate
            liquids.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, -0.00001f, 1.00000f);

            float curvedProgressMoveUpLiquidsParent = liquidsParentMoveUpStep1.Evaluate(progressMove);
            liquids.GetComponent<RectTransform>().localPosition = Vector3.Lerp(startPosLiquidsParent, targetPosLiquidsParent, curvedProgressMoveUpLiquidsParent);

            //For move down liquiq -> The liquid will gradually decrease as the jar is steeped
            float curvedProgressMoveDownLiquid = liquidMoveStep1.Evaluate(progressMove);
            rtSurfaceLiquid.localPosition = Vector3.Lerp(startPosLiquid, targetPosLiquid, curvedProgressMoveDownLiquid);

            ////-----HANDLE SECOND-BOTTLE----

            if (targetDegrees.eulerAngles.z >= 269f && rtBottle.rotation.eulerAngles.z >= 269)
            {
                float currentRotationZ = 361 - rtBottle.rotation.eulerAngles.z;
                float thresholdZ = 360 - threshholdToPouredLiquidMoveUp.eulerAngles.z;
                if (currentRotationZ >= thresholdZ && !flowExist)
                {
                    flowExist = true;
                    //Create flow of liquid
                    StartCoroutine(uManager.CreateFlowLiquid(this, secondBottle));
                }
            }
            else if (targetDegrees.eulerAngles.z <= 91f)
            {
                float currentRotationZ = 90 - rtBottle.rotation.eulerAngles.z;
                float thresholdZ = 90 - threshholdToPouredLiquidMoveUp.eulerAngles.z;
                if (currentRotationZ <= thresholdZ && !flowExist)
                {
                    flowExist = true;
                    StartCoroutine(uManager.CreateFlowLiquid(this, secondBottle));
                }
            }

            elapsedTime += Time.deltaTime;
            yield return waitForSeconds1;
        }

        ////----HANDLE FIRST-BOTTLE----
        //Stop waving
        surfaceWave.SetActive(false);

        flowController.imageFlow.enabled = false;
        Destroy(flowController.gameObject, 3.5f);

        //Remove surfaceliquid just pouring in first-Bottle
        liquidObjects.RemoveAt(liquidObjects.Count - 1);
        Destroy(rtSurfaceLiquid.gameObject);

        //Set new surface-Liquid
        rtSurfaceLiquid = uManager.FindRTSurfaceLiquid(liquidObjects);
        if (liquidObjects.Count > 0)
            surfaceLiquidColor = liquidObjects.Last().GetComponent<LiquidController>().typeLiquidColor;

        //Check if there are any liquids left in the list or if they are all null element
        uManager.CheckAllNullOrNotInList(liquidObjects);


        ////-----HANDLE SECOND-BOTTLE----
        Destroy(childBigWaveSecondBottle, 0.75f);
        secondBottle.isSelected = false;

        //Check if second-Bottle is full or not?
        bool secondBottleIsFullFill = uManager.CheckBottleFullFill(secondBottle.liquidObjects, targetLocalPosPouredLqInt.y);
        if (secondBottleIsFullFill)
        {
            soundManager.PlayWinSFX(Camera.main.transform.position);
            secondBottle.isSelected = true;

            uManager.SpawnParticleSystem((int)secondBottle.surfaceLiquidColor, secondBottle);

            soundManager.PlayFullFilSFX(Camera.main.transform.position);
            gameManager.CheckConditionToWin();
        }

        ////STEP 2: TURN BACK
        elapsedTime = 0;
        while (elapsedTime < moveAndPourTimeStep2)
        {
            float progressMove = elapsedTime / moveAndPourTimeStep2;
            //For moving
            float curvedProgressMoveBottle = moveAndPourCurve.Evaluate(progressMove);
            rtBottle.localPosition = Vector3.Lerp(posForPour, startLocalPosBottle, curvedProgressMoveBottle);

            //For rotate
            float curvedProgressRotate = rotateCurveStep2.Evaluate(progressMove);
            rtBottle.rotation = Quaternion.Lerp(targetDegrees, startRotation, curvedProgressRotate);

            //For rotate liquid -> Keep globalRotation of liquid when the bottle rotate
            liquids.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, -0.00001f, 1.00000f);

            float curvedProgressMoveUpLiquidsParent = liquidsParentMoveDownStep2.Evaluate(progressMove);
            liquids.GetComponent<RectTransform>().localPosition = Vector3.Lerp(targetPosLiquidsParent, startPosLiquidsParent, curvedProgressMoveUpLiquidsParent);

            elapsedTime += Time.deltaTime;
            yield return waitForSeconds1;
        }
        //Reset state of bottle
        stateBottle = UtilityManager.StateType.Idlle;

        //Set null coroutine var
        MoveUpAndDownCorouVar = null;
    }

    public IEnumerator PouredLiquidMoveUp(RectTransform rtPouredLiquid, Vector3Int targetLocalPosPouredLqInt)
    {
        float elapsedTime = 0;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);
        Vector3 startPos = rtPouredLiquid.localPosition;
        float pourLiquidMoveUpTime = moveAndPourTimeStep1 * 0.28f;
        while (elapsedTime < pourLiquidMoveUpTime)
        {
            float progressMove = elapsedTime / pourLiquidMoveUpTime;
            //RectTransform rtPouredLiquid = 
            float curvedProgressMovePouredLiquid = pouredLiquidMoveUp.Evaluate(progressMove);
            rtPouredLiquid.localPosition = Vector3.Lerp(startPos
                                                        , targetLocalPosPouredLqInt
                                                        , curvedProgressMovePouredLiquid);
            elapsedTime += Time.deltaTime;
            yield return waitForSeconds;
        }
        pouredLiquidMoveUpDone = true;
    }

    private IEnumerator DestroyLastLiquid(GameObject lastLiquid)
    {
        yield return new WaitWhile(() => !pouredLiquidMoveUpDone);
        if (lastLiquid != null)
            Destroy(lastLiquid);
    }

    private Vector3Int CreatePouredLiquidAndReturnTargetLocalPos(BottleController secondBottle, GameObject pouredLiquid)
    {
        //Set name of pouredLiquid
        pouredLiquid.name = $"{surfaceLiquidColor} Liquid {secondBottle.liquidObjects.Count}";

        //Set typeLiquidColor for pouredLiquid
        pouredLiquid.GetComponent<LiquidController>().typeLiquidColor = surfaceLiquidColor;

        //Set color liquid image
        GameObject liquidImage = pouredLiquid.transform.GetChild(0).gameObject;
        liquidImage.GetComponent<UnityEngine.UI.Image>().color = uManager.objData.colorList[(int)surfaceLiquidColor];

        //Get & set color childs of liquid
        Waving childSmallWaving = liquidImage.transform.GetChild(0).GetComponent<Waving>();
        childSmallWaving.SetColorWaves(uManager.objData.colorList[(int)surfaceLiquidColor]);

        //Get & set color childs of liquid
        Waving childBigWaving = liquidImage.transform.GetChild(1).GetComponent<Waving>();
        childBigWaving.SetColorWaves(uManager.objData.colorList[(int)surfaceLiquidColor]);
        childBigWaveSecondBottle = childBigWaving.gameObject;

        //Add into surfaceLiquid
        secondBottle.rtSurfaceLiquid = pouredLiquid.GetComponent<RectTransform>();
        secondBottle.surfaceLiquidColor = this.surfaceLiquidColor;

        //Count layer liquid at second-Bottle after pouring
        int countLayersLiquid = secondBottle.liquidObjects.Count + uManager.CountLayerSameTypeOnSurface(liquidObjects);
        pouredLiquid.transform.localPosition = uManager.globalPosLiquids[countLayersLiquid - 1];
        pouredLiquid.transform.SetParent(secondBottle.liquids.transform);
        
        ////Move pouredLiquid to the top Liquid.transform in Hierachy to its sprite behind all the other sprites
        pouredLiquid.transform.SetAsFirstSibling();

        //number layer will be pour from first-Bottle to second-Bottle
        int numberLayerWillBePour = uManager.CountLayerSameTypeOnSurface(liquidObjects);

        //the distance the pouredLiquid moves to create the rising liquid effect
        //85f is the distance between liquid layers
        float distanceMustMovePosY = (85f * (numberLayerWillBePour - 1)) + 101f;

        Vector3 targetLocalPosPouredLq = pouredLiquid.transform.localPosition;

        //Re-set localPos of poured liquid at bottom bottle at liquids.transform parent
        pouredLiquid.transform.localPosition = new Vector3(0, -200f, 0);

        //Set currentIndex for pouredLiquid
        Vector3Int localPosPouredAtSecondBottle = uManager.GetLocalPosReferParent(secondBottle, targetLocalPosPouredLq);
        pouredLiquid.GetComponent<LiquidController>().currentIndex = uManager.globalPosLiquids.IndexOf(localPosPouredAtSecondBottle);
        return new Vector3Int(Mathf.RoundToInt(targetLocalPosPouredLq.x), Mathf.RoundToInt(targetLocalPosPouredLq.y), Mathf.RoundToInt(targetLocalPosPouredLq.z));
    }

    public void InsertLiquidObjects(GameObject liquid, UtilityManager.LiquidColor liquidColor)
    {
        liquidObjects.Insert(0, liquid);
        lastLiquidColor = liquidColor;
    }

}
