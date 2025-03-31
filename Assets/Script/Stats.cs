using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class Stats
{
    public string name;  // 캐릭 이름
    public int maxhp;             // 최대체력
    public int hp;                // 체력
    public int maxmp;             // 최대코스트
    public int mp;                // 코스트
    public int atk;               // 공격력
    public int def;               // 방어력
    public int speed;             // 속도
    public int movespeed;         // 이속
    public int moveCount;         // 이동가능횟수
    public int trun;              // 보유 턴
    public Vector3 charPosition;  // 현제위치
    public Quaternion charRotation;  // 현재 방향
    public bool isdie;            // 죽음
    public Team team;             // 팀
    public List<Skill> useSkill;   // 사용스킬
    public Sprite characterillustration;
    public GameObject characterPrefab;


    public Stats(Character data, Vector3 charPosition, Quaternion charRotation, bool die)
    {
        name = data.charactername;
        maxhp = data.maxhp;
        hp = data.maxhp;
        maxmp = data.maxmp;
        mp = data.maxmp;
        atk = data.atk;
        def = data.def;
        speed = data.speed;
        movespeed = data.movespeed;
        moveCount = data.moveCount;
        trun = data.trun;

        this.charPosition = charPosition;
        this.charRotation = charRotation;

        isdie = die;

        team = data.team;
        useSkill = data.useSkill;
        characterillustration = data.characterillustration;
        characterPrefab = data.characterPrefab;
    }

    public Transform GetCharacterTransform()
    {
        return characterPrefab != null ? characterPrefab.transform : null;
    }
}

