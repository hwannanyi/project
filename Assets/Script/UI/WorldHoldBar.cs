using UnityEngine;
using UnityEngine.UI;
public class WorldHoldBar : MonoBehaviour
{
   
    public static WorldHoldBar Create(GameObject holdBarPrefab, Transform trans
        , Canvas parentCanvas, Stats chter, int idx)
    {
        // 체력바 프리팹을 캔버스의 자식으로 생성
        GameObject holdBarObj = Object.Instantiate(holdBarPrefab, parentCanvas.transform);
        WorldHoldBar holdBar = holdBarObj.GetComponent<WorldHoldBar>();
        holdBar.target = trans;
        holdBar.stats = chter;
        holdBar.idx = idx;
        holdBar.idx = idx;
        holdBar.holdGaugeMax = chter.holdGauge[idx].holdGauge; // holdGauge의 최대값 설정
        return holdBar;
    }

    public Transform target; // 따라다닐 캐릭터 Transform
    public Stats stats; // 캐릭터의 Stats 컴포넌트
    public Image hpFillImage; // 체력바의 Fill 이미지
    public int idx; // holdGauge의 인덱스
    public float holdGaugeMax; // holdGauge 값

    // 이후 체력바 위치, 체력 갱신 등은 Update에서 구현

    void Update()
    {
        if (target != null)
        {
            // 캐릭터의 위치를 따라다니도록 설정
            // World Space
            transform.position = target.position + Vector3.forward * (2.5f + (float)idx/2);

            // 체력바 fillAmount 갱신
            if (hpFillImage != null && stats != null && stats.holdGauge != null && stats.holdGauge.Count > idx && stats.holdGauge[idx].holdGauge > 0)
            {
                hpFillImage.fillAmount = Mathf.Clamp01(stats.holdGauge[idx].holdGauge / holdGaugeMax);
            }
            if (stats.isdie && stats.holdGauge[idx].holdGauge<=0) Destroy(gameObject); // 캐릭터가 죽으면 바 제거
        }
    }
}
