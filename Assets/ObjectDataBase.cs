using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataBase", fileName = "NewDataBase")]
public class ObjectDataBase : ScriptableObject
{
    public List<PourAnimation> PourAnimations;
    public List<Color32> colorList;
    public List<Color32> particleColorList;
    public List<Color32> ligthEffectColorList;
}

[Serializable]
public class PourAnimation
{
    [field: SerializeField] public string Name {get; private set;}
    [Header("For Rotation Of FistBottle To Spawn Flow")]
    //Set threshold for detect rotation of first-Bottle to spawn flow of liqid
    [field: SerializeField] public float offsetZTargetDegrees;
    [field: SerializeField] public List<float> offsetZThreshHoldDegressToSpawnFlow;

    //For liquids parent in first-Bottle will be move up
    [Header("For Liquids Parent Move Up")]
    [field: SerializeField] public AnimationCurve liquidsParentMoveUpStep1;
    [field: SerializeField] public AnimationCurve liquidsParentMoveDownStep2;
    [field: SerializeField] public List<Vector3> targetLiquidsParentMoveUp ;

    //For surface liquid in first-Bottle will be move down
    [Header("For Liquid Move Down")]
    [field: SerializeField] public List<AnimationCurve> liquidMoveDownStep1;
    [field: SerializeField] public List<float> ratioOffsetPosYLq ;
    [field: SerializeField] public AnimationCurve moveAndPourCurve;
    [field: SerializeField] public AnimationCurve rotateCurveStep1;
    [field: SerializeField] public AnimationCurve rotateCurveStep2;
}