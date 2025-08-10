using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TextCore.Text;
using static UnityEngine.RuleTile.TilingRuleOutput;
//using UnityEditor.Experimental.GraphView;
using NUnit.Framework;
using System.Collections.Generic;

public class SkillEffectProjectile : MonoBehaviour
{
    public SkillData skill;
    public Vector3 targetPosition;
    public GameObject caster;
    public Stats casterStats;

    private GameObject targetUnit; // 유도 타겟
    public float speed = 5f;

    private bool isInitialized = false;

    public ColliderMerger colliderMerger; // 콜라이더 병합 컴포넌트

    public UnityEngine.Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;
    public Vector3 direction;
    public SpriteRenderer spriteRenderer;

    public GameObject trackingObject; //tracking이 참이라면 생성해 경로를 남김

    public bool CastLock = false;

    public AICastSkill aICastSkill;

    public List<Vector3> trackingPositions = new();

    public SkillTiming skillTiming;

    public void Initialize(SkillData skillData, Vector3 targetPos, GameObject casterObject, Stats character, GameObject target = null)
    {
        skillTiming = SkillTiming.start;
        skill = skillData;
        caster = casterObject;
        casterStats = character;
        colliderMerger = GetComponent<ColliderMerger>();

        aICastSkill = casterObject.GetComponent<AICastSkill>();
        CastLock = aICastSkill.skillCastLock;

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

/*        GameObject HitboxTile = Instantiate(hitbox, this.transform);
        HitboxTile.transform.localPosition = Vector3.zero;*/
/*
        // 병합 실행
        ColliderMerger merger = GetComponent<ColliderMerger>();
        if (merger != null)
        {
            merger.MergeChildBoxColliders();
        }
*/
        // 2. 그 인스턴스에서 SkillProjectileHitbox 스크립트를 가져와 초기화
        if (skill.aoetype == AoeType.spAoe)
        {
            if (skillData.specialAoe == null || skillData.specialAoe.Length == 0)
            {
                return;
            }
            for (int i = 0; i < skillData.specialAoe.Length; i++)
            {
                GameObject hitboxObj = Instantiate(hitbox, this.transform);
                hitboxObj.transform.localPosition = Vector3.zero;
                HitboxTile hitboxScript = hitboxObj.GetComponent<HitboxTile>();
                hitboxScript.Initialize(skillData.specialAoe[i].size.x, skillData.specialAoe[i].size.y,
                    skillData.specialAoe[i].position);
            }
        }
        else
        {
            GameObject hitboxObj = Instantiate(hitbox, this.transform);
            hitboxObj.transform.localPosition = Vector3.zero;
            HitboxTile hitboxScript = hitboxObj.GetComponent<HitboxTile>();
            hitboxScript.Initialize(skill.Xaoe, skill.Yaoe, Vector2.zero);
        }
        // 충돌 처리 전달
        SkillHitOn hit = GetComponent<SkillHitOn>();
        if (hit != null)
        {
            hit.Initialize(skill, casterObject, character); // 또는 실제 캐릭터 GameObject
        }

        //colliderMerger.MergeChildBoxColliders();
        // 스킬 데이터 전송
        var castingSkillData = GetComponent<CastingSkillData>();
        if (castingSkillData != null)
        {
            castingSkillData.SetSkillData(
                skillData,
                targetPos,
                casterObject,
                character,
                target);
        }

        skillTiming = SkillTiming.casting;
    }

    void Update()
    {
        if (!isInitialized) return;

        //if(skillTiming != SkillTiming.casting) return;

        Vector3 destination = (skill.targeting && targetUnit != null)
        ? targetUnit.transform.position
        : targetPosition;

        // 도착처리
        if (Vector3.Distance(transform.position, destination) < 0.2f)
        {
            OnHit();
        }

        Tracking(skill.tracking);

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
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MapBorder"))
        {
            OnHit();
        }
    }

    public void OnHit()
    {
        // 데미지, 이펙트 등
        Debug.Log($"[SkillEffectProjectile] {skill.skillName} 타격 완료");

        transform.position = GetNearestTile(transform.position);
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


        // AI 스킬 시전 잠금 해제
        aICastSkill.skillCastLock = !CastLock && aICastSkill.skillCastLock;



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

    public void Tracking(bool tracking)
    { 
        if (!tracking) return;

        Vector3 nearestTile = GetNearestTile(transform.position);

        if (trackingPositions.Contains(nearestTile))
            return;
        trackingPositions.Add(nearestTile);
        SkillData additionalSkillData = skill.AdditionalSkillData.skill;
        GameObject skillObject = Instantiate(additionalSkillData.SkillEffectPrefab, nearestTile, Quaternion.identity);
        if (skillObject.TryGetComponent<SkillEffectHitscan>(out var effect))
            effect.Initialize(additionalSkillData, nearestTile, caster, casterStats, null);

    }
}
