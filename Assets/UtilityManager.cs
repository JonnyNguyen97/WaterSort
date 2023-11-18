using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UtilityManager : MonoBehaviour
{
    public enum LiquidColor { Turquoise = 0, Yellow = 1, Blue = 2, Orange = 3, AirforceBlue = 4, Lightseagreen = 5, Empty = 99 };
    public enum StateType { Idlle, Pouring };
    [SerializeField] public List<Vector3Int> globalPosLiquids;
    [SerializeField] public BottleController firstBottleClicked;

    [Header("Reference")]
    [SerializeField] public ObjectDataBase objData;

    [Header("Prefab")]
    [SerializeField] public GameObject liquidPrefab;
    [SerializeField] public FlowController flowControllerPrefab;

    [Header("Particle Prefab")]
    [SerializeField] public GameObject parShootFire;
    [SerializeField] public GameObject parShootTinselPaper1;
    [SerializeField] public GameObject parShootTinselPaper2;
    [SerializeField] public GameObject parSurfaceAirBubbles;
    [SerializeField] public GameObject parlightEffect;

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (firstBottleClicked != null)
            {
                //MoveUp is NOT executing
                if (firstBottleClicked.MoveUpAndDownCorouVar == null && firstBottleClicked.isSelected)
                {
                    firstBottleClicked.MoveDown();
                    firstBottleClicked = null;
                }
            }
        }
    }

    public void InitialLayerColorInBottle(List<LiquidColor> layersOfLiquid, BottleController thisBottle)
    {
        Vector3 localPosTop = thisBottle.rtTopPointLq.localPosition;
        Vector3 localPosBottom = thisBottle.rtBottomPointLq.localPosition;

        List<Vector3> localPosLiquids = CalculateLocalPosLiquids(layersOfLiquid.Count, localPosTop, localPosBottom);
        for (int i = layersOfLiquid.Count - 1; i >= 0; i--)
        {
            //If layer lquid not empty
            if (layersOfLiquid[i] != LiquidColor.Empty)
            {
                GameObject objLiquid = Instantiate(liquidPrefab, thisBottle.transform);

                //Set localPos at bottle.tranform calculate base on localPosTop and localPosBottom
                objLiquid.transform.localPosition = localPosLiquids[i];

                //Set current of liquid for dertermind targetPosLiquid in ScriptableObject
                Vector3Int referPosInt = new Vector3Int(0, (int)localPosLiquids[i].y, 0);
                objLiquid.GetComponent<LiquidController>().currentIndex = globalPosLiquids.IndexOf(referPosInt);

                //Set name
                objLiquid.name = $"{layersOfLiquid[i]} Liquid";

                //Set typeColor for LiquidController of instance
                objLiquid.GetComponent<LiquidController>().typeLiquidColor = layersOfLiquid[i];

                //Get & set color of liquid
                GameObject liquidImage = objLiquid.transform.GetChild(0).gameObject;
                liquidImage.GetComponent<UnityEngine.UI.Image>().color = objData.colorList[(int)layersOfLiquid[i]];

                //Get & set color childs of liquid
                Waving childWaving = liquidImage.transform.GetChild(0).GetComponent<Waving>();
                childWaving.SetColorWaves(objData.colorList[(int)layersOfLiquid[i]]);

                //SetParent in fill object
                objLiquid.transform.SetParent(thisBottle.liquids.transform);

                //Add liquid into the list
                if (thisBottle.liquidObjects.Count == 0)
                {
                    thisBottle.InsertLiquidObjects(objLiquid, layersOfLiquid[i]);

                    //Determine surface liquid
                    thisBottle.rtSurfaceLiquid = objLiquid.GetComponent<RectTransform>();
                    thisBottle.surfaceLiquidColor = layersOfLiquid[i];
                }
                else if (thisBottle.liquidObjects.Count > 0)
                {
                    //If liquid has the same type color as before -> don't adding & remove it!
                    if (thisBottle.lastLiquidColor == layersOfLiquid[i])
                    {
                        thisBottle.liquidObjects.Insert(0, null);
                        Destroy(objLiquid);
                    }
                    else
                        thisBottle.InsertLiquidObjects(objLiquid, layersOfLiquid[i]);
                }
            }
        }
    }

    //Calculate the localPosition of each liquid layer
    List<Vector3> CalculateLocalPosLiquids(int countLayer, Vector3 toplocalPos, Vector3 bottomLocalPos)
    {
        List<Vector3> collectLocalPos = new List<Vector3>();

        //Calculate half the height of liquids 
        float halfHeight = (toplocalPos.y - bottomLocalPos.y) / countLayer / 2;

        //Set start from top to bottom on Y-axis
        float startTopPosY = toplocalPos.y;

        for (int i = 0; i < countLayer; i++)
        {
            float height = halfHeight * 2;
            collectLocalPos.Insert(0, new Vector3(toplocalPos.x, startTopPosY - (i * height), toplocalPos.z));
        }
        return collectLocalPos;
    }

    //Determine the direction and location to pour
    public Vector3 CalculateToPassParaToPouring(BottleController firstBottle, BottleController secondBottle)
    {
        //Declare variables
        Vector3 startPosLiquidsParent = firstBottle.liquids.GetComponent<RectTransform>().localPosition;
        Vector3 targetPosLiquidsParent;

        Vector3 startPosLiquid = firstBottle.rtSurfaceLiquid.localPosition;
        Vector3 targetPosLiquid;
        Quaternion targetDegrees;
        Quaternion thresholdDegreeToPouredMoveUp;
        Vector3 posForPour = secondBottle.rawPosForPour;

        int referIndex = FindIndexInScriptableObject(firstBottle.liquidObjects);
        int currentIndex = firstBottle.rtSurfaceLiquid.GetComponent<LiquidController>().currentIndex;

        //Calculate precise value
        float preciseWidthBottle = secondBottle.rtBottle.rect.width * secondBottle.rtBottle.localScale.x;
        float preciseHeightFirstLiquid = firstBottle.rtSurfaceLiquid.rect.height * firstBottle.rtSurfaceLiquid.localScale.y;
        Vector3 rotation = firstBottle.rtBottle.localRotation.eulerAngles;
        //Case1: firstBottle (Left) < secondBottle (Right)
        if (firstBottle.transform.localPosition.x <= secondBottle.transform.localPosition.x)
        {
            targetPosLiquidsParent = objData.PourAnimations[referIndex].targetLiquidsParentMoveUp[currentIndex];
            //FirstBottle will move to this position to pour water
            posForPour.x = secondBottle.rawPosForPour.x - preciseWidthBottle * 0.275f;

            targetDegrees = Quaternion.Euler(0, 0, rotation.z - objData.PourAnimations[referIndex].offsetZTargetDegrees);
            thresholdDegreeToPouredMoveUp = Quaternion.Euler(0, 0, rotation.z - objData.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]);
        }

        //Case2: secondBottle (Left) < firstBottle (Right)
        else
        {
            targetPosLiquidsParent = objData.PourAnimations[referIndex].targetLiquidsParentMoveUp[currentIndex];
            targetPosLiquidsParent.x = -targetPosLiquidsParent.x;
            //FirstBottle will move to this position to pour water
            posForPour.x = secondBottle.rawPosForPour.x + preciseWidthBottle * 0.275f;

            targetDegrees = Quaternion.Euler(0, 0, rotation.z + objData.PourAnimations[referIndex].offsetZTargetDegrees);
            thresholdDegreeToPouredMoveUp = Quaternion.Euler(0, 0, rotation.z + objData.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]);
        }
        print($"offsetZThreshHoldDegressToSpawnFlow {objData.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]}");

        //The liquid in first-Bottle will moveup to this position when water is poured
        float offSetMoveDownPosy = startPosLiquid.y - (preciseHeightFirstLiquid * objData.PourAnimations[referIndex].ratioOffsetPosYLq[currentIndex]);
        targetPosLiquid = new Vector3(startPosLiquid.x
                                    , offSetMoveDownPosy
                                    , startPosLiquid.z);


        print($"referIndex {referIndex} - currentIndex {currentIndex} | value: {objData.PourAnimations[referIndex].ratioOffsetPosYLq[currentIndex]} | offSetPosy {offSetMoveDownPosy}");

        //For refer Animation in Scriptabl object
        AnimationCurve liquidsParentMoveUpStep1 = objData.PourAnimations[referIndex].liquidsParentMoveUpStep1;
        AnimationCurve liquidsParentMoveDownStep2 = objData.PourAnimations[referIndex].liquidsParentMoveDownStep2;
        AnimationCurve liquidMoveStep1 = objData.PourAnimations[referIndex].liquidMoveDownStep1[currentIndex];

        //Pass all parameter for first
        firstBottle.SetParaForPouring(startPosLiquidsParent, targetPosLiquidsParent, startPosLiquid, targetPosLiquid
                                    , targetDegrees, thresholdDegreeToPouredMoveUp
                                    , liquidMoveStep1
                                    , liquidsParentMoveUpStep1, liquidsParentMoveDownStep2);

        return posForPour;
    }

    public IEnumerator CreateFlowLiquid(BottleController fBottle, BottleController sBottle)
    {
        yield return new WaitForSeconds(0.01f);

        //Create water flow
        GameObject objectFlow = Instantiate(flowControllerPrefab.gameObject, sBottle.transform);

        fBottle.flowController = objectFlow.GetComponent<FlowController>();
        fBottle.flowController.SetReferenceObject(fBottle, sBottle, this);

        //Check left or right
        if (fBottle.transform.localPosition.x <= sBottle.transform.localPosition.x)
            objectFlow.GetComponent<RectTransform>().localPosition = fBottle.flowController.leftLocalPos;
        else
            objectFlow.GetComponent<RectTransform>().localPosition = fBottle.flowController.rightLocalPos;

        //SetParent for flow to cover it by MaskLiquid
        objectFlow.transform.SetParent(sBottle.maskFlow.transform);

        //Move objectFlow to the top secondBottle.transform in Hierachy to its sprite behind all the other sprites
        objectFlow.transform.SetAsFirstSibling();

        //Set color flow
        objectFlow.GetComponent<UnityEngine.UI.Image>().color = objData.colorList[(int)fBottle.surfaceLiquidColor];

        fBottle.soundManager.PlayPourLiquidSFX(Camera.main.transform.position);
        fBottle.childBigWaveSecondBottle.SetActive(true);
    }

    public bool CheckBottleFullFill(List<GameObject> liquidObjects, int posYInt)
    {
        if (liquidObjects.Count < 4)
            return false;

        int countNull = liquidObjects.Where(x => x == null).Count();

        if (countNull == 3 && posYInt == 145)
            return true;
        else
            return false;
    }

    public void SpawnParticleSystem(int indexColor, BottleController bottleFullFill)
    {
        float scaleFactor = bottleFullFill.GetComponent<RectTransform>().localScale.x;

        GameObject inst = Instantiate(parShootFire, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -200);

        inst = Instantiate(parShootTinselPaper1, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -200);

        inst = Instantiate(parShootTinselPaper2, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -200);

        inst = Instantiate(parlightEffect, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -100, -100);
        ParticleSystem particleSystem = inst.GetComponent<ParticleSystem>();
        var mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(objData.ligthEffectColorList[indexColor]);

        inst = Instantiate(parlightEffect, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -250, -100);
        particleSystem = inst.GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(objData.ligthEffectColorList[indexColor]);

        inst = Instantiate(parSurfaceAirBubbles, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -55, -200);
        particleSystem = inst.GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(Color.white, objData.particleColorList[indexColor]);

        bottleFullFill.parBottomAirBubbles.transform.localScale *= scaleFactor;
        particleSystem = bottleFullFill.parBottomAirBubbles;
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(objData.particleColorList[indexColor], Color.white);
        bottleFullFill.parBottomAirBubbles.GetComponent<ParticleSystemRenderer>().enabled = true;
    }

    public Vector3Int GetLocalPosReferParent(BottleController parentBottle, Vector3 localPosInLiquid)
    {
        GameObject temp = new GameObject();
        temp.transform.SetParent(parentBottle.liquids.transform);
        temp.transform.localPosition = localPosInLiquid;
        temp.transform.SetParent(parentBottle.transform);
        Vector3Int referPos = new Vector3Int(0, Mathf.RoundToInt(temp.transform.localPosition.y), 0);
        Destroy(temp);
        return referPos;
    }

    public void CheckAllNullOrNotInList(List<GameObject> argLiquidObjects)
    {
        bool allElementInListIsNull = true;
        foreach (GameObject child in argLiquidObjects)
        {
            if (child != null)
                allElementInListIsNull = false;
        }

        if (allElementInListIsNull)
            argLiquidObjects.Clear();
    }

    //number layer will be pour from first-Bottle to second-Bottle
    public int CountLayerSameTypeOnSurface(List<GameObject> argLiquidObjects)
    {
        int countLayer = 0;

        for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
        {
            //First iterate
            if (i == argLiquidObjects.Count - 1)
            {
                if (argLiquidObjects[i] != null)
                    countLayer++;
                else
                    Debug.LogError($"This argLiquidObjects has null at the last element!");
            }
            //And after...
            else
            {
                if (argLiquidObjects[i] == null)
                    countLayer++;
                else
                    break;
            }
        }
        return countLayer;
    }

    //Find the index that calculates from that index to the surface of the water and all the water will be poured out!
    public int FindIndexInScriptableObject(List<GameObject> argLiquidObjects)
    {
        int lastIndex = -1;

        for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
        {
            //First iterate
            if (lastIndex == -1)
            {
                if (argLiquidObjects[i] != null)
                    lastIndex = i;
            }
            //And after...
            else
            {
                if (argLiquidObjects[i] == null)
                {
                    lastIndex = i;
                    if (i > 0)
                    {
                        if (argLiquidObjects[i - 1] != null)
                            break;
                    }
                }
                else
                    break;
            }
        }

        return lastIndex;
    }

    public void SetActiveLiquidCollider(GameObject liquids, bool val)
    {
        foreach (Transform liquid in liquids.transform)
        {
            //Empty Bottle Bottom Collider is for cases where multiple bottles are poured at the same time into the second Bottle
            if (liquid.name != "EmptyBottle_BottomCollider")
                liquid.gameObject.GetComponent<BoxCollider2D>().enabled = val;
        }
    }

    public RectTransform FindRTSurfaceLiquid(List<GameObject> argLiquidObjects)
    {
        RectTransform rectTrans = null;
        if (argLiquidObjects.Count > 0)
        {
            for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
            {
                if (argLiquidObjects[i] != null)
                {
                    rectTrans = argLiquidObjects[i].GetComponent<RectTransform>();
                    break;
                }
                //Remove null element in list
                else
                    argLiquidObjects.RemoveAt(i);
            }
        }
        return rectTrans;
    }

    private int CalculateSumFromNToZero(int n)
    {
        if (n == 0)
            return 0;
        else
            return n + CalculateSumFromNToZero(n - 1);
    }
}
