/*using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI;

// 스킬 효과(투사체)를 관리하는 클래스
public class SkillEffectProjectile : MonoBehaviour
{
    public float speed;  // 투사체 이동 속도
    public float range;  // 투사체의 최대 사거리
    public float Xaoe;   // 투사체 길이
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

    // 투사체 초기화 메서드
    public void Initialize(SkillData skill, Vector3 targetPosition, GameObject charcter, Stats character)
    {
        skillData = skill;
        speed = skill.projectileSpeed;
        range = skill.range;
        Xaoe = skill.Xaoe;

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
        // 투사체를 지속적으로 이동시키는 기능
        transform.position += direction * speed * Time.deltaTime;

        // 최대 사거리에 도달하면 투사체 삭제
        if (Vector3.Distance(startPosition, transform.position) >= range - Xaoe)
            Destroy(gameObject);


    }
}
*/
using UnityEngine;

public class SkillEffectProjectile : MonoBehaviour
{
    private SkillData skill;
    private Vector3 targetPosition;
    private GameObject caster;
    private Stats casterStats;

    private GameObject targetUnit; // 유도 타겟
    public float speed = 5f;

    private bool isInitialized = false;

    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;
    private Vector3 direction;
    public SpriteRenderer spriteRenderer;


    public void Initialize(SkillData skillData, Vector3 targetPos, GameObject casterObject, Stats character, GameObject target = null)
    {
        skill = skillData;
        caster = casterObject;
        casterStats = character;


        // 외형 변경
        if (spriteRenderer != null && skill.SkillEffectIllustration != null)
        {
            spriteRenderer.sprite = skill.SkillEffectIllustration;
        }

        if (skill.targeting && target != null)
        {
            targetUnit = target;
        }
        else
        {
            targetPosition = targetPos;
        }

        isInitialized = true;
        rotatingVisual.rotation = Quaternion.Euler(90f, 0, 0f);

        // 초기 방향 계산
        direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero && rotatingVisual != null)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(90f, angle, 0f);
        }

        GameObject HitboxTile = Instantiate(hitbox, this.transform);
        HitboxTile.transform.localPosition = Vector3.zero;

        // 병합 실행
        ColliderMerger merger = GetComponent<ColliderMerger>();
        if (merger != null)
        {
            merger.MergeChildBoxColliders();
        }

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
            hit.Initialize(skill, casterObject, character); // 또는 실제 캐릭터 GameObject
        }

    }

    void Update()
    {
        if (!isInitialized) return;

        Vector3 destination = (skill.targeting && targetUnit != null)
        ? targetUnit.transform.position
        : targetPosition;


        // direction 지역 변수 선언 제거 → 필드 변수로 사용
        direction = (destination - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;
        if (skill.targeting && targetUnit != null)
        {
            Debug.Log($"[Destination 추적] TargetUnit: {targetUnit.name}, Position: {targetUnit.transform.position}");
        }
        // 실시간 회전
        if (rotatingVisual != null && direction.magnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(90f, angle, 0f);
        }

        // 이동기이라면 시전자를 스킬 위치로 이동시킴
        if (skill.skillTypes.Contains(skillType.movement))
        {
            var manager = CharacterStats.Instance;

            var casterStats = manager.GetStats(caster);
            // charcterUnit의 위치를 스킬 위치로 이동한뒤 위치 갱신
            if (caster != null)
            {
                caster.transform.position = transform.position;
                casterStats.charPosition = transform.position;
            }
        }

        // 도착처리
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        // 데미지, 이펙트 등
        Debug.Log($"[SkillEffectProjectile] {skill.skillName} 타격 완료");

        // 이동기이라면 시전자를 스킬 위치로 이동시킴
        if (skill.skillTypes.Contains(skillType.movement))
        {
            var manager = CharacterStats.Instance;

            var casterStats = manager.GetStats(caster);
            // charcterUnit의 위치를 스킬 위치로 이동한뒤 위치 갱신
            if (caster != null)
            {
                caster.transform.position = GetNearestTile(caster.transform.position);
                casterStats.charPosition = GetNearestTile(caster.transform.position);
            }
        }

        Destroy(gameObject);
        //TurnManager.Instance.ExitReactPhase(); //미사용, 스킬종료로 인해 대응단계가 종료되지 않도록 변경
    }

    // 가장 가까운 타일을 찾는 메서드
    private Vector3 GetNearestTile(Vector3 currentPosition)
    {
        // 현재 위치에서 가장 가까운 타일의 좌표를 계산
        float x = Mathf.Round(currentPosition.x);
        float y = Mathf.Round(currentPosition.z);

        return new Vector3(x, 0f, y);  // 가장 가까운 타일로 반환
    }
}
