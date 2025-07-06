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
    public List<Skill> useSkill;   // 사용스킬
    public List<SkillData> usingSkill = new();   // 사용스킬
    public Sprite characterillustration;
    public Sprite characterProfileillustration;
    public GameObject characterPrefab;

    public List<Debuffa> debuffEffects = new(); // 디버프 효과 리스트
    public List<Buffa> buffEffects = new(); // 버프 효과 리스트
    public List<CC> ccEffects = new();     //CC 효과

    public bool available = true; // 행동 여부
    public bool movable = true; // 이동 가능 여부

    public GameObject highlightEffect; // 인스펙터에서 하이라이트 오브젝트 할당


    public AIPattern aIPattern; // AI 패턴
    public void SetHighlight(bool isOn)
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(isOn);
    }

    public Stats(Character data, Vector3 charPosition, Quaternion charRotation, bool die, List<SkillData> usingSkill)
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

        this.charPosition = charPosition;
        this.charRotation = charRotation;

        isdie = die;

        team = data.team;
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

