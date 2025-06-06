using UnityEngine;
using System.Collections.Generic;

public class SkillRangeVisualizer : MonoBehaviour
{
    [Header("하이라이트 프리팹 및 풀 크기")]
    public GameObject tileHighlightPrefab;
    public int poolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeHighlights = new List<GameObject>();

    public static SkillRangeVisualizer Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        // 오브젝트 풀 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(tileHighlightPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // BFS로 구한 범위 타일 하이라이트 표시
    public void ShowTiles(List<Vector3> tilePositions)
    {
        HideSkillRange();

        foreach (var pos in tilePositions)
        {
            GameObject highlight = GetFromPool();
            highlight.transform.position = pos;
            highlight.SetActive(true);
            activeHighlights.Add(highlight);
        }
    }

    // 논타겟 투사체 경로 타일 하이라이트 표시
    public void ShowProjectilePathTiles(List<Vector3> pathPositions)
    {
        HideSkillRange();

        foreach (var pos in pathPositions)
        {
            GameObject highlight = GetFromPool();
            highlight.transform.position = pos;
            highlight.SetActive(true);
            activeHighlights.Add(highlight);
        }
    }

    // 풀에서 오브젝트 꺼내기
    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        // 풀에 남은 게 없으면 새로 생성 (예외 상황)
        GameObject obj = Instantiate(tileHighlightPrefab, transform);
        obj.SetActive(false);
        return obj;
    }

    // 모든 하이라이트 숨기기(풀로 반환)
    public void HideSkillRange()
    {
        foreach (var obj in activeHighlights)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        activeHighlights.Clear();
    }

    // (참고) 논타겟 투사체 경로 타일 계산 예시
    public List<Vector3> GetProjectilePathTiles(Vector3 start, Vector3 direction, int range, float tileSize)
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 pos = start;
        Vector3 dir = direction.normalized * tileSize;

        for (int i = 0; i < range; i++)
        {
            pos += dir;
            // 격자 정렬
            result.Add(new Vector3(Mathf.Round(pos.x / tileSize) * tileSize, pos.y, Mathf.Round(pos.z / tileSize) * tileSize));
        }
        return result;
    }
}