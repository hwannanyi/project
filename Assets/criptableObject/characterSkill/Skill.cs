using System;
using System.Collections.Generic;
using UnityEngine;

public enum skillType 
{ attack, defense, assistance, movement }

public enum StartSkillPosition 
{ player, target, mouse, special}

public enum projectileType
{ straight, throwtype }
public enum Target
{ self, team, enemy, all, spTarget}
public enum aoeType
{ single, square, spAoe}


public enum aoeCenter
{ center, edge, Rcorner, Lcorner}

public enum IncreaseType
{ none, ad, ap, hp }


public enum skillhitEffect
{
    none,
    damage,
    heal,
    Shields
}

public enum condition_statement
{
    none,       //없음
    more,       //이상
    over,       //초과
    below,      //이하
    under,      //미만
    equal,      //같다
    inequality  //같지않다
}

public enum condition_effect
{
    none,
    more,
    over,
    below,
    under,
    equal,
    inequality
}

public enum Debuffs
{ none, corrosion }

public enum Buffs
{ none, solid }

public enum CCs
{ none, stun }

public enum React
{
    no, maintarget, all
}

[System.Serializable]
public class DoubleList_Vector2
{
    public Vector2[] list; //행에 들어갈 배열들
}

[System.Serializable]
[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;      // 스킬 이름
    public Sprite SkillEffectIllustration;
    public GameObject SkillEffectPrefab;
    public bool passive;          // 패시브 여부 (false면 액티브)
    public List<skillType> skillTypes;      // 스킬 타입 (공격, 방어, 보조, 이동)
    [TextArea] public string tooltip; // 스킬 설명

    public int actionsNumber = 1; // 스킬 행동 개수 (기본 1)
    public int skillNumber = 1;   // 스킬의 갯수 (변형 가능)
    public int skillCumulative = 1; // 최대 충전 횟수 (기본 1)

    public StartSkillPosition startSkillPosition; // 스킬 시작 위치 (플레이어, 지정된 대상 등)
    public int XstartSkillPosition;
    public int YstartSkillPosition;

    public bool projectile;       // 투사체 여부 (false면 히트스캔)
    public bool targeting;        // 타겟팅 여부 (false면 논타겟팅)
    public bool penetration;     // 투사체의 관통 여부
    public projectileType projectileType;
    public List<Target> skillTarget; // 적중 가능한 대상 (자신, 아군, 적 등)
    public float projectileSpeed; // 투사체 속도 (히트스캔이면 0)  //////////(연출용)
    public float afterdelay; // 선딜 //////////(연출용)
    public float beforedelay; // 후딜 //////////(연출용)

    public float range;           // 사거리
    public aoeType aoetype;        // 범위 유형 (단일, 직선, 정사각형 등)
    public bool fourRotation;      // 4방향 회전
    public bool effectRotation;    // 방향에 따른 스킬의 회전
    public float Xaoe;             // 범위 크기
    public float Yaoe;             // 범위 크기
    public DoubleList_Vector2[] specialAoe; // 특수 범위 (2차원 좌표 리스트)
    public aoeCenter aoecenter;    // 광역기의 중심점

    public int cooldown;        // 쿨타임
    public int colldownSkill;   // 지속 스킬횟수
    public int basicValue;      // 기본 위력
    public UDictionary<IncreaseType, float> increase; // 위력 계수 (예: {공격력: 0.1}, {방어력: 0.5})

    public int damageHit;         // 타격 횟수
    public float hitSpeed;        // 타격 속도


    public List<HitEffectEntry> hitEffects;

    public React react; //대응가능유무 + 대응가능대상


}



[System.Serializable]
public class HitEffects
{
    public List<DamageEffect> DamageEffect; //적중 효과
    public List<DebuffEffect> DebuffEffects; //디버프 효과
    public List<BuffEffect> BuffEffects;   //버프 효과
    public List<CCEffect> CCEffects;     //CC 효과
}

[System.Serializable]
public class HitEffectEntry
{
    public Target target;
    public HitEffects effects;
}

public class EffectsCondition
{

}

[System.Serializable]
public class DamageEffect
{
    public List<skillhitEffect> skillhitEffect; //데미지? 힐? 실드?
    public string condition;       // 효과 발동 조건 (예: "공격력 50% 이상")
    public float baseValue;        // 기본 위력
    public UDictionary<IncreaseType, float> increase; //위력 계수
    public int dot;                // 지속 시간 (턴)
}

[System.Serializable]
public class ConditionalEffect
{
}
[System.Serializable]
public class DebuffEffect
{
    public Debuffs Debuff;
    public float baseValue;        // 기본 위력
    public UDictionary<IncreaseType, float> increase; //위력 계수
    public int trunDuration;       // 기본 지속 시간 (턴사이클 수)
    public UDictionary<IncreaseType, float> timeIncrease; // 지속 시간 계수
}

[System.Serializable]
public class BuffEffect
{
    public Buffs Debuff;
    public float baseValue;        // 기본 위력
    public float increaseValue;    // 위력 계수
    public int trunDuration;       // 지속 시간 (턴사이클 수)
    public int skillDuration;      // 지속 스킬횟수
    public int numberDurations;    // 지속 일반스킬횟수
    public int counterDurations;   // 지속 대응횟수
}

[System.Serializable]
public class CCEffect
{
    public CCs Debuff;
    public float baseValue;        // 기본 위력
    public float increaseValue;    // 위력 계수
    public int trunDuration;       // 지속 시간 (턴사이클 수)
    public int skillDuration;      // 지속 스킬횟수
    public int numberDurations;    // 지속 일반스킬횟수
    public int counterDurations;   // 지속 대응횟수
}