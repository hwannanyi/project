using System.Linq;
using UnityEngine;

/// <summary>
/// 스킬의 히트박스(타일)를 관리하는 컴포넌트.
/// 콜라이더의 크기와 위치를 조정하고, 활성화/비활성화 기능을 제공한다.
/// </summary>
public class HitboxTile : MonoBehaviour
{
    // 콜라이더의 크기(X, Y) 정보
    public Vector2 colliderXY;
    // 콜라이더의 위치 오프셋(X, Y)
    public Vector2 offsetXY;
    // BoxCollider 캐싱용 변수
    private BoxCollider boxCollider;
    // 회전용 트랜스폼(필요시 사용)
    public Transform rotating;
    // Collider 캐싱용 변수 (활성화/비활성화 용도)
    private Collider col;

    /// <summary>
    /// 컴포넌트가 생성될 때 호출됨. 콜라이더와 트랜스폼을 캐싱한다.
    /// </summary>
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>(); // BoxCollider 컴포넌트 캐싱
        rotating = GetComponent<Transform>();      // Transform 컴포넌트 캐싱
        // col = GetComponent<Collider>();
        // if (col != null) col.enabled = false; // 기본적으로 콜라이더 비활성화 (주석 처리됨)
    }

    /// <summary>
    /// 히트박스(타일) 초기화. 콜라이더 크기와 오프셋을 설정한다.
    /// </summary>
    /// <param name="X">콜라이더 X 크기</param>
    /// <param name="Y">콜라이더 Y 크기</param>
    /// <param name="offset">오프셋 벡터</param>
    public void Initialize(float X, float Y, Vector2 offset)
    {
        // 콜라이더 크기 및 오프셋 적용
        ResizeColliderWithOffset(new Vector2(X, Y), offset);
    }

    /// <summary>
    /// 콜라이더를 활성화한다. (충돌 체크 시작)
    /// </summary>
    public void EnableCollider()
    {
        if (col == null) col = GetComponent<Collider>(); // Collider 캐싱
        if (col != null)
        {
            col.enabled = true; // 콜라이더 활성화
            Debug.Log("[HitboxTile] Collider 활성화됨");
        }
    }

    /// <summary>
    /// 콜라이더의 크기와 위치 오프셋을 적용한다.
    /// </summary>
    /// <param name="XY">콜라이더 크기(X, Y)</param>
    /// <param name="offset">오프셋 벡터</param>
    private void ResizeColliderWithOffset(Vector2 XY, Vector2 offset)
    {
        if (boxCollider == null || colliderXY == null)
        {
            Debug.LogWarning("BoxCollider 또는 colliderXY 값이 없음");
            return;
        }

        // 콜라이더 크기 조정 (약간 작게 설정하여 경계 충돌 방지)
        boxCollider.size = new Vector2(XY.x - 0.2f, XY.y - 0.2f);
        // 오프셋만큼 위치 이동
        transform.position += new Vector3(offset.x, 0f, offset.y);
    }
}