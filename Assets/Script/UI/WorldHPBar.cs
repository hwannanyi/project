using UnityEngine;
using UnityEngine.UI;

public class WorldHPBar : MonoBehaviour
{

    public static WorldHPBar Create(GameObject hpBarPrefab,Transform trans 
        ,Canvas parentCanvas, Stats chter)
    {
        // 체력바 프리팹을 캔버스의 자식으로 생성
        GameObject hpBarObj = Object.Instantiate(hpBarPrefab, parentCanvas.transform);
        WorldHPBar hpBar = hpBarObj.GetComponent<WorldHPBar>();
        hpBar.target = trans;
        hpBar.stats = chter;
        return hpBar;
    }

    public Transform target; // 따라다닐 캐릭터 Transform
    public Stats stats; // 캐릭터의 Stats 컴포넌트
    public Image hpFillImage; // 체력바의 Fill 이미지

    // 이후 체력바 위치, 체력 갱신 등은 Update에서 구현

    void Update()
    {
        if (target != null)
        {
            // 캐릭터의 위치를 따라다니도록 설정
            // World Space
            transform.position = target.position + Vector3.forward * 2f;

            // 체력바 fillAmount 갱신
            if (hpFillImage != null && stats != null && stats.maxhp > 0)
            {
                hpFillImage.fillAmount = Mathf.Clamp01(stats.hp / (float)stats.maxhp);
                
            }
            if (stats.isdie) Destroy(gameObject); // 캐릭터가 죽으면 체력바 제거
        }
    }
}