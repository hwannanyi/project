using System.Collections.Generic;
using UnityEngine;

public class SkillSaveList : MonoBehaviour
{
    public static SkillSaveList Instance;

    public SelectedSkillList selectedSkillList;
    public List<PendingSkillList> pendingSkillList;
    public List<ReactPendingSkillList> reactPendingSkillList;

    public List<Action> actionQueue = new();

}
public interface Action
{
    void Execute();  // 스킬 or 이동 실행
}

[System.Serializable]
public class Move : Action
{
    public int characterNumber;
    public int moveSpeed = 5;  // 이동 속도
    private Vector3 targetPosition;  // 목표 위치
    private Vector3 startPosition;   // 이동 시작 위치
    public int moveRange = 5; // 최대 이동 거리 제한
    public int moveCount;     // 이동가능횟수

    public void Execute()
    {
        Debug.Log($"캐릭터 {characterNumber} 이동 시작 (속도: {moveSpeed}, 범위: {moveRange})");
        // 여기에 실제 이동 처리 로직 삽입 (ex. Coroutine 사용 등)
    }
}

[System.Serializable]
public class SelectedSkillList : Action
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;

    public void Execute()
    {
        Debug.Log($"스킬 사용: {selectedSkill.skillName} by {selectedCharacter?.name ?? "Unknown"}");
        // 여기에 실제 스킬 사용 로직 삽입
    }
}

[System.Serializable]
public class PendingSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}

[System.Serializable]
public class ReactPendingSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}