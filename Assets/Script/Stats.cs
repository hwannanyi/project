using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class Stats
{
    public int characterNumber; // 캐릭터 번호

    public string name;  // 캐릭 이름
    public int maxhp;             // 최대체력
    public int hp;                // 체력
    public int shields;           // 보호막
    public int maxmp;             // 최대코스트
    public int mp;                // 코스트
    public int atk;               // 공격력
    public int def;               // 방어력
    public float damageReduction; // 받피감
    public float damageIncreased; //주피증

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

    public Stats(Character data, bool die, List<SkillData> usingSkill)
    {

        characterNumber = 0;
        name = data.charactername;
        maxhp = data.maxhp;
        hp = data.maxhp;
        shields = 0;
        maxmp = data.maxmp;
        mp = data.maxmp;
        atk = data.atk;
        def = data.def;
        damageReduction = 1f;
        damageIncreased = 1f;

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
                    passive = new SkillData(null, data.charactername, false),
                    conditionHit = null
                });
            }
            else
            {
                // 패시브가 있으면 SkillData로 복사 생성, ConditionHit도 복사
                passiveSkill.Add(new PassiveSkill
                {
                    passive = new SkillData(src.passive, data.charactername, false),
                    conditionHit = src.conditionHit,
                    passiveTarget = src.passiveTarget
                });
            }
        }
        lastHitType = Condition_Hit.none; // 초기값 설정

        //조건부분

        lastHitSkillData = null;
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
    }

    [System.Serializable]
    public class Buffa
    {
        public Buffs effect;
        public float Value;        // 기본 위력
        public int trun;       // 지속 시간
    }

    [System.Serializable]
    public class CC
    {
        public CCs effect;
        public float Value;        // 기본 위력
        public int trun;       // 지속 시간
    }
}

