using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReactTimeUI : MonoBehaviour
{
    public Image reactTimeBar;      // 대응시간 바 (UI에 연결)
    public TextMeshProUGUI reactTimeText;       // 대응시간 텍스트 (UI에 연결)

    private float maxReactTime;      // 최대 대응시간
    private float currentReactTime;  // 남은 대응시간

    public GameObject isActive;

    public void Awake()
    {
        isActive.SetActive(false); // UI 오브젝트 비활성화
    }
    // 대응시간 UI 시작
    public void SetReactTime(float maxTime)
    {
        maxReactTime = maxTime;
        currentReactTime = maxTime;
        if (isActive != null)
            isActive.SetActive(true); // UI 오브젝트 활성화
        UpdateUI();
    }


    // Update에서 비활성화
    void Update()
    {
        if (isActive == null) return;
        if (currentReactTime > 0f)
        {
            currentReactTime -= Time.deltaTime;
            if (currentReactTime <= 0f)
            {
                currentReactTime = 0f;
                isActive.SetActive(false); // UI 오브젝트 비활성화
            }
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (reactTimeBar != null)
            reactTimeBar.fillAmount = maxReactTime > 0 ? currentReactTime / maxReactTime : 0;

        if (reactTimeText != null)
            reactTimeText.text = $"{currentReactTime:F1}초";
    }
}
