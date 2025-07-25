using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public GameObject black_Curtain; // 검은 커튼 오브젝트
    public Image black_Curtain_image; // 커튼의 이미지 컴포넌트

    float black_Curtain_value = 1f; // 커튼 알파값(투명도)
    public float black_Curtain_speed = 1f; // 커튼 전환 속도

    public void Awake()
    {
        black_Curtain.SetActive(true); // 시작 시 커튼 활성화
        black_Curtain_image.color = new Color(0, 0, 0, black_Curtain_value); // 완전히 어둡게 설정

        StartCoroutine(FadeOutCurtainRoutine()); // 커튼 점점 투명하게
    }

    // 커튼이 점점 투명해졌다가 비활성화되는 코루틴
    private IEnumerator FadeOutCurtainRoutine()
    {
        yield return new WaitForSeconds(0.3f); // 잠깐 대기
        while (black_Curtain_value > 0f)
        {
            black_Curtain_value -= black_Curtain_speed * Time.deltaTime;
            black_Curtain_value = Mathf.Clamp01(black_Curtain_value);
            black_Curtain_image.color = new Color(0, 0, 0, black_Curtain_value);
            yield return null;
        }
        black_Curtain.SetActive(false); // 완전히 투명해지면 커튼 비활성화
    }
}
