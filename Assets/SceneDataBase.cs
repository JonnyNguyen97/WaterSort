using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SceneDataBase", fileName = "NewScene")]
public class SceneDataBase : ScriptableObject
{
    [SerializeField] public int conditionToWin;
    [SerializeField] public List<GameObject> bottleList;
    [SerializeField] public List<Vector3> posofBottlesInCavas;
}
