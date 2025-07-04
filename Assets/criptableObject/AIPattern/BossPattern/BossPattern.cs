using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BossPattern", menuName = "Scriptable Objects/BossPattern")]
public class BossPattern : ScriptableObject
{
    [Header("스킬순서")]
    public List<SkillData> skill; // 스킬 데이터
}


[System.Serializable]
public class SkillQueue
{
    public List<SkillData> skill; //스킬
    public int currentIndex; // 현재 인덱스
}