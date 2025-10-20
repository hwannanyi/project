using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public enum VictoryRule
{
    killAll, // 모든 적 처치
    Survive, // 일정 시간 생존
    ReachObjective, // 목표 지점 도달
    story // 스토리 진행
}

public enum StoryTimingType
{
    hp, // 체력이 일정 비율 이하
    kill, // 처치됨
    turnCount, // 일정턴
    turn, // 특정 턴
    TuToMove, // 이동(튜토전용)
    TuToskillcast, // 스킬 시전(튜토전용)
}

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class Stage : ScriptableObject
{
    [Header("스테이지 프로필")]
    public string stageName; // 스테이지 이름
    [TextArea] public string stageDescription; // 스테이지 설명
    public Sprite stageImage; // 스테이지 이미지

    [Header("스토리")]
    public string ID;
    [Tooltip("조건과 대상")]
    public List<StoryTiming> storyTiming; // 스토리 등장타이밍

    [Header("스테이지 번호")]
    public int stagenumber; // 스테이지 번호

    [Header("참여인원")]
    public int participants = 1; // 참여자 수
    public List<Vector2> startPositions; // 시작 위치 리스트
    public Character P1;
    public Character P2;


    [Header("맵(미구현)")]
    public int[][] map; // 시작 위치 리스트
    public int mapWidth; // 맵 너비
    public int mapHeight; // 맵 높이
    public Sprite mapSprite; // 맵 스프라이트
    public Sprite mapBackGround; // 맵 배경 스프라이트

    [Header("스테이지 적 설정")]
    public List<EnemyData> enemyDatalist; // 적 데이터 리스트

    [Header("승리조건")]
    public VictoryRule clear; 

    [Header("패배조건")]
    public VictoryRule feil;

    [Header("턴")]
    public List<bool> isServeTurn;
    public List<int> turnOrder; // 행동횟수
}
[System.Serializable]
public class EnemyData
{
    public string enemyName; // 적 이름
    public Vector2 position; // 적 시작위치
}

[System.Serializable]
public class StoryTiming
{
    public string Target; // 적 이름
    public StoryTimingType storyTimingType; // 스토리 등장타이밍 타입
    public float value; // 해당 조건의 값 (예: 체력 비율, 처치 수, 턴 수 등)
    public bool isPopUp; // 팝업스토리창 여부
    public string ID; // 불러올 스토리 ID
}