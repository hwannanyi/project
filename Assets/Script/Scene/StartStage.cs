using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartStage : MonoBehaviour
{
    public GameObject black_Curtain; // 검은 커튼 오브젝트
    public Image black_Curtain_image; // 커튼의 이미지 컴포넌트

    float black_Curtain_value = 0f; // 커튼 알파값(투명도)
    public float black_Curtain_speed = 1f; // 커튼 전환 속도

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        StartCoroutine(StartGameRoutine()); // 코루틴 실행
    }

    // 입장 연출 코루틴
    private IEnumerator StartGameRoutine()
    {
        black_Curtain.SetActive(true); // 커튼 활성화
        black_Curtain_value = 0f;
        // 커튼이 점점 어두워짐
        while (black_Curtain_value < 1f)
        {
            black_Curtain_value += black_Curtain_speed * Time.deltaTime;
            black_Curtain_image.color = new Color(0, 0, 0, black_Curtain_value);
            yield return null;
        }
        black_Curtain_image.color = new Color(0, 0, 0, 1f); // 완전히 어두워짐
        yield return new WaitForSeconds(0.3f); // 잠깐 대기
        SceneManager.LoadScene("StageScene"); // 게임 씬으로 전환
    }

}
