using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowController : MonoBehaviour
{
    [SerializeField] BottleController firstBottle;
    [SerializeField] BottleController secondBottle;
    [SerializeField] UtilityManager uManager;
    [SerializeField] public Vector3 leftLocalPos;
    [SerializeField] public Vector3 rightLocalPos;
    [SerializeField] float speedMoveDown = 2f;
    [SerializeField] float speedIncrHeight = 2f;
    [SerializeField] RectTransform rtFlow;
    [SerializeField] public UnityEngine.UI.Image imageFlow;
    Coroutine pouredLiquidMoveUpCoroutine;

    // Update is called once per frame
    void Update()
    {
        ExpandScaleY();
    }

    //Name it to distinguish objects when touched
    public void SetReferenceObject(BottleController fBottle, BottleController sBottle, UtilityManager argUtilityManager)
    {
        firstBottle = fBottle;
        secondBottle = sBottle;
        uManager = argUtilityManager;
    }

    private void ExpandScaleY()
    {
        Vector3 newLocalPos = rtFlow.localPosition;
        newLocalPos.y -= speedMoveDown;
        rtFlow.localPosition = newLocalPos;

        Vector2 widthHeight = rtFlow.sizeDelta;
        widthHeight.y += speedIncrHeight;
        rtFlow.sizeDelta = widthHeight;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //If first bottle touche
        if (col.gameObject.CompareTag("Liquid") && pouredLiquidMoveUpCoroutine == null)
        {
            uManager.SetActiveLiquidCollider(secondBottle.liquids, false);
            pouredLiquidMoveUpCoroutine = StartCoroutine(firstBottle.PouredLiquidMoveUp(firstBottle.pouredLiquidAtsBottle.GetComponent<RectTransform>()
                                                        , firstBottle.targetLocalPosPouredLqInt));
        }
    }

}
