using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
/*using UnityEditor;
using UnityEditor.Experimental.GraphView;*/
using UnityEngine;
using UnityEngine.TextCore.Text;
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
    public int MoveBoss; //보스 이동 스킬 번호
    public string tooltip;        // 스킬 설명

    public Skill AdditionalSkills; // 추가 스킬 (예: 연계기 등)


    public AutoCastInfoData AdditionalSkillData;

    [Header("스킬 시작시 시전되는 추가스킬")]
    public AutoCastInfoData StartAddSkills; // 스킬이 시작될때

    [Header("스킬 시작시 시전되는 추가스킬")]
    public AutoCastInfoData EndAddSkills; // 추가 스킬 스킬이 끝날때

    [NonSerialized] public Stats summonCharacter; // 소환수 캐릭터 (예: 소환수, 함정 등)


    public int actionsNumber;     // 스킬 행동 개수 (기본 1)
    public int skillNumber;       // 스킬의 갯수 (변형 가능)
    public int skillCumulative;   // 최대 충전 횟수 (기본 1)

    public int rageCost; // 코스트
    public int hpCost; // 체력 코스트
    public int rageCost_bas;            // 기본 코스트
    public int hpCost_bas;            // 기본 체력 코스트

    public StartSkillPosition startSkillPosition; // 스킬 시작 위치 (플레이어, 지정된 대상 등)
    public int XstartSkillPosition;
    public int YstartSkillPosition;

    public bool projectile;       // 투사체 여부 (false면 히트스캔)
    public bool targeting;        // 타겟팅 여부 (false면 논타겟팅)
    public bool penetration;     // 투사체의 관통 여부
    public bool tracking;       // 경로에 스킬 생성
    public bool RangeAdjustment;// 임의로 거리 조절 가능한
    public bool unlimitedRota;         // 자유 회전 가능한
    public projectileType projectileType;

    public List<Target> skillTarget; // 적중 가능한 대상 (자신, 아군, 적 등)
    public float projectileSpeed; // 투사체 속도 (히트스캔이면 0)  //////////(연출용)
    public float hitscantime;
    public bool skillTimeInf; // 스킬 지속시간 무한

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

    [Header("순차적 목표추적형")]
    public bool projectile_targetMove = false; // 순차적 목표추적형
    public bool startPos = false; // 시작지점
    public List<string> targetPos = new();
    public float nextDelay = 0; // 목표지점 재출발하는데 걸리는 딜레이
    public bool easing; // 
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);//이동 이징 유형
    public int repeat = 0; // 반복 횟수
    public bool rewind = false; // 반복할때 역순이동
    public float skillTime = 0; // 마지막 이동시 스킬이 사라지기까지 걸리는 시간

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

    public int skillCastCode; // 스킬 실행시 스킬을 찾기위한 임시코드 
    public Gurd gurd; //가드 정보

    public bool parryingT; //패링가능여부
    public List<HoldEffect> holdHit = new(); // 홀드 히트 정보
    public List<MashingEffect> keyMashingHit = new();

    public float skillPreview = 1f; // 범위 표시 시간 제한

    //정지연출
    public SFDType SFDtype = SFDType.none; // 정지해체 기준
    public float SFDtime = 0f; // 정지 선딜 시간
    public bool skillPreviewStop = false; // 스킬 시전 전에 정지


    [Header("패턴유형")]
    public PatternType patternType;

    [Header("circle")]
    public int ammo_circle;
    public float radius_circle;
    public bool isRadius_tracking_circle;
    public float delayTime_circle;
    public Vector3 position_circle;
    public bool isPosition_tracking_circle;
    public int count_circle;
    public bool isRandom_circle;

    [Header("straight")]
    public int ammo_straight; // 발사체 수
    public int interval_straight; // 발사 간격
    public Vector2 direction_straight; // 발사 방향
    public float radius_straight; // 발사 반경
    public bool isRadius_tracking_straight; // 반경 추적 여부
    public float delayTime_straight; // 발사 지연 시간
    public Vector3 position_straight; // 발사 위치
    public bool isPosition_tracking_straight; // 위치 추적 여부
    public int count_straight; // 반복 횟수
    public bool isRandom_straight; // 랜덤 여부

    [Header("조건검사")]
    public bool conditionCheck; // 조건검사 여부
    public List<StatusType> status = new();
    public bool statusNot; // 상태가 없어야 데미지 적용

    [Header("상태적용")]
    public List<StatusType> statusApply = new();
    public List<StatusType> statusRemove = new();

    public List<StatusEffect> statusEffects = new(); // 상태 효과 리스트

    public SkillData(Skill data, string characterName, int depth = 0)
    {
        if (data == null)
        {
            return;
        }

        if (depth > 3) // 최대 5단계까지만 허용
            return;

        skillCastCode = 0; // 스킬 실행시 스킬을 찾기위한 임시코드

        skillName = data.skillName;
        useCharacterName = characterName; // 직접 문자열을 할당

        skillIcon = data.skillIcon;
        SkillEffectIllustration = data.SkillEffectIllustration;
        SkillEffectPrefab = data.SkillEffectPrefab;

        passive = data.passive;
        skillTypes = new List<skillType>(data.skillTypes);
        MoveBoss = data.MoveBoss;
        tooltip = data.tooltip;

        rageCost = data.rageCost;
        hpCost = data.hpCost;
        rageCost_bas = data.rageCost;
        hpCost_bas = data.hpCost;

        startSkillPosition = data.startSkillPosition;
        projectile = data.projectile;
        targeting = data.targeting;
        penetration = data.penetration;
        tracking = data.tracking;
        RangeAdjustment = data.RangeAdjustment;
        unlimitedRota = data.unlimitedRota; // 자유 회전 가능한
        projectileType = data.projectileType;



        skillTarget = new List<Target>(data.skillTarget);
        XstartSkillPosition = data.XstartSkillPosition;
        YstartSkillPosition = data.YstartSkillPosition;
        projectileSpeed = data.projectileSpeed;
        hitscantime = data.hitscantime; // 히트스캔 시간
        skillTimeInf = data.skillTimeInf; // 스킬 지속시간 무한

        afterdelay = data.afterdelay;
        beforedelay = data.beforedelay;
        range = data.range;
        aoetype = data.aoetype;
        Xaoe = data.Xaoe;
        Yaoe = data.Yaoe;
        aoecenter = data.aoecenter;
        gurd = data.gurd; // 가드 정보
        parryingT = data.parryingHit; // 패링 가능 여부
        holdHit = data.holdHit;
        keyMashingHit = data.keyMashingHit;

        // SkillData.cs 생성자 내 specialAoe 복사 부분 수정
        specialAoe = new AoeInfo[data.specialAoe.Length];

        projectile_targetMove = data.projectile_targetMove;
        targetPos = new List<string>(data.targetPos);
        nextDelay = data.nextDelay;
        easing = data.easing;
        easingCurve = new AnimationCurve(data.easingCurve.keys);
        repeat = data.repeat;
        rewind = data.rewind;
        skillTime = data.skillTime;

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

        fourRotation = data.fourRotation; 


        skillPreview = data.skillPreview; // 범위 표시 시간 제한

        patternType = data.patternType;
        // circle
        ammo_circle = data.ammo_circle;
        radius_circle = data.radius_circle;
        isRadius_tracking_circle = data.isRadius_tracking_circle;
        delayTime_circle = data.delayTime_circle;
        position_circle = data.position_circle;
        isPosition_tracking_circle = data.isPosition_tracking_circle;
        count_circle = data.count_circle;
        isRandom_circle = data.isRandom_circle;
        // straight
        ammo_straight = data.ammo_straight;
        interval_straight = data.interval_straight;
        direction_straight = data.direction_straight;
        radius_straight = data.radius_straight;
        isRadius_tracking_straight = data.isRadius_tracking_straight;
        delayTime_straight = data.delayTime_straight;
        position_straight = data.position_straight;
        isPosition_tracking_straight = data.isPosition_tracking_straight;
        count_straight = data.count_straight;
        isRandom_straight = data.isRandom_straight;

        // 조건검사
        conditionCheck = data.conditionCheck;
        status = new List<StatusType>(data.status);
        statusNot = data.statusNot; // 상태가 없어야 데미지 적용
        // 상태적용
        statusApply = new List<StatusType>(data.statusApply);
        statusRemove = new List<StatusType>(data.statusRemove);
        statusEffects = new List<StatusEffect>(data.statusEffects); // 상태 효과 리스트

        // 연계 스킬 데이터이 있으면 추가하고 없으면 null을 입력
        // AdditionalSkills
        if (!string.IsNullOrEmpty(data.AdditionalSkills.skillName))
        {
            AdditionalSkillData = new AutoCastInfoData();
            AdditionalSkillData.condition = data.AdditionalSkills.condition;
            AdditionalSkillData.conditionHit = data.AdditionalSkills.conditionHit;
            AdditionalSkillData.targetrule = data.AdditionalSkills.targetrule;
            AdditionalSkillData.skillName = data.AdditionalSkills.skillName != null ? data.AdditionalSkills.skillName : null;
        }
        else
        {
            AdditionalSkillData = null;
        }

        // StartAddSkills
        if (!string.IsNullOrEmpty(data.StartAddSkills.skillName))
        {
            StartAddSkills = new AutoCastInfoData();
            StartAddSkills.condition = data.StartAddSkills.condition;
            StartAddSkills.conditionHit = data.StartAddSkills.conditionHit;
            StartAddSkills.targetrule = data.StartAddSkills.targetrule;
            StartAddSkills.skillName = data.StartAddSkills.skillName != null ? data.StartAddSkills.skillName : null;
        }
        else
        {
            StartAddSkills = null;
        }

        // EndAddSkills
        if (!string.IsNullOrEmpty(data.EndAddSkills.skillName))
        {
            EndAddSkills = new AutoCastInfoData();
            EndAddSkills.condition = data.EndAddSkills.condition;
            EndAddSkills.conditionHit = data.EndAddSkills.conditionHit;
            EndAddSkills.targetrule = data.EndAddSkills.targetrule;
            EndAddSkills.skillName = data.EndAddSkills.skillName != null ? data.EndAddSkills.skillName : null;
        }
        else
        {
            EndAddSkills = null;
        }

        summonCharacter = data.summonCharacter != null
    ? new Stats(data.summonCharacter, false, new())
    : null; // 소환수 캐릭터 (예: 소환수, 함정 등)

        if(summonCharacter != null)
        {
            for (int i = 0; i < data.summonCharacter.useSkill.Count; i++)
            {
                if (data.summonCharacter.useSkill[i] == null)
                {
                    summonCharacter.usingSkill.Add(new SkillData(null, summonCharacter.name, depth + 1));
                }
                else
                {
                    summonCharacter.usingSkill.Add(new SkillData(data.summonCharacter.useSkill[i], summonCharacter.name, depth + 1));


                    if (!string.IsNullOrEmpty(summonCharacter.useSkill[i].AdditionalSkills.skillName))
                    {
                        summonCharacter.usingSkill[i].AdditionalSkillData.skill =
                        GetSkillDataByName(summonCharacter.useSkill[i].AdditionalSkills.skillName, summonCharacter);
                    }

                    if (!string.IsNullOrEmpty(summonCharacter.useSkill[i].StartAddSkills.skillName))
                    {
                        summonCharacter.usingSkill[i].StartAddSkills.skill =
                        GetSkillDataByName(summonCharacter.useSkill[i].StartAddSkills.skillName, summonCharacter);
                    }

                    if (!string.IsNullOrEmpty(summonCharacter.useSkill[i].EndAddSkills.skillName))
                    {
                        summonCharacter.usingSkill[i].EndAddSkills.skill =
                        GetSkillDataByName(summonCharacter.useSkill[i].EndAddSkills.skillName, summonCharacter);
                    }
                    //AdditionalSkills
                }
            }


        }

        //정지연출
        SFDtype = data.SFDtype;
        SFDtime = data.SFDtime;
        skillPreviewStop = data.skillPreviewStop;
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

    //정지연출
    
/*    public IEnumerator SFD(SFDType sfdType, float delay) //Suspended for directing
    {
        Debug.Log("SFD Start");

        if(SFDType.none == sfdType)
            yield break;

        // delay만큼 대기
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Dictionary를 while문 안에서 new로 생성해야 합니다.
        Dictionary<SFDType, bool> sfdDurations = new()
        {
            { SFDType.moveUp, false },
            { SFDType.moveDo, false },
            { SFDType.skillE, false },
            { SFDType.moveUpDo, false}
        };
        SFDController.Instance.isSFD = true;
        Time.timeScale = 0f; // 게임 시간 정지
        while (true)
        {
            // 원하는 조건에 따라 반복문을 종료하세요.
            // 예시: 해당 키가 눌렸을 때 종료
            if (sfdDurations.ContainsKey(sfdType) && sfdDurations[sfdType])
            {
                break;
            }
            yield return null;
            // 매 프레임마다 키 입력을 갱신
            sfdDurations[SFDType.moveUp] = Input.GetKeyDown(KeyCode.UpArrow);
            sfdDurations[SFDType.moveDo] = Input.GetKeyDown(KeyCode.DownArrow);
            sfdDurations[SFDType.skillE] = Input.GetKeyDown(KeyCode.E);
            sfdDurations[SFDType.moveUpDo] = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
        }
        Time.timeScale = 1f; // 게임 시간 재개
        SFDController.Instance.isSFD = false;
    }*/
    


    public SkillData GetSkillDataByName(string name, Stats character)
    {
        if (character == null || character.useSkill == null)
            return null;

        // skillName이 name과 일치하는 skill 찾음
        return new SkillData(
            (character.useSkill.FirstOrDefault(skill => skill != null && skill.skillName == name)),
            character.name
            );
    }

}