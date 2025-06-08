using UnityEngine;

[DisallowMultipleComponent]
public class ColliderMerger : MonoBehaviour
{
    [Tooltip("Start()에서 자동 병합 여부")]
    public bool mergeOnStart = true;

    private void Start()
    {
        if (mergeOnStart)
        {
            MergeChildBoxColliders();
        }
    }

    public void MergeChildBoxColliders()
    {
        var childColliders = GetComponentsInChildren<BoxCollider>();

        // 자기 자신 제외
        childColliders = System.Array.FindAll(childColliders, c => c.gameObject != this.gameObject);

        if (childColliders.Length == 0)
        {
            Debug.LogWarning("[ColliderMerger] 병합할 자식 BoxCollider가 없습니다.");
            return;
        }

        // 전체 Bounds 계산
        Bounds combinedBounds = childColliders[0].bounds;
        for (int i = 1; i < childColliders.Length; i++)
        {
            combinedBounds.Encapsulate(childColliders[i].bounds);
        }

        // 기존 부모 콜라이더 제거
        BoxCollider existing = GetComponent<BoxCollider>();
        if (existing != null) Destroy(existing);

        // 병합된 콜라이더 추가
        BoxCollider merged = gameObject.AddComponent<BoxCollider>();
        merged.center = transform.InverseTransformPoint(combinedBounds.center);
        merged.size = combinedBounds.size;
        merged.isTrigger = true; // 트리거 활성화

        Debug.Log("[ColliderMerger] 병합된 BoxCollider 생성 완료 (isTrigger = true)");
    }
}