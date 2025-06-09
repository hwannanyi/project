using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class CharacterMovement : MonoBehaviour
{

    public int characterNumber;
    public int moveSpeed = 5;  // 이동 속도
    private Vector3 targetPosition;  // 목표 위치
    private Vector3 startPosition;   // 이동 시작 위치
    public bool isMoving = false;  // 이동 중인지 여부
    public bool isBlocked = false;  // 타일에 부딪히면 이동 차단
    private Coroutine moveCoroutine;  // 이동 코루틴을 저장할 변수
    //public static event Action<bool> turnEnd; // 턴 종료시 발생 이벤트
    public int moveRange = 5; // 최대 이동 거리 제한
    public int moveCount;     // 이동가능횟수

    public GameObject highlightPrefab; // 하이라이트 프리팹
    private List<GameObject> highlights = new List<GameObject>();
    public bool isShowMoveHighlights;

    void Start()
    {  
        targetPosition = transform.position;  // 시작 위치 설정
        startPosition = transform.position;   // 초기 위치 설정
        CharacterSelection.selectedCharacterIndex = -1;
    }
    void Awake()
    {
        isShowMoveHighlights = false; // 초기값 설정
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (CharacterStats.Instance.characters.Contains(gameObject))
            {
                int index = CharacterStats.Instance.characters.IndexOf(gameObject);
                characterNumber = index;
                moveSpeed = CharacterStats.Instance.characterList[index].speed;
                moveRange = CharacterStats.Instance.characterList[index].movespeed;
                CharacterStats.Instance.characterList[index].NowMoveCount = CharacterStats.Instance.characterList[index].moveCount;

            }
        }
        //UI클릭시 클릭 무시
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        int indexnumber = CharacterSelection.selectedCharacterIndex;
        if(indexnumber < 0 || indexnumber >= CharacterStats.Instance.characters.Count || SkillManager.Instance.selectedSkill != null || SkillManager.Instance.isSkillReadyFinal)
        {
            ClearHighlights(); // 선택 해제 시 하이라이트 제거
            return; // 유효하지 않은 인덱스인 경우 아무 작업도 하지 않음
        }
        int nowmoveCount = CharacterStats.Instance.characterList[indexnumber].NowMoveCount;
        if (characterNumber == indexnumber)
        {
            if (SkillManager.Instance.waitingForResponse == true)
            {
                return;
            }

            if (!isMoving) { 
            // 현재 위치
            Vector2Int startTile = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

            // 이동 가능 타일 계산
            List<Vector2Int> movableTiles = GetMovableTiles(startTile, moveRange);

            // 하이라이트 표시
            ShowMoveHighlights(movableTiles);
            }

            // 이동 중이 아니고, 막히지 않았을 때만 마우스 클릭을 처리
            if (nowmoveCount > 0 &&!isMoving && !isBlocked && Input.GetMouseButtonDown(1))
            {
                // 대응단계 확인
                /*if (TurnManager.Instance.IsInReactPhase())
                {
                    Stats myStats = CharacterStats.Instance.characterList[characterNumber];

                    if (SkillManager.Instance.GetRespondingCharacter() != myStats)
                    {
                        Debug.Log("[React] 현재 대응 중인 캐릭터가 아닙니다.");
                        return;
                    }

                    if (SkillManager.Instance.HasMovedInReactPhase() || SkillManager.Instance.HasAlreadyReacted())
                    {
                        Debug.Log("[React] 이미 대응 행동을 했습니다.");
                        return;
                    }

                    SkillManager.Instance.MarkReactMove();
                }*/

                // [수정됨] Perspective 카메라 대응: 마우스 위치를 정확히 가져오기 위한 Raycast 방식
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y = 0 기준 평면
                float enter;
                Vector3 mousePosition = transform.position;
                if (groundPlane.Raycast(ray, out enter))
                {
                    mousePosition = ray.GetPoint(enter);
                    mousePosition.y = 0f;
                }
                Vector3 roundedTarget = new Vector3(Mathf.Round(mousePosition.x), 0f, Mathf.Round(mousePosition.z));

                // 현재 위치
                Vector2Int startTile = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

                // 목표 위치
                Vector2Int targetTile = new Vector2Int(Mathf.RoundToInt(roundedTarget.x), Mathf.RoundToInt(roundedTarget.z));

                // 이동 가능 타일 계산
                List<Vector2Int> movableTiles = GetMovableTiles(startTile, moveRange);

                // 이동 가능 타일이 아니면 이동하지 않음
                if (!movableTiles.Contains(targetTile))
                {
                    return;
                }

                targetPosition = roundedTarget;
                startPosition = transform.position;  // 이동 시작 위치 저장
                moveCoroutine = StartCoroutine(MoveToTarget());  // 이동을 코루틴으로 처리
            }
        }
        else
        {
            // 선택 해제 시 하이라이트 제거
                ClearHighlights();
        }

    }



    /*public void SendSignal(bool end)
    {
        Debug.Log("신호 보냄: " + end);
        turnEnd?.Invoke(end); // 이벤트 호출 (신호 보내기)
    }*/

    // 목표 위치로 이동하는 코루틴
    private IEnumerator MoveToTarget()
    {
        isMoving = true;  // 이동 중 상태로 설정

        // 목표 위치로 이동할 때까지
        while (Vector3.Distance(transform.position, targetPosition) > 0.05f && !isBlocked)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;  // 다음 프레임까지 대기
        }

        // 이동 완료 후 정확한 목표 위치로 설정
        transform.position = targetPosition;
        //SendSignal(true)
        isMoving = false;  // 이동 완료 후 이동 가능 상태로 변경
        PositionUpdate();

    }

    // 타일에 부딪혔을 때 호출되는 메서드
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 타일인 경우
        if (other.CompareTag("Tile"))
        {
            isBlocked = true;  // 이동을 막음
            isMoving = false;  // 이동 중 상태 해제
            if (moveCoroutine != null)  // 이동 중이면 코루틴을 중지
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
                StopCoroutine(moveCoroutine);  // 이동 중단
            }

            // 현재 위치에서 가장 가까운 타일로 이동
            Vector3 nearestTile = GetNearestTile(transform.position);
            StartCoroutine(MoveToTargetFromCurrent(nearestTile));  // 가장 가까운 타일로 이동
            PositionUpdate();
            //SendSignal(true);
        }
    }
    
    // 가장 가까운 타일을 찾는 메서드
    private Vector3 GetNearestTile(Vector3 currentPosition)
    {
        // 현재 위치에서 가장 가까운 타일의 좌표를 계산
        float x = Mathf.Round(currentPosition.x);
        float y = Mathf.Round(currentPosition.z);

        return new Vector3(x, 0f, y);  // 가장 가까운 타일로 반환
    }

    // 가장 가까운 타일로 이동하는 코루틴
    private IEnumerator MoveToTargetFromCurrent(Vector3 nearestTile)
    {
        isMoving = true;  // 이동 중 상태로 설정

        // 가장 가까운 타일로 이동할 때까지
        while (Vector3.Distance(transform.position, nearestTile) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nearestTile, moveSpeed * Time.deltaTime);
            yield return null;  // 다음 프레임까지 대기
        }

        transform.position = nearestTile;  // 정확한 목표 위치로 설정
        isMoving = false;  // 이동 완료 후 이동 가능 상태로 변경
    }

    // 충돌이 끝났을 때 이동을 재개하도록 설정
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            isBlocked = false;  // 이동 차단 해제
        }
    }

    // 장애물 체크 함수 예시 (장애물 레이어 등으로 구현)
    private bool IsBlockedTile(Vector2Int pos)
    {
        // 예시: Physics.OverlapBox 등으로 해당 위치에 장애물이 있는지 체크
        // 실제 구현은 프로젝트 상황에 맞게 수정
        Collider[] cols = Physics.OverlapBox(new Vector3(pos.x, 0, pos.y), Vector3.one * 0.4f, Quaternion.identity, LayerMask.GetMask("Obstacle"));
        return cols.Length > 0;
    }

    // BFS로 이동 가능한 타일 계산
    public List<Vector2Int> GetMovableTiles(Vector2Int start, int moveRange)
    {
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(Vector2Int pos, int dist)>();
        var result = new List<Vector2Int>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();
            if (dist > moveRange) continue;
            result.Add(pos);

            foreach (var dir in dirs)
            {
                var next = pos + dir;
                if (visited.Contains(next)) continue;
                if (IsBlockedTile(next)) continue;

                visited.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }
        return result;
    }

    private void PositionUpdate()
    {
        ClearHighlights();
        CharacterStats.Instance.characterList[characterNumber].NowMoveCount--;
        CharacterStats.Instance.characterList[characterNumber].charPosition = transform.position;
    }


    public void ShowMoveHighlights(List<Vector2Int> movableTiles)
    {
        if(isShowMoveHighlights)
        {
            return;
        }
        ClearHighlights();

        foreach (var tile in movableTiles)
        {
            Vector3 pos = new Vector3(tile.x, 0.01f, tile.y); // 살짝 띄워서 z-fighting 방지
            GameObject highlight = Instantiate(highlightPrefab, pos, Quaternion.Euler(90, 0, 0));
            highlights.Add(highlight);
        }
        isShowMoveHighlights = true;
    }

    public void ClearHighlights()
    {
        isShowMoveHighlights = false; // 하이라이트 표시 상태 초기화
        foreach (var go in highlights)
        {
            Destroy(go);
        }
        highlights.Clear();
    }

    
}
