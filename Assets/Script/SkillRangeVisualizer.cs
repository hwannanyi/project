using UnityEngine;
using System.Collections.Generic;
//using static UnityEditor.PlayerSettings;
//using UnityEditor;
using static UnityEngine.GraphicsBuffer;

public class SkillRangeVisualizer : MonoBehaviour
{
    public static SkillRangeVisualizer Instance { get; private set; }

    // 사거리 표시용
    public GameObject rangeHighlightPrefab;
    public int rangePoolSize = 100;
    private Queue<GameObject> rangePool = new Queue<GameObject>();
    private List<GameObject> activeRangeHighlights = new List<GameObject>();

    // 스킬 범위 표시용
    public GameObject areaHighlightPrefab;
    public int areaPoolSize = 100;
    private Queue<GameObject> areaPool = new Queue<GameObject>();
    private List<GameObject> activeAreaHighlights = new List<GameObject>();

    private bool isSkillRangeActive = false;
    private float Xaoe, Yaoe;
    private Vector3 casterPosition;

    private bool isNonTargetProjectileMode = false;
    private float noproXaoe, noproYaoe, range;
    private Vector3 noprocasterPosition;

    //마우스 좌표 저장
    private Vector3 prevMouseWorldTile = Vector3.positiveInfinity;

    //스킬 유형 표시
    public Sprite ProjectileLine;
    public Sprite projectileParabola;
    public Sprite whiteTile;

    public SkillManager skillManager;

    public GameObject maintarget;
    public SkillSave skillSave;

    void Awake()
    {
        skillManager = GetComponent<SkillManager>();
        skillSave = GetComponent<SkillSave>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < rangePoolSize; i++)
        {
            GameObject obj = Instantiate(rangeHighlightPrefab, transform);
            obj.SetActive(false);
            rangePool.Enqueue(obj);
        }
        // areaPool도 미리 생성
        for (int i = 0; i < areaPoolSize; i++)
        {
            GameObject obj = Instantiate(areaHighlightPrefab, transform);
            obj.SetActive(false);
            areaPool.Enqueue(obj);
        }
    }

    void Update()
    {
        // 타겟팅 스킬이고, 스킬 준비가 끝났으면 타겟 위치로 범위 표시
        if (skillSave.TeamSkill != null && isSkillRangeActive && skillManager.isSkillReadyFinal)
        {
            //if (skillSave.TeamSkill[0].skillData.selectedSkill.targeting)
            //ShowRectSkillRangeByTarget(maintarget, Xaoe, Yaoe);
        }

        if (isSkillRangeActive && !skillManager.isSkillReadyFinal)
        {
            Vector3 mouseWorldTile = GetMouseTileCenterPosition(); // 마우스의 월드 타일 중심 좌표
            if (mouseWorldTile != prevMouseWorldTile)
            {
                if(skillManager.selectedSkill.startSkillPosition != StartSkillPosition.player)
                {
                    ShowRectSkillRangeByMouse(casterPosition, Xaoe, Yaoe);
                }
                else
                {
                    ShowRectSkillRangeByMe(casterPosition, Xaoe, Yaoe);
                }
                //ShowRectSkillRangeByMouse(casterPosition, Xaoe, Yaoe);
                prevMouseWorldTile = mouseWorldTile;
            }
        }

        if (isNonTargetProjectileMode && !skillManager.isSkillReadyFinal)
        {
            Vector3 mouseWorldTile = GetMouseTileCenterPosition(); // 마우스의 월드 타일 중심 좌표
            if (mouseWorldTile != prevMouseWorldTile)
            {
                ShowNonTargetProjectileRange(casterPosition, Xaoe, Yaoe, range);
                prevMouseWorldTile = mouseWorldTile;
            }
        }
    }


    // 논타겟 투사체 사거리 표시 (Xaoe*Yaoe*range 크기, 4방향)
    public void ShowNonTargetProjectileRange(Vector3 casterPosition, float Xaoe, float Yaoe, float range)
    {
        HideAreaSkillRange();






        // 1. 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorld = GetMouseTileCenterPosition();

        // 2. 방향 벡터 계산 (XZ 평면)
        Vector3 dir = mouseWorld - casterPosition;
        dir.y = 0;

        // 3. 4방향 중 가장 가까운 방향 결정
        Vector3[] directions = {
        Vector3.forward,   // +Z (위)
        Vector3.back,      // -Z (아래)
        Vector3.left,      // -X (왼쪽)
        Vector3.right      // +X (오른쪽)
    };
        float maxDot = float.MinValue;
        Vector3 mainDir = Vector3.forward;
        foreach (var d in directions)
        {
            float dot = Vector3.Dot(dir.normalized, d);
            if (dot > maxDot)
            {
                maxDot = dot;
                mainDir = d;
            }
        }

        // 4. 사각형 범위 타일 구하기
        List<Vector3> tiles = new List<Vector3>();
        Vector3 start = new Vector3(Mathf.Floor(casterPosition.x) + 0.5f, casterPosition.y, Mathf.Floor(casterPosition.z) + 0.5f);

        for (int i = 1; i <= range; i++)
        {
            Vector3 center = start + mainDir * i;
            var areaTiles = GetRectAreaTiles(center, Xaoe, Yaoe);
            foreach (var t in areaTiles)
            {
                if (!tiles.Contains(t))
                    tiles.Add(t);
            }
        }

        // 5. 하이라이트 표시 (areaHighlightPrefab 사용)
        // mainDir에 따라 Z축 회전 각도 결정
        float angle = 0f;
        if (mainDir == Vector3.forward) angle = 90f;
        else if (mainDir == Vector3.right) angle = 0f;
        else if (mainDir == Vector3.back) angle = -90;
        else if (mainDir == Vector3.left) angle = 180f;

        foreach (var pos in tiles)
        {
            GameObject highlight = GetFromAreaPool();
            // SpriteRenderer 교체
            var sr = highlight.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && ProjectileLine != null)
            {
                sr.sprite = ProjectileLine;
            }
            highlight.transform.position = pos;
            highlight.transform.rotation = Quaternion.Euler(90, 0, angle);
            highlight.SetActive(true);
            activeAreaHighlights.Add(highlight);
        }
    }

    // 그 외 스킬: BFS로 사거리, 마우스 포인터를 중심으로 사각형 범위 표시
    public void ShowNormalSkillRange(Vector3 casterPosition, float range)
    {
        HideSkillRange();
        var bfsTiles = GetBFSTiles(casterPosition, range);
        foreach (var pos in bfsTiles)
        {
            GameObject highlight = GetFromPool();
            highlight.transform.position = pos;
            highlight.transform.rotation = Quaternion.Euler(90, 0, 0); // x축 90도 회전
            highlight.SetActive(true);
            activeRangeHighlights.Add(highlight);
        }
    }

    public void HideSkillRange()
    {

        foreach (var obj in activeRangeHighlights)
        {
            obj.SetActive(false);
            rangePool.Enqueue(obj);
        }
        activeRangeHighlights.Clear();
    }

    // 투사체 경로 + 투사체 크기만큼 타일 반환
    public List<Vector3> GetProjectileSkillTiles(Vector3 start, Vector3 direction, float range, float Xaoe, float Yaoe)
    {
        HashSet<Vector3> result = new HashSet<Vector3>();
        Vector3 pos = start;
        Vector3 dir = direction.normalized;

        for (int i = 0; i < range; i++)
        {
            pos += new Vector3(dir.x * Xaoe, 0, dir.z * Yaoe);
            // 투사체 크기만큼 사각형 범위 커버
            foreach (var tile in GetRectAreaTiles(pos, Xaoe, Yaoe))
                result.Add(tile);
        }
        return new List<Vector3>(result);
    }

    // BFS로 사거리 내 타일 반환 (격자 단위)
    public List<Vector3> GetBFSTiles(Vector3 start, float range)
    {
        HashSet<Vector3> visited = new HashSet<Vector3>();
        Queue<(Vector3 pos, int dist)> queue = new Queue<(Vector3, int)>();
        Vector3 startGrid = new Vector3(
            Mathf.Round(start.x),
            start.y,
            Mathf.Round(start.z)
        );
        queue.Enqueue((startGrid, 0));
        visited.Add(startGrid);

        Vector3[] dirs = new Vector3[]
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        while (queue.Count > 0)
        {
            var (cur, dist) = queue.Dequeue();
            if (dist >= range) continue;
            foreach (var d in dirs)
            {
                Vector3 next = new Vector3(
                    Mathf.Round(cur.x + d.x),
                    cur.y,
                    Mathf.Round(cur.z + d.z)
                );
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue((next, dist + 1));
                }
            }
        }
        return new List<Vector3>(visited);
    }

    // 중심점 기준 사각형 범위 타일 반환
    public List<Vector3> GetRectAreaTiles(Vector3 center, float Xaoe, float Yaoe)
    {
        List<Vector3> result = new List<Vector3>();
        float halfX = Xaoe / 2;
        float halfY = Yaoe / 2;

        for (float dx = -halfX; dx < halfX; dx++)
        {
            for (float dz = -halfY; dz < halfY; dz++)
            {
                float x = center.x + dx;
                float y = center.y;
                float z = center.z + dz;
                Vector3 tilePos = new Vector3(x, y, z);
                if (!result.Contains(tilePos))
                    result.Add(tilePos);
            }
        }
        return result;
    }

    private GameObject GetFromPool()
    {
        if (rangePool.Count > 0)
            return rangePool.Dequeue();
        GameObject obj = Instantiate(rangeHighlightPrefab, transform);
        obj.SetActive(false);
        return obj;
    }
    // areaPool에서 오브젝트 가져오기
    private GameObject GetFromAreaPool()
    {
            if (areaPool.Count > 0)
                return areaPool.Dequeue();
            GameObject obj = Instantiate(areaHighlightPrefab, transform);
            obj.SetActive(false);
            return obj;
    }

    // 월드 좌표를 타일 중심으로 변환
    private Vector3 GetMouseTileCenterPosition()
    {
        Vector3 worldPos = GetMouseWorldPosition();

        // 타일 인덱스 구하기
        int centerX = Mathf.RoundToInt(worldPos.x);
        int centerZ = Mathf.RoundToInt(worldPos.z);
        /*
                // 타일 중심 좌표로 변환
                float centerX = tileX + 0.5f;
                float centerZ = tileZ + 0.5f;
        */

        return new Vector3(centerX, 0, centerZ);

    }

    public void ShowRectSkillRangeByMouse(Vector3 casterPosition, float Xaoe, float Yaoe)
    {
        HideAreaSkillRange();

        // 1. 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorld = GetMouseTileCenterPosition();

        // 2. 방향 벡터 계산 (XZ 평면)
        Vector3 dir = mouseWorld - casterPosition;
        dir.y = 0;

        // 3. 4방향 중 가장 가까운 방향 결정
        Vector3[] directions = {
        Vector3.forward,   // +Z (위)
        Vector3.back,      // -Z (아래)
        Vector3.left,      // -X (왼쪽)
        Vector3.right      // +X (오른쪽)
    };
        float maxDot = float.MinValue;
        Vector3 mainDir = Vector3.forward;
        foreach (var d in directions)
        {
            float dot = Vector3.Dot(dir.normalized, d);
            if (dot > maxDot)
            {
                maxDot = dot;
                mainDir = d;
            }
        }

        // 마우스 위치를 타일 좌표로 스냅
        Vector3 snappedMouse = new Vector3(Mathf.Floor(mouseWorld.x)+0.5f, casterPosition.y, Mathf.Floor(mouseWorld.z) + 0.5f);


        // 4. 범위 중심점 계산 (시전자에서 mainDir 방향으로 1칸 이동)
        Vector3 center = snappedMouse;

        // 5. 사각형 범위 타일 구하기
        var tiles = GetRectAreaTiles(center, Xaoe, Yaoe);

        // 6. 하이라이트 표시 (areaPool, activeAreaHighlights 사용)
        foreach (var pos in tiles)
        {
            GameObject highlight = GetFromAreaPool();
            highlight.transform.position = pos;
            highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
            highlight.SetActive(true);
            activeAreaHighlights.Add(highlight);
        }
    }

    //타겟팅 스킬 범위의 타겟추적
    public void ShowRectSkillRangeByTarget(GameObject target, float Xaoe, float Yaoe)
    {
        if (target == null) return;

        HideAreaSkillRange();

        // 타겟의 위치를 타일 중심으로 스냅
        Vector3 targetPos = target.transform.position;
        Vector3 tileCenter = new Vector3(
            Mathf.Floor(targetPos.x) + 0.5f,
            targetPos.y,
            Mathf.Floor(targetPos.z) + 0.5f
        );

        // 사각형 범위 타일 구하기
        var tiles = GetRectAreaTiles(tileCenter, Xaoe, Yaoe);

        // 하이라이트 표시
        foreach (var pos in tiles)
        {
            GameObject highlight = GetFromAreaPool();
            highlight.transform.position = pos;
            highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
            highlight.SetActive(true);
            activeAreaHighlights.Add(highlight);
        }
    }

    //자신에게서 써지는 스킬
    public void ShowRectSkillRangeByMe(Vector3 casterPosition, float Xaoe, float Yaoe)
    {
        HideAreaSkillRange();

        // 시전자 위치를 타일 중심으로 스냅
        Vector3 tileCenter = new Vector3(
            Mathf.Floor(casterPosition.x) + 0.5f,
            casterPosition.y,
            Mathf.Floor(casterPosition.z) + 0.5f
        );

        // 사각형 범위 타일 구하기
        var tiles = GetRectAreaTiles(tileCenter, Xaoe, Yaoe);

        // 하이라이트 표시
        foreach (var pos in tiles)
        {
            GameObject highlight = GetFromAreaPool();
            highlight.transform.position = pos;
            highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
            highlight.SetActive(true);
            activeAreaHighlights.Add(highlight);
        }
    }

    // 스킬 범위 하이라이트 숨기기
    public void HideAreaSkillRange()
    {
        SkillManager.Instance.validReactTargets.Clear(); // 여기서 한 번만 비우기
        foreach (var obj in activeAreaHighlights)
        {
            // 비활성화 전에 Sprite를 whiteTile로 변경
            var sr = obj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && whiteTile != null)
            {
                sr.sprite = whiteTile;
            }
            obj.SetActive(false);
            areaPool.Enqueue(obj);
        }
        activeAreaHighlights.Clear();
    }

    // 마우스 위치를 월드 좌표로 변환 (바닥이 y=0이라고 가정)
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    // 스킬 범위 표시 시작
    public void StartSkillRangePreview(Vector3 casterPos, float xRange, float yRange)
    {
        casterPosition = casterPos;
        Xaoe = xRange;
        Yaoe = yRange;
        isSkillRangeActive = true;
    }

    // 타겟팅 스킬의 타겟 결정
    public void StartSkillTargetRangePreview(GameObject target)
    {
        maintarget = target;

    }

    public void StartNonTargetProjectileRange(Vector3 casterPos, float xAoe, float yAoe, float skillRange)
    {
        StopSkillRangePreview();
        isNonTargetProjectileMode = true;
        casterPosition = casterPos;
        Xaoe = xAoe;
        Yaoe = yAoe;
        range = skillRange;
    }

    public void StopNonTargetProjectileRange()
    {
        isNonTargetProjectileMode = false;
        prevMouseWorldTile = Vector3.positiveInfinity;
        HideAreaSkillRange();
    }

    // 스킬 범위 표시 종료
    public void StopSkillRangePreview()
    {
        isSkillRangeActive = false;
        HideSkillRange();
    }
}
