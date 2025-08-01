using System.Collections.Generic;
using UnityEngine;
public enum Team
{ team, enemy, neutral, spTarget }

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]

public class Character : ScriptableObject
{
    public string charactername;  // 캐릭 이름
    public int maxhp;             // 최대체력
    public int maxmp;             // 최대코스트
    public int atk;               // 공격력
    public int def;               // 방어력
    public int speed;             // 속도
    public int movespeed;         // 이속
    public int moveCount;         // 이동가능횟수
    public int trun;              // 보유 턴
    public Team team;             // 팀
    public bool summons = false;          // 소환수인가요?
    public List<Skill> useSkill = new List<Skill> { null, null, null, null, null, null, null, null, null, null };   // 사용스킬

    public List<PassiveList> passiveSkill;   // 패시브 스킬
    public Sprite characterillustration;
    public Sprite characterProfileillustration;
    public GameObject characterPrefab;

    public BossPattern skillQueue; // 스킬 큐
}
[System.Serializable]
public class PassiveList
{
    public ConditionHit conditionHit; // 패시브 조건
    public Skill passive = null; // 패시브 스킬
    public SkillAutoCast passiveTarget; // 패시브 타겟팅 정보
}

[System.Serializable]
public class SkillAutoCast
{
    [Header("이전 스킬 사용후 현제스킬 사용까지의 시간")]
    public bool isCastingNotCast; //앞 순서 스킬 실행 종료 뒤에 실행

    [Header("시전 위치로부터")]
    public Vector3 coordinate;

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




