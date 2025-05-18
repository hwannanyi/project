


using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Move,
    Skill
}

public class SkillSaveList : MonoBehaviour
{
    public static SkillSaveList Instance;

    public List<ActionWrapper> ReactSkillaction;
    public List<ActionWrapper> Skillaction;
    public List<SelectedSkillList> pendingSkillList;

    /*public void save()
    {
        Skillaction.Add();
    }*/

}
[System.Serializable]
public class ActionWrapper
{
    public ActionType type;

    // Move 또는 Skill 중 하나만 사용할 예정
    public Move moveData;
    public SelectedSkillList skillData;
}

[System.Serializable]
public class Move
{
    public int characterNumber;
    public int moveSpeed = 5;  // 이동 속도
    private Vector3 targetPosition;  // 목표 위치
    private Vector3 startPosition;   // 이동 시작 위치
    public int moveRange = 5; // 최대 이동 거리 제한
    public int moveCount;     // 이동가능횟수
}

[System.Serializable]
public class SelectedSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}