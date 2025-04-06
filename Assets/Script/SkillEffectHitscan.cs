using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

// 스킬 효과(투사체)를 관리하는 클래스
public class SkillEffectHitscan : MonoBehaviour
{
    public float range;  // 투사체의 최대 사거리
    public int damage;   // 투사체가 가하는 피해량
    public SkillData skillData;  // 스킬 데이터 참조

    private Vector3 startPosition;  // 투사체 시작 위치
    private Vector3 direction;      // 투사체 이동 방향


    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;

    private void Awake()
    {

    }

    public void Initialize(SkillData skill, Vector3 targetPosition, GameObject charcter, Stats character)
    {
        skillData = skill;
        range = skill.range;

        startPosition = transform.position;

        // direction 벡터 계산 (이동 방향)
        targetPosition.z = startPosition.z;
        direction = (targetPosition - startPosition).normalized;

        // 필수: direction이 0이 아닐 때만 회전 처리
        if (direction != Vector3.zero && rotatingVisual != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        GameObject HitboxTile = Instantiate(hitbox, this.transform);
        HitboxTile.transform.localPosition = Vector3.zero;

        // 2. 그 인스턴스에서 SkillProjectileHitbox 스크립트를 가져와 초기화
        HitboxTile hitboxScript = HitboxTile.GetComponent<HitboxTile>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(skill);
        }
        // 충돌 처리 전달
        SkillHitOn hit = GetComponent<SkillHitOn>();
        if (hit != null)
        {
            hit.Initialize(skill, charcter, character); // 또는 실제 캐릭터 GameObject
        }

    }

    void Update()
    {
            Destroy(gameObject,3);
    }
}
