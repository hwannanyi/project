using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

// 스킬 효과(투사체)를 관리하는 클래스
public class SkillEffectHitscan : MonoBehaviour
{
    public float speed;  // 투사체 이동 속도
    public float range;  // 투사체의 최대 사거리
    public int damage;   // 투사체가 가하는 피해량
    public Skill skillData;  // 스킬 데이터 참조

    private Vector3 startPosition;  // 투사체 시작 위치
    private Vector3 direction;      // 투사체 이동 방향


    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;

    private void Awake()
    {

    }

    // 투사체 초기화 메서드
    public void Initialize(Skill skill, Vector3 targetPosition)
    {

        skillData = skill;
        speed = skill.projectileSpeed;
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


        GameObject ProjectileHitbox = Instantiate(hitbox, this.transform);
        ProjectileHitbox.transform.localPosition = Vector3.zero;

        // 2. 그 인스턴스에서 SkillProjectileHitbox 스크립트를 가져와 초기화
        HitboxTile hitboxScript = ProjectileHitbox.GetComponent<HitboxTile>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(skill);
        }
    }

    void Update()
    {
        
    }
}