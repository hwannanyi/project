using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Data.SqlTypes;


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
    public SpriteRenderer spriteRenderer;

    public ChRotation lookRotation; // 바라보는 방향
    public GameObject lookRota;

    //SFD
    public SFDController SFD;

    void Start()
    {  
        targetPosition = transform.position;  // 시작 위치 설정
        startPosition = transform.position;   // 초기 위치 설정
        CharacterSelection.selectedCharacterIndex = -1;
        SFD = SFDController.Instance;
        lookRotation = CharacterStats.Instance.GetStats(gameObject).charRotation;

    }
    void Awake()
    {
        isShowMoveHighlights = false; // 초기값 설정
        StartCoroutine(TrySetCharacterData());
    }



    private IEnumerator TrySetCharacterData()
    {
        while (!CharacterStats.Instance.characters.Contains(gameObject))
        {
            yield return null; // 다음 프레임까지 대기
        }
        int index = CharacterStats.Instance.characters.IndexOf(gameObject);
        characterNumber = index;
        CharacterStats.Instance.characterList[index].characterNumber = index;
        moveSpeed = CharacterStats.Instance.characterList[index].speed;
        moveRange = CharacterStats.Instance.characterList[index].movespeed;
        CharacterStats.Instance.characterList[index].NowMoveCount = CharacterStats.Instance.characterList[index].moveCount;

        CharacterStats.Instance.characterList[characterNumber].charPosition = transform.position;
    }

    void Update()
    {
        try
        {
            if (StoryManager.instance.isStoryActive)
            {
                return; // 모든 입력 무시
            }
        }
        catch
        {
            return; // StoryManager를 못불려와도 모든입력무시
        }
        var stats = CharacterStats.Instance;
        var character = stats.GetStats(gameObject);


        int indexnumber = CharacterSelection.selectedCharacterIndex;
        if(indexnumber < 0 || indexnumber >= CharacterStats.Instance.characters.Count || SkillManager.Instance.selectedSkill != null 
            || SkillManager.Instance.isSkillReadyFinal)
        {

            return; // 유효하지 않은 인덱스인 경우 아무 작업도 하지 않음
        }
        
        //int nowmoveCount = CharacterStats.Instance.characterList[indexnumber].NowMoveCount;

        if (characterNumber == indexnumber)
        {
            //이동불가 상태라면 이동금지
            //스킬 시전중 이동 금지
            if (character.movable == false ||
                SkillManager.Instance.isCastingSkill)
            {
                return;
            }

/*            // 이동 중이 아니고, 차단되지 않았을 때만 하이라이트 표시
            if (!isMoving && !isBlocked)
            {
                ClearHighlights();
                ShowMoveHighlights();
            }*/

/*            // --- [추가] 마우스 방향에 따라 x축 반전 ---
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y = 0 기준 평면
            float enter;
            Vector3 mousePosition = transform.position;
            if (groundPlane.Raycast(ray, out enter))
            {
                mousePosition = ray.GetPoint(enter);
                mousePosition.y = 0f;
            }
            //*/

            if (SkillManager.Instance.waitingForResponse == true)
            {
                return;
            }

            /*// 마우스 우클릭 시 4방향 중 마우스 방향으로 한 칸 이동
            if (!isMoving && !isBlocked && Input.GetMouseButtonDown(1))
            {
                if (groundPlane.Raycast(ray, out enter))
                {
                    Vector3 mousePos = ray.GetPoint(enter);
                    mousePos.y = 0f;

                    // 캐릭터 위치와 마우스 위치(타일 위치)가 같으면 이동하지 않음
                    Vector2Int charTile = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
                    Vector2Int mouseTile = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.z));
                    if (charTile == mouseTile)
                        return;

                    Vector3 dir = mousePos - transform.position;
                    dir.y = 0f;

                    // 4방향 중 가장 가까운 방향 구하기
                    Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                    float maxDot = float.NegativeInfinity;
                    Vector3 chosenDir = Vector3.zero;
                    foreach (var d in directions)
                    {
                        float dot = Vector3.Dot(dir.normalized, d);
                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            chosenDir = d;
                        }
                    }

                    Vector3 nextPos = transform.position + chosenDir;
                    // 장애물 체크 (필요 없으면 아래 if문 제거)
                    Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));
                    if (!IsBlockedTile(nextTile))
                    {
                        targetPosition = new Vector3(nextTile.x, 0f, nextTile.y);
                        startPosition = transform.position;
                        moveCoroutine = StartCoroutine(MoveToTarget());
                    }
                }
            }*/

            //키보드 이동
            if (!isMoving && !isBlocked)
            {

                bool teamTurn = (character.team == Team.team && TurnManager.Instance.isPlayerTurn);
//                if (nowmoveCount <= 0 && teamTurn)//이동횟수가 있어야 이동가능
//                     return;

                Vector3 chosenDir = Vector3.zero;

                try
                {
                    if (!StoryManager.instance.moveLock)
                    {
                        if (Input.GetKeyDown(KeyCode.UpArrow))
                        {
                            chosenDir = Vector3.forward;
                            lookRotation = ChRotation.up;
                        }
                        else if (Input.GetKeyDown(KeyCode.DownArrow))
                        {
                            chosenDir = Vector3.back;
                            lookRotation = ChRotation.down;
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            chosenDir = Vector3.left;
                            lookRotation = ChRotation.left;
                        }
                        else if (Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            chosenDir = Vector3.right;
                            lookRotation = ChRotation.right;
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.UpArrow) && SFD.moveUp && SFD.isSFD)
                        {
                            chosenDir = Vector3.forward;
                            SFD.isSFD = false;
                            lookRotation =  ChRotation.up;
                        }
                        else if (Input.GetKeyDown(KeyCode.DownArrow) && SFD.moveDo)
                        {
                            chosenDir = Vector3.back;
                            SFD.isSFD = false;
                            lookRotation =  ChRotation.down;
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftArrow) && SFD.moveL)
                        {
                            chosenDir = Vector3.left;
                            SFD.isSFD = false;
                            lookRotation = ChRotation.left;
                        }
                        else if (Input.GetKeyDown(KeyCode.RightArrow) && SFD.moveR)
                        {
                            chosenDir = Vector3.right;
                            SFD.isSFD = false;
                            lookRotation = ChRotation.right;
                        }
                    }
                }
                catch
                {
                    chosenDir = Vector3.zero; // StoryManager를 못불려와도 모든입력무시
                }

                if (chosenDir != Vector3.zero)
                {
                    Vector3 nextPos = transform.position + chosenDir;
                    Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));
                    if (!IsBlockedTile(nextTile))
                    {
                        float targetZ = 0f;
                        switch (lookRotation)
                        {
                            case ChRotation.up: targetZ = 0f; break;
                            case ChRotation.down: targetZ = 180f; break;
                            case ChRotation.left: targetZ = 90f; break;
                            case ChRotation.right: targetZ = -90f; break;
                        }

                        lookRota.transform.localRotation = Quaternion.Euler(0f, 0f, targetZ);

                        targetPosition = new Vector3(nextTile.x, 0f, nextTile.y);
                        startPosition = transform.position;
                        moveCoroutine = StartCoroutine(MoveToTarget());
                    }

                }
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

        Vector3 moveDir = targetPosition - transform.position;
        if (moveDir.x > 0)
            spriteRenderer.flipX = false;
        else if (moveDir.x < 0)
            spriteRenderer.flipX = true;

        Vector3 velocity = Vector3.zero; // SmoothDamp에서 사용할 속도 참조 변수
        float smoothTime = 0.02f; // 감속에 걸리는 시간 (값이 작을수록 더 빠르게 멈춤)

        // 목표 위치에 가까워질수록 점점 느려``지며 이동
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // SmoothDamp를 사용해 자연스럽게 감속하며 이동
            // moveSpeed는 최대 속도, smoothTime은 감속 시간
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, moveSpeed);
            yield return null; // 다음 프레임까지 대기
        }

        transform.position = targetPosition; // 정확한 목표 위치로 위치 보정
        isMoving = false; // 이동 완료 후 이동 가능 상태로 변경
        

        var stats = CharacterStats.Instance;
        var character = stats.GetStats(gameObject);
        bool teamTurn = (character.team == Team.team && TurnManager.Instance.isPlayerTurn);
/*        int count = CharacterStats.Instance.characterList[characterNumber].NowMoveCount;
        count = teamTurn ? count - 1 : count; // 팀 턴일 때만 이동 횟수 차감
        CharacterStats.Instance.characterList[characterNumber].NowMoveCount = count;*/

        PositionUpdate(); // 위치 갱신 처리
    }

    // 타일에 부딪혔을 때 호출되는 메서드
    private void OnTriggerEnter(Collider other)
    {

        // 충돌한 오브젝트가 타일인 경우
        if (other.CompareTag("Tile") || other.CompareTag("MapBorder"))
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
        if (other.CompareTag("Tile") || other.CompareTag("MapBorder"))
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
        CharacterStats.Instance.characterList[characterNumber].charPosition = transform.position;
    }

    public void OnDestroy()
    {
    }

    public void ShowMoveHighlights()
    {
        if (isShowMoveHighlights)
            return;

        ClearHighlights();

        // 마우스 방향 계산
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float enter;
        Vector3 mousePosition = transform.position;
        if (groundPlane.Raycast(ray, out enter))
        {
            mousePosition = ray.GetPoint(enter);
            mousePosition.y = 0f;
        }

        Vector3 dir = mousePosition - transform.position;
        dir.y = 0f;

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right};
        float maxDot = float.NegativeInfinity;
        Vector3 chosenDir = Vector3.zero;
        foreach (var d in directions)
        {
            float dot = Vector3.Dot(dir.normalized, d);
            if (dot > maxDot)
            {
                maxDot = dot;
                chosenDir = d;
            }
        }

        Vector3 nextPos = transform.position + chosenDir;
        Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));

        // 장애물 체크
        if (!IsBlockedTile(nextTile))
        {
            Vector3 pos = new Vector3(nextTile.x, 0.01f, nextTile.y);
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
