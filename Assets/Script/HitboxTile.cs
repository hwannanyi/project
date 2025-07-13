using System.Linq;
using UnityEngine;

public class HitboxTile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //public SkillData skillData;  // 스킬 데이터 참조
    public Vector2 colliderXY;
    public Vector2 offsetXY; // 오프셋 값
    private BoxCollider boxCollider; // 콜라이더 캐시
    public Transform rotating;
    private Collider col;


    private void Awake()
    {
        // Collider 크기 조절 (초기화 전에 호출될 수 있으므로 Initialize에서도 한 번 더 호출)
        boxCollider = GetComponent<BoxCollider>();
        rotating = GetComponent<Transform>();
        col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // 기본적으로 꺼져 있어야 함
        }
    }

    // 투사체 초기화 메서드
    public void Initialize(float X, float Y, Vector2 offset)
    {
        //skillData = skill;

        // 콜라이더 크기 및 오프셋 설정
        ResizeColliderWithOffset(new Vector2(X, Y), offset);

    }

    public void EnableCollider()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            Debug.Log("[HitboxTile] Collider 활성화됨");
        }
    }

    private void ResizeColliderWithOffset(Vector2 XY, Vector2 offset)
    {
        if (boxCollider == null || colliderXY == null)
        {
            Debug.LogWarning("BoxCollider2D 또는 SkillData 없음");
            return;
        }

        // 크기 조정: 스킬의 Xaoe, Yaoe 반영
            boxCollider.size = XY;
        transform.position += new Vector3(offset.x, 0f, offset.y);
    }
}
