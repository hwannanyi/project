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
    public int moveSpeed = 10;  // 이동 속도
    private Vector3 targetPosition;  // 목표 위치
    private Vector3 startPosition;   // 이동 시작 위치
    public bool isMoving = false;  // 이동 중인지 여부

    private Coroutine moveCoroutine;  // 이동 코루틴을 저장할 변수



    void Start()
    {
        targetPosition = transform.position;  // 시작 위치 설정
        startPosition = transform.position;   // 초기 위치 설정


    }

    void Update()
    {
            //키보드 이동
            if (!isMoving)//이동중 입력 무시
            {
                Vector3 chosenDir = Vector3.zero;//이동방향 기본값

                    if (!StoryManager.instance.moveLock)
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
                if (chosenDir != Vector3.zero)
                {
                    Vector3 nextPos = transform.position + chosenDir;
                    Vector2Int nextTile = new Vector2Int(Mathf.RoundToInt(nextPos.x), Mathf.RoundToInt(nextPos.z));

                        targetPosition = new Vector3(nextTile.x, 0f, nextTile.y);
                        startPosition = transform.position;
                        moveCoroutine = StartCoroutine(MoveToTarget());
                    
                }
            }
        
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
}
