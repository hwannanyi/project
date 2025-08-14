using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Data.SqlTypes;

public class Cursor : MonoBehaviour
{
    public static Cursor Instance;
    public SkillManager skillManager; // 스킬 매니저 참조
    public StoryManager storyManager; // 스토리 매니저 참조
    public int moveSpeed = 10;  // 이동 속도
    private Vector3 targetPosition;  // 목표 위치
    private Vector3 startPosition;   // 이동 시작 위치
    public bool isMoving = false;  // 이동 중인지 여부

    private Coroutine moveCoroutine;  // 이동 코루틴을 저장할 변수

    public float moveDelay = 0.1f; // 이동 딜레이(초)
    private float lastMoveTime = 0f;

    void Start()
    {
        targetPosition = transform.position;  // 시작 위치 설정
        startPosition = transform.position;   // 초기 위치 설정
    }

    void Update()
    {
        //키보드 이동
        if (!isMoving && !SkillManager.Instance.isSkillReadyFinal)//이동중 입력 무시
        {
            Vector3 chosenDir = Vector3.zero;//이동방향 기본값

            if (!storyManager.moveLock)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                    chosenDir = Vector3.forward;
                else if (Input.GetKey(KeyCode.DownArrow))
                    chosenDir = Vector3.back;
                else if (Input.GetKey(KeyCode.LeftArrow))
                    chosenDir = Vector3.left;
                else if (Input.GetKey(KeyCode.RightArrow))
                    chosenDir = Vector3.right;
            }


            // 방향이 설정되었을 때만 이동 시작
            // 이동 딜레이 체크
            if (chosenDir != Vector3.zero && Time.time - lastMoveTime > moveDelay)
            {
                if (skillManager.selectedSkill.projectile)
                {
                    Vector3 nextPos = skillManager.selectedCaster.transform.position + chosenDir;
                    Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));
                    transform.position = new Vector3(nextTile.x, 0f, nextTile.y);
                    lastMoveTime = Time.time; // 마지막 이동 시간 갱신
                }
                else
                {
                    Vector3 nextPos = transform.position + chosenDir;
                    Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));
                    transform.position = new Vector3(nextTile.x, 0f, nextTile.y);
                    lastMoveTime = Time.time; // 마지막 이동 시간 갱신
                }

            }
        }

    }

    void OnDisable()
    {
        isMoving = false;  // 커서가 비활성화될 때 이동 상태 초기화
    }

    /*    void OnEnable()
        {
            Vector3 nearestTile = GetNearestTile(transform.position);
            StartCoroutine(MoveToTargetFromCurrent(nearestTile));  // 가장 가까운 타일로 이동
        }

        // 목표 위치로 이동하는 코루틴
        private IEnumerator MoveToTarget()
        {
            isMoving = true;  // 이동 중 상태로 설정

            Vector3 velocity = Vector3.zero; // SmoothDamp에서 사용할 속도 참조 변수
            float smoothTime = 0.05f; // 감속에 걸리는 시간 (값이 작을수록 더 빠르게 멈춤)

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

        private Vector3 GetNearestTile(Vector3 currentPosition)
        {
            // 현재 위치에서 가장 가까운 타일의 좌표를 계산
            float x = Mathf.Round(currentPosition.x);
            float y = Mathf.Round(currentPosition.z);

            return new Vector3(x, 0f, y);  // 가장 가까운 타일로 반환
        }
    */
}
