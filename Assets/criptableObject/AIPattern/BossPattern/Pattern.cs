using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pattern", menuName = "Scriptable Objects/Pattern")]
public class Pattern : ScriptableObject
{
    public enum PatternType
    {
        circle,
        straight,
        repeat,
    }

    public Skill skill;

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


    [Header("repeat")]
    public List<SkillQueue> skill_repeat;
    public int count_repeat = 1;
    public int count_repeat_Random = 0;
    public bool isRandom_repeat;
    public bool isRandom_index; //랜덤순서
    public bool isindex_mix; //섞기
    public int Random_index = 0; //랜덤 시작값
    public bool at_once = false; //한번에 다발사
    public List<int> at_onces = new List<int>(); //사용된 인덱스 저장용

    [Header("조건")]
    public bool useCond; // 조건 사용 여부
    public Condition condition; // 조건
    public StatusType statusType; // 상태 유형
}
