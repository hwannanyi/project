using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

// 스킬 효과(투사체)를 관리하는 클래스
public class SkillEffectHitscan : MonoBehaviour
{
    public float range;  // 투사체의 최대 사거리
    public int damage;   // 투사체가 가하는 피해량
    public SkillData skillData;  // 스킬 데이터 참조

    private Vector3 startPosition;  // 투사체 시작 위치
    private Vector3 direction;      // 투사체 이동 방향
    private Vector3 targetPosition;
    public GameObject targetUnit;

    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;

    public bool isInitialized = false;

    private void Awake()
    {

    }

    public void Initialize(SkillData skill, Vector3 targetPos, GameObject charcter, Stats character, GameObject target = null)
    {
        skillData = skill;
        range = skill.range;

        

        if (skill.targeting && target != null)
        {
            targetUnit = target;
            transform.position = target.transform.position;
            startPosition = transform.position;
            targetPosition = target.transform.position;
        }
        else
        {
            startPosition = transform.position;
            targetPosition = targetPos;
        }

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

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;


        if (skillData.targeting && targetPosition != null)
        {
            transform.position = targetPosition;
        }

        // 실시간으로 타겟 방향 갱신

        /*        direction = GetDirection();
                RotateVisual(direction);*/

        // 히트스캔은 위치 갱신만 하고, 이펙트는 곧 사라짐
        Destroy(gameObject, 3); // 아주 짧게 남기기
    }

    private Vector3 GetDirection()
    {
        Vector3 currentTarget = (skillData.targeting && targetUnit != null)
            ? targetUnit.transform.position
            : targetPosition;

        currentTarget.z = startPosition.z;
        return (currentTarget - startPosition).normalized;
    }

    private void RotateVisual(Vector3 dir)
    {
        if (rotatingVisual != null && dir.magnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // 보정 필요 시 -90f
        }
    }


}
