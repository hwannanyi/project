using UnityEngine;

public class MaskScreen : MonoBehaviour
{


    public GameObject canvasObj;
    public GameObject maskScreen;



    public void SetMaskScreen(GameObject UI)
    {
        RectTransform targetRect = UI.GetComponent<RectTransform>();
        RectTransform myRect = GetComponent<RectTransform>();

        if (targetRect != null && myRect != null)
        {
            // 앵커까지 복사
            myRect.anchorMin = targetRect.anchorMin;
            myRect.anchorMax = targetRect.anchorMax;
            myRect.pivot = targetRect.pivot;

            // 크기, 위치, 회전, 스케일 복사
            myRect.sizeDelta = targetRect.sizeDelta;
            myRect.anchoredPosition = targetRect.anchoredPosition;
            myRect.localRotation = targetRect.localRotation;
            myRect.localScale = targetRect.localScale;
            SetMaskScreenToCanvas();
        }
    }
    public void SetMaskScreenToCanvas()
    {
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        RectTransform maskRect = maskScreen.GetComponent<RectTransform>();
        RectTransform parentRect = maskRect.parent as RectTransform;

        if (canvasRect != null && maskRect != null && parentRect != null)
        {
            // 1. 앵커와 피벗을 센터로 고정
            maskRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);

            // 2. 캔버스의 중심 월드 좌표 구하기
            Vector3 canvasWorldCenter = canvasRect.TransformPoint(canvasRect.rect.center);

            // 3. maskScreen의 부모 기준 로컬 좌표로 변환
            Vector3 localPos = parentRect.InverseTransformPoint(canvasWorldCenter);

            // 4. 크기와 위치 적용
            maskRect.sizeDelta = canvasRect.rect.size;
            maskRect.anchoredPosition = new Vector2(localPos.x, localPos.y);

            // 5. 회전, 스케일 초기화
            maskRect.localRotation = Quaternion.identity;
            maskRect.localScale = Vector3.one;
        }
    }
}
