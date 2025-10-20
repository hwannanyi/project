using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum TargetTypeX
{
    none, self, Character, Skill
}

public enum TargetTypeY
{
    none, self, Character, Skill
}
public enum Rotation
{
    none, self, Character, Skill
}

public enum DesignationType
{
    none, hp, hpRatio, distance, characterNumber
}

public enum TargetTeam
{
    enemy, team, all
}


[CreateAssetMenu(fileName = "BossPattern", menuName = "Scriptable Objects/BossPattern")]
public class BossPattern : ScriptableObject
{
    [Header("스킬순서")]
    public List<DoubleList_SkillQueue> skillQueue; // 스킬 큐
    public List<PatternList> skillCondition; //행에 들어갈 배열들
    public List<PatternList> patterns_turn_alone; //패턴
    public List<PatternList> patterns_turn_cooperation; //패턴
    public List<PatternList> patterns_turn_cooperation_end;
    public bool isRandomPattern; //패턴 랜덤실행
}

[System.Serializable]
public class DoubleList_SkillQueue
{
    public List<SkillQueue> skillQueue; //행에 들어갈 배열들
}

[System.Serializable]
public class PatternList
{
    public List<Pattern> pattern; //행에 들어갈 배열들
}

[System.Serializable]
public class DoubleList_SkillCondition
{
    public List<SkillCondition> skillCondition; //행에 들어갈 배열들
}

[System.Serializable]
public struct SkillQueue
{
    public Skill skill; //스킬
    [Header("몇턴부터 사용함?")]
    public int currentIndex; // 현재 인덱스

    [Header("스킬시전조건")]
    public Condition condition; // 현재 인덱스

    [Header("이전 스킬 사용후 현제스킬 사용까지의 시간")]
    public float delay; // 딜레이
    public float delayRandom; // 딜레이 랜덤 최대값
    public bool isCastingNotCast; //앞 순서 스킬 실행 종료 뒤에 실행

    [Header("시전 위치로부터")] 
    public Vector3 coordinate;
    public Vector3 coordinateRandom; //랜덤 최대값

    [Header("사거리 영향 받음")]
    public bool isWithin_Range; //사거리 영향 받음

    [Header("스킬 방향")]
    public Vector3 Rotation;

    [Header("타겟팅 스킬이면 미사용 좌표, 위치, " +
        "none은 시전자 위치가 시전 위치")]
    public TargetTypeX targetTypeX; // X축 타겟 타입
    public TargetTypeY targetTypeY; // Y축 타겟 타입

    [Header("타겟팅 스킬이면 미사용 좌표, 방향")]
    public Rotation RotationType;

    [Header("캐릭선택 방식, 역순, 몇등")]
    public DesignationType Designation;
    public bool reverse_order;
    public int index;

    [Header("타겟")]
    public TargetTeam target;


}

[System.Serializable]
public struct SkillCondition
{
    public Skill skill; //스킬
    [Header("몇턴부터 사용함?")]
    public int currentIndex; // 현재 인덱스

    [Header("스킬시전조건")]
    public Condition condition; // 현재 인덱스

    [Header("이전 스킬 사용후 현제스킬 사용까지의 시간")]
    public float delay; // 딜레이

    [Header("시전 위치")]
    public Vector3 coordinate;

    [Header("스킬 방향")]
    public Vector3 Rotation;

    [Header("타겟팅 스킬이면 미사용 좌표, 위치, " +
        "none은 시전자 위치가 시전 위치")]
    public TargetTypeX targetTypeX; // X축 타겟 타입
    public TargetTypeY targetTypeY; // Y축 타겟 타입

    [Header("타겟팅 스킬이면 미사용 좌표, 방향")]
    public Rotation RotationType;

    [Header("타겟")]
    public TargetTeam target;
}