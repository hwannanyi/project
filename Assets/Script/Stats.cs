using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
//using UnityEditor;
using UnityEngine;


[System.Serializable]
public class Stats
{
    public int characterNumber; // 캐릭터 번호

    public string name;  // 캐릭 이름
    public int maxhp;             // 최대체력
    public int hp;                // 체력
    public int shields;           // 보호막
    public int rage;              // 코스트
    public int risk;              // 리스크
    public int atk;               // 공격력
    public int def;               // 방어력
    public int damage_Defense; // 받피감
    public int damage_Plus; //주피증

    public int speed;             // 속도
    public int movespeed;         // 이속
    public int moveCount;         // 이동가능횟수
    public int NowMoveCount;         // 이동가능횟수
    public int trun;              // 보유 턴
    public Vector3 charPosition;  // 현제위치
    public Quaternion charRotation;  // 현재 방향
    public bool isdie;            // 죽음
    public Team team;             // 팀
    public bool summons;          // 소환수인가요?
    public Stats CharacterSummons; // 소환수 주인
    public List<Skill> useSkill;   // 사용스킬
    [NonSerialized] public List<SkillData> usingSkill = new();   // 사용스킬
    public List<PassiveSkill> passiveSkill = new();   // 패시브 스킬

    public Sprite characterillustration;
    public Sprite characterProfileillustration;
    public GameObject characterPrefab;

    

    public List<Debuffa> debuffEffects = new(); // 디버프 효과 리스트
    public List<Buffa> buffEffects = new(); // 버프 효과 리스트
    public List<CC> ccEffects = new();     //CC 효과

    public bool available = true; // 행동 여부
    public bool movable = true; // 이동 가능 여부

    public GameObject highlightEffect; // 인스펙터에서 하이라이트 오브젝트 할당

    public WorldHPBar HPbar; // HP바 오브젝트


    public bool isPatternEnd;
    public float gurd;

    /// <summary>
    /// /////////////////////
    /// </summary>
    public Condition_Hit lastHitType; // 마지막 충돌 타입(적중, 방어 등) 저장
    public SkillData lastHitSkillData; // 마지막 적중 스킬 데이터 저장

    public AIPattern aIPattern; // AI 패턴
    public void SetHighlight(bool isOn)
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(isOn);
    }

    [System.Serializable]
    public class PassiveSkill
    {
        public ConditionHit conditionHit; // 패시브 조건
        public SkillData passive = null; // 패시브 스킬
        public SkillAutoCast passiveTarget; // 패시브 타겟팅 정보
    }


    public bool isparrying; //패링중
    public float parryingTime; //패링 지속시간
    public bool hold; // 홀드중
    public List<HoldEffect> holdGauge =new();
    public List<MashingEffect> keyMashing = new();

    public Stats(Character data, bool die, List<SkillData> usingSkill)
    {
        isPatternEnd = false;

        characterNumber = 0;
        name = data.charactername;
        maxhp = data.maxhp;
        hp = data.maxhp;
        shields = 0;
        rage = data.rage;
        risk = 0;

        atk = data.atk;
        def = data.def;
        damage_Defense = 0;
        damage_Plus = 0;

        speed = data.speed;
        movespeed = data.movespeed;
        moveCount = data.moveCount;
        NowMoveCount = data.moveCount;
        trun = data.trun;

        this.charPosition = Vector3.zero;
        this.charRotation = Quaternion.identity;

        isdie = die;

        team = data.team;

        summons = data.summons;
        CharacterSummons = null;

        useSkill = data.useSkill;
        this.usingSkill = usingSkill;

        characterillustration = data.characterillustration;
        characterProfileillustration = data.characterProfileillustration;
        characterPrefab = data.characterPrefab;

        debuffEffects = new List<Debuffa>();
        buffEffects = new List<Buffa>();
        ccEffects = new List<CC>();

        available = true;
        movable = true;


        HPbar = null; // HP바 오브젝트

        //방어관련
        gurd = 0f; // 방어시간 초기화
        parryingTime = 0.05f;
        isparrying = false;
        hold = false;
        holdGauge = new();
        keyMashing = new();


        aIPattern = new AIPattern(data.skillQueue);
        //패시브 스킬 추가
        for (int i = 0; i < data.passiveSkill.Count; i++)
        {
            var src = data.passiveSkill[i];
            if (src == null)
            {
                // 패시브가 null이면 SkillData도 null로 생성, ConditionHit도 null로 저장
                passiveSkill.Add(new PassiveSkill
                {
                    passive = new SkillData(null, data.charactername),
                    conditionHit = null
                });
            }
            else
            {
                // 패시브가 있으면 SkillData로 복사 생성, ConditionHit도 복사
                passiveSkill.Add(new PassiveSkill
                {
                    passive = new SkillData(src.passive, data.charactername),
                    conditionHit = src.conditionHit,
                    passiveTarget = src.passiveTarget
                });
            }
        }
        lastHitType = Condition_Hit.none; // 초기값 설정

        //조건부분

        lastHitSkillData = null;
    }

    public void Rage_Overheating()
    {
        damage_Defense = risk;
    }

    // 패링 코루틴
    public IEnumerator ParryingCoroutine()
    {
        isparrying = true;//패링상태 활성화
        float time = parryingTime;//시간 시작
        while (time > 0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        isparrying = false; // 시간 종료와 함께 패링 끝
    }

    //홀드하고 있는 중인가?
    public bool IsHold()
    {
        switch (characterNumber)
        {
            case 0:
                return Input.GetKey(KeyCode.Alpha1);
            case 1:
                return Input.GetKey(KeyCode.Alpha2);
            case 2:
                return Input.GetKey(KeyCode.Alpha3);
            case 3:
                return Input.GetKey(KeyCode.Alpha4);
            default:
                // 기본값: LeftShift
                return false;
        }
    }
    

// 홀드시 모든 홀드 게이지가 줄어듬
public void Hold()
{

        for (int i = 0; i < holdGauge.Count; i++)
        {
            holdGauge[i].holdGauge -= Time.deltaTime;
        }
        // 0 이하 값 제거 (뒤에서부터)
        for (int i = holdGauge.Count - 1; i >= 0; i--)
        {
            if (holdGauge[i].holdGauge <= 0f)
                holdGauge.RemoveAt(i);
        }
    
}

// 키매싱 값이 0 이하가 되면 제거
public void Mashing(int num)
{
    if (characterNumber == num)
    {
        for (int i = 0; i < keyMashing.Count; i++)
        {
            keyMashing[i].keyMashingCount -= 1;
        }
        // 0 이하 값 제거 (뒤에서부터)
        for (int i = keyMashing.Count - 1; i >= 0; i--)
        {
            if (keyMashing[i].keyMashingCount <= 0)
                keyMashing.RemoveAt(i);
        }
    }
}

    public Transform GetCharacterTransform()
    {
        return characterPrefab != null ? characterPrefab.transform : null;
    }

    [System.Serializable]
    public class Debuffa
    {
        public Debuffs effect;
        public float Value;        // 기본 위력
        public int trun;       // 지속 시간
        public float time;       // 실시간 지속 시간
        public bool isApplied = false;
    }

    [System.Serializable]
    public class Buffa
    {
        public Buffs effect;
        public float Value;        // 기본 위력
        public int trun;       // 지속 시간
        public float time;       // 실시간 지속 시간
        public bool isApplied = false;
    }

    [System.Serializable]
    public class CC
    {
        public CCs effect;
        public float Value;        // 기본 위력
        public int trun;       // 지속 시간
        public float time;       // 실시간 지속 시간
        public bool isApplied = false;
    }
}

