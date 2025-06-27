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
    public List<Skill> useSkill = new List<Skill> { null, null, null, null, null };   // 사용스킬
    public Sprite characterillustration;
    public Sprite characterProfileillustration;
    public GameObject characterPrefab;
}


    
