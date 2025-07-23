using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class Stage : ScriptableObject
{
    [Header("스테이지 프로필")]
    public string stageName; // 스테이지 이름
    [TextArea] public string stageDescription; // 스테이지 설명
    public Sprite stageImage; // 스테이지 이미지

    [Header("스테이지 번호")]
    public int stagenumber; // 스테이지 번호

    [Header("참여인원")]
    public int participants = 1; // 참여자 수
    public List<Vector2> startPositions; // 시작 위치 리스트

    [Header("맵(미구현)")]
    public int[][] map; // 시작 위치 리스트

    [Header("스테이지 적 설정")]
    public List<EnemyData> enemyDatalist; // 적 데이터 리스트


}
[System.Serializable]
public class EnemyData
{
    public string enemyName; // 적 이름
    public Vector2 position; // 적 시작위치
}