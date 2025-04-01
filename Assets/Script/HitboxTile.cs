using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class HitboxTile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Skill skillData;  // 스킬 데이터 참조
    private BoxCollider2D boxCollider; // 콜라이더 캐시
    public Transform rotating;
    private Collider2D col;


    private void Awake()
    {
        // Collider 크기 조절 (초기화 전에 호출될 수 있으므로 Initialize에서도 한 번 더 호출)
        boxCollider = GetComponent<BoxCollider2D>();
        rotating = GetComponent<Transform>();
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false; // 기본적으로 꺼져 있어야 함
        }
    }

    // 투사체 초기화 메서드
    public void Initialize(Skill skill)
    {
        skillData = skill;

        // 콜라이더 크기 및 오프셋 설정
        ResizeColliderWithOffset();

        
    }

    public void EnableCollider()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            Debug.Log("[HitboxTile] Collider 활성화됨");
        }
    }

    private void ResizeColliderWithOffset()
    {
        if (boxCollider == null || skillData == null)
        {
            Debug.LogWarning("BoxCollider2D 또는 SkillData 없음");
            return;
        }

        // 크기 조정: 스킬의 Xaoe, Yaoe 반영
        if (skillData.projectileType.ToString() != "spAoe")
            boxCollider.size = new Vector2(skillData.Xaoe, skillData.Yaoe);
    }
}
