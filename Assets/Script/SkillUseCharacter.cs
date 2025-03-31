using TMPro;
using UnityEngine;

public class SkillUseCharacter : MonoBehaviour
{

    void Update()
    {
/*        if (characterNumber == CharacterSelection.selectedCharacterIndex)
        {

            // 이동 중이 아니고, 막히지 않았을 때만 마우스 클릭을 처리
            if (!isMoving && !isBlocked && Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0f;  // Z 값을 0으로 고정 (2D 타일 이동)

                targetPosition = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0f);
                startPosition = transform.position;  // 이동 시작 위치 저장
                moveCoroutine = StartCoroutine(MoveToTarget());  // 이동을 코루틴으로 처리


            }
        }*/
        if (Input.GetKeyDown(KeyCode.Q))
        {

        }
    }
}
