using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;


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

    void Start()
    {  
        targetPosition = transform.position;  // 시작 위치 설정
        startPosition = transform.position;   // 초기 위치 설정
        CharacterSelection.selectedCharacterIndex = -1;
    }
    void Awake()
    {
        
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
                moveCount = CharacterStats.Instance.characterList[index].moveCount;

            }
        }
        if (characterNumber == CharacterSelection.selectedCharacterIndex)
        {
            if (SkillManager.Instance.waitingForResponse == true)
            {
                return;
            }
            // 이동 중이 아니고, 막히지 않았을 때만 마우스 클릭을 처리
            if (moveCount>0 &&!isMoving && !isBlocked && Input.GetMouseButtonDown(0))
            {
                // 대응단계 확인
                if (TurnManager.Instance.IsInReactPhase())
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
                }

                // [수정됨] Perspective 카메라 대응: 마우스 위치를 정확히 가져오기 위한 Raycast 방식
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.forward, Vector3.zero); // z = 0 기준 평면
                float enter;
                Vector3 mousePosition = transform.position;
                if (groundPlane.Raycast(ray, out enter))
                {
                    mousePosition = ray.GetPoint(enter);
                    mousePosition.z = 0f;
                }
                Vector3 roundedTarget = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0f);

                // 이동 제한 범위를 벗어나면 이동하지 않음
                if (Mathf.Abs(roundedTarget.x - transform.position.x) > moveRange 
                    || Mathf.Abs(roundedTarget.y - transform.position.y) > moveRange)
                {
                    return;
                }

                targetPosition = roundedTarget;
                startPosition = transform.position;  // 이동 시작 위치 저장
                moveCoroutine = StartCoroutine(MoveToTarget());  // 이동을 코루틴으로 처리
            }
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
        //SendSignal(true);
        moveCount = moveCount - 1;
        isMoving = false;  // 이동 완료 후 이동 가능 상태로 변경
        PositionUpdate();

    }

    // 타일에 부딪혔을 때 호출되는 메서드
    private void OnTriggerEnter2D(Collider2D other)
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
            moveCount = moveCount - 1;
            PositionUpdate();
            //SendSignal(true);
        }
    }

    // 가장 가까운 타일을 찾는 메서드
    private Vector3 GetNearestTile(Vector3 currentPosition)
    {
        // 현재 위치에서 가장 가까운 타일의 좌표를 계산
        float x = Mathf.Round(currentPosition.x);
        float y = Mathf.Round(currentPosition.y);

        return new Vector3(x, y, 0f);  // 가장 가까운 타일로 반환
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
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tile"))
        {
            isBlocked = false;  // 이동 차단 해제
        }
    }

    private void PositionUpdate()
    {
        CharacterStats.Instance.characterList[characterNumber].charPosition = transform.position;
    }

}
