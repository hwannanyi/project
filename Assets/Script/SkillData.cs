using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.VolumeComponent;


[System.Serializable]
public class SkillData
{
    public string useCharacterName; // 시전자 이름
    public string skillName;      // 스킬 이름
    public Sprite skillIcon;        // 스킬 아이콘
    public Sprite SkillEffectIllustration;
    public GameObject SkillEffectPrefab;
    public bool passive;          // 패시브 여부 (false면 액티브)
    public List<skillType> skillTypes;      // 스킬 타입 (공격, 방어, 보조, 이동)
    public string tooltip;        // 스킬 설명

    public Skill AdditionalSkills; // 추가 스킬 (예: 연계기 등)
    public SkillData AdditionalSkillData;

    public Stats summonCharacter; // 소환수 캐릭터 (예: 소환수, 함정 등)


    public int actionsNumber;     // 스킬 행동 개수 (기본 1)
    public int skillNumber;       // 스킬의 갯수 (변형 가능)
    public int skillCumulative;   // 최대 충전 횟수 (기본 1)

    public UDictionary<CostType, int> cost; // 코스트
    public UDictionary<CostType, int> currentcost; // 기본코스트

    public StartSkillPosition startSkillPosition; // 스킬 시작 위치 (플레이어, 지정된 대상 등)
    public int XstartSkillPosition;
    public int YstartSkillPosition;

    public bool projectile;       // 투사체 여부 (false면 히트스캔)
    public bool targeting;        // 타겟팅 여부 (false면 논타겟팅)
    public bool penetration;     // 투사체의 관통 여부
    public bool tracking;       // 경로에 스킬 생성
    public bool RangeAdjustment;// 임의로 거리 조절 가능한
    public projectileType projectileType;

    public List<Target> skillTarget; // 적중 가능한 대상 (자신, 아군, 적 등)
    public float projectileSpeed; // 투사체 속도 (히트스캔이면 0)  //////////(연출용)
    public float afterdelay; // 선딜 //////////(연출용)
    public float beforedelay; // 후딜 //////////(연출용)
    public float range;           // 사거리
    public AoeType aoetype;        // 범위 유형 (단일, 직선, 정사각형 등)
    public bool fourRotation;
    public bool effectRotation;
    public float Xaoe;             // 범위 크기
    public float Yaoe;             // 범위 크기
    public AoeCenter aoecenter;

    public AoeInfo[] specialAoe; // 특수 범위 (크기, 위치 배열)

    public int cooldown;        // 기본쿨타임
    public int currentCooldown; //현제 쿨타임
    public int colldownTime;        // 쿨타임
    public int colldownSkill;      // 지속 스킬횟수

    public int basicValue;      // 기본 위력
    public UDictionary<IncreaseType, float> increase; // 위력 계수 (예: {공격력: 0.1}, {방어력: 0.5})
    public int damageHit;         // 타격 횟수
    public float hitSpeed;        // 타격 속도
    public List<HitEffectEntry> hitEffects;
    public List<ConditionalEffect> conditionalEffects; // 특정 조건부 효과

    public React react; //대응가능유무 + 대응가능대상
    public bool isreactSkill; // 4방향 회전
    public float reactTime;
    

    public SkillData(Skill data, string characterName, bool isreactSkill)
    {
        if (data == null)
        {
            return;
        }
        isreactSkill = this.isreactSkill;
        skillName = data.skillName;
        useCharacterName = characterName; // 직접 문자열을 할당

        skillIcon = data.skillIcon;
        SkillEffectIllustration = data.SkillEffectIllustration;
        SkillEffectPrefab = data.SkillEffectPrefab;

        passive = data.passive;
        skillTypes = new List<skillType>(data.skillTypes);
        tooltip = data.tooltip;
        actionsNumber = data.actionsNumber;
        skillNumber = data.skillNumber;
        skillCumulative = data.skillCumulative;

        cost = data.cost;
        currentcost = data.cost;

        startSkillPosition = data.startSkillPosition;
        projectile = data.projectile;
        targeting = data.targeting;
        penetration = data.penetration;
        tracking = data.tracking;
        RangeAdjustment = data.RangeAdjustment;
        projectileType = data.projectileType;

        skillTarget = new List<Target>(data.skillTarget);
        XstartSkillPosition = data.XstartSkillPosition;
        YstartSkillPosition = data.YstartSkillPosition;
        projectileSpeed = data.projectileSpeed;
        afterdelay = data.afterdelay;
        beforedelay = data.beforedelay;
        range = data.range;
        aoetype = data.aoetype;
        Xaoe = data.Xaoe;
        Yaoe = data.Yaoe;
        aoecenter = data.aoecenter;

        // SkillData.cs 생성자 내 specialAoe 복사 부분 수정
        specialAoe = new AoeInfo[data.specialAoe.Length];
        for (int i = 0; i < data.specialAoe.Length; i++)
        {
            specialAoe[i] = data.specialAoe[i]; // 구조체이므로 값 복사로 충분
        }

        cooldown = data.cooldown;
        currentCooldown = data.cooldown;
        colldownSkill = data.colldownSkill;
        colldownTime = 0;


        basicValue = data.basicValue;
        increase = new UDictionary<IncreaseType, float>();
        foreach (var pair in data.increase)
        {
            increase.Add(pair.Key, pair.Value);
        }
        damageHit = data.damageHit;
        hitSpeed = data.hitSpeed;
        hitEffects = new List<HitEffectEntry>(data.hitEffects);
        //BuffEffects = new List<BuffEffect>(data.BuffEffects);
        //CCEffects = new List<CCEffect>(data.CCEffects);
        //conditionalEffects = new List<ConditionalEffect>(data.conditionalEffects);
        react = data.react;
        fourRotation = data.fourRotation; 
        reactTime = data.reactTime;

        // 연계 스킬 데이터이 있으면 추가하고 없으면 null을 입력
        AdditionalSkillData = data.AdditionalSkills != null
            ? new SkillData(data.AdditionalSkills, characterName, false)
        : null;

        summonCharacter = data.summonCharacter != null
    ? new Stats(data.summonCharacter, false, new())
    : null; // 소환수 캐릭터 (예: 소환수, 함정 등)

        if(summonCharacter != null)
        {
            for (int i = 0; i < data.summonCharacter.useSkill.Count; i++)
            {
                if (data.summonCharacter.useSkill[i] == null)
                {
                    summonCharacter.usingSkill.Add(new SkillData(null, summonCharacter.name, false));
                }
                else
                {
                    summonCharacter.usingSkill.Add(new SkillData(data.summonCharacter.useSkill[i], summonCharacter.name, false));
                    //AdditionalSkills
                }
            }
        }
    }

    public void StartCooldown()
    {
        colldownTime = currentCooldown;
    }

    public void ReduceCooldown(int amount)
    {
        colldownTime = Mathf.Max(0, colldownTime - amount);
    }
    
    public bool IsAvailable()
    {
        return colldownTime <= 0;
    }
}