using UnityEngine;
using System.Collections;

public class DamageTextAutoDestroy : MonoBehaviour
{
    public float moveDistance = 1f;      // 올라갈 거리 (픽셀)
    public float moveDuration = 1.0f;    // 이동에 걸리는 시간(초)
    public float destroyDelay = 1f;      // 파괴까지의 시간(초)

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransform 캐싱
        transform.SetAsLastSibling(); // 나중에 생성된 텍스트가 항상 위에 보이도록 레이어 조정
        StartCoroutine(MoveUpAndDestroy()); // 이동 및 파괴 코루틴 시작
    }

    /// <summary>
    /// 텍스트를 moveDistance만큼 부드럽게(처음엔 빠르고 점점 느려지게) 위로 올리고, 일정 시간 후 파괴
    /// </summary>
    private IEnumerator MoveUpAndDestroy()
    {
        Vector2 startPos = rectTransform.anchoredPosition; // 시작 위치
        Vector2 targetPos = startPos + Vector2.up * moveDistance; // 목표 위치
        float timer = 0f;

        // moveDuration 동안 Ease-Out 방식으로 부드럽게 이동
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            // Ease-Out: 처음엔 빠르고 점점 느려짐
            float easeT = 1f - (1f - t) * (1f - t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos; // 최종 위치 보정

        // 이동이 끝난 후 destroyDelay만큼 대기 후 파괴
        yield return new WaitForSeconds(destroyDelay - moveDuration);
        Destroy(gameObject);
    }
}
