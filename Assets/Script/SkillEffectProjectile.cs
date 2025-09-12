//using UnityEditor.Experimental.GraphView;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

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


    public List<Vector3> targetPos = new(); // 순차적 목표추적형
    public int targetCount = 0; // 현제 순서
    public int repeatCount = 0; // 반복 횟수
    public bool targetPosEnd = false; // 목표지점 도달 여부

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
        SkillHitOn hit = GetComponent<SkillHitOn>();

        if (skill.aoetype == AoeType.spAoe)
        {
            if (skillData.specialAoe == null || skillData.specialAoe.Length == 0)
            {
                return;
            }
            for (int i = 0; i < skillData.specialAoe.Length; i++)
            {
                GameObject hitboxObj = Instantiate(hitbox, transform.position, transform.rotation);
                //hitboxObj.transform.localPosition = Vector3.zero;
                HitboxTile hitboxScript = hitboxObj.GetComponent<HitboxTile>();
                hitboxScript.Initialize(skillData.specialAoe[i].size.x, skillData.specialAoe[i].size.y,
                    skillData.specialAoe[i].position, transform, hit);
            }
        }
        else
        {
            GameObject hitboxObj = Instantiate(hitbox, transform.position, transform.rotation);
            //hitboxObj.transform.localPosition = Vector3.zero;
            HitboxTile hitboxScript = hitboxObj.GetComponent<HitboxTile>();
            hitboxScript.Initialize(skill.Xaoe, skill.Yaoe, Vector2.zero, transform, hit);
        }
        // 충돌 처리 전달
        
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
        targetPosEnd = false;
        if (skill.projectile_targetMove)
            StartCoroutine(TargetPosMove(skill.targetPos));
    }
    
    void Update()
    {
        if (!isInitialized) return;

        //if(skillTiming != SkillTiming.casting) return;

        Vector3 destination = (skill.targeting && targetUnit != null)
        ? targetUnit.transform.position
        : targetPosition;

        // 도착처리
        if (!skill.projectile_targetMove &&
            Vector3.Distance(transform.position, destination) < 0.2f)
        {
            Destroy(gameObject);
        }
        if (skill.projectile_targetMove && targetPosEnd
            )
        {
            Destroy(gameObject, skill.skillTime);
        }

        Tracking(skill.tracking);


        if (!skill.projectile_targetMove)
        {

            // direction 지역 변수 선언 제거 → 필드 변수로 사용
            direction = (destination - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }

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

    // 순차적 목표추적형
    public IEnumerator TargetPosMove(List<string> list)
    {
        targetCount = 0;
        targetPos = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            targetPos.Add(TargetPos_Vector3(list[i]));
            Vector3 target = targetPos[i];

            // 방향/이동: easing 적용
            targetCount = i;
            yield return StartCoroutine(MoveToTargetWithEasing(target));

            if (skill.nextDelay <= 0) continue;
            yield return new WaitForSeconds(skill.nextDelay);
        }

        //반복
        for(int j = 0; j < skill.repeat; j++)
        {
            if (skill.rewind)
                targetPos.Reverse();

            for (int i = 0; i < targetPos.Count; i++)
            {
                Vector3 target = targetPos[i];

                // 방향/이동: easing 적용
                targetCount = i;
                yield return StartCoroutine(MoveToTargetWithEasing(target));

                if (skill.nextDelay <= 0) continue;
                yield return new WaitForSeconds(skill.nextDelay);
            }
        }
        targetPosEnd = true;
        yield break;
    }


    // 개별 목표로 easing 이동 (TargetPosMove에서 사용)
    private IEnumerator MoveToTargetWithEasing(Vector3 target)
    {
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, target);

        if (dist <= 0.001f)
        {
            transform.position = target;
            yield break;
        }

        float duration = Mathf.Max(0.0001f, dist / Mathf.Max(0.0001f, speed));
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, target) > 0.001f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = skill.easing ? skill.easingCurve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(start, target, eased);

            // 방향 업데이트(비주얼 회전용)
            direction = (target - transform.position).normalized;

            yield return null;
        }

        transform.position = target;
    }

    // 문자열 Vector3 변환 메서드
    public Vector3 TargetPos_Vector3(string pos)
    {
        float x = 0f, y = 0f;

        if (string.IsNullOrEmpty(pos))
            return new Vector3(x, 0, y);

        var parts = pos.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return new Vector3(x, 0, y);

        // 숫자 파싱 시 +1, -1 모두 파싱 가능
        bool parsedX = float.TryParse(parts[0], out float tx);
        bool parsedY = float.TryParse(parts[1], out float ty);

        // 상대값 여부 판단: 파트 중 하나가 '+' 또는 '-' 로 시작하면 상대값으로 간주
        bool part0Sign = parts[0].Length > 0 && (parts[0][0] == '+' || parts[0][0] == '-');
        bool part1Sign = parts[1].Length > 0 && (parts[1][0] == '+' || parts[1][0] == '-');
        bool isRelative = (part0Sign || part1Sign) && targetPos != null && targetPos.Count > 0;

        if (isRelative && parsedX && parsedY)
        {
            // targetPos의 마지막 좌표에 더하거나 빼서 결과 생성
            Vector3 last = targetPos[targetPos.Count - 1];
            x = last.x + tx;
            y = last.z + ty;
        }
        else if (parsedX && parsedY)
        {
            // 절대 좌표 처리
            x = tx;
            y = ty;
        }

        return new Vector3(x, 0, y);
    }

/*    public void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;
        if ((!skill.penetration || skill.skillTypes.Contains(skillType.movement)) && other.CompareTag("MapBorder"))
        {
            Destroy(gameObject);
        }
    }*/

    public void OnDestroy()
    {
        if (!isInitialized) return;
        OnHit();
    }

    public void OnHit()
    {
        if (!isInitialized) return;
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
        skillObject.GetComponent<SkillEffectProjectile>().enabled = false;
        skillObject.GetComponent<SkillEffectHitscan>().enabled = true;
        if (skillObject.TryGetComponent<SkillEffectHitscan>(out var effect))
            effect.Initialize(additionalSkillData, nearestTile, caster, casterStats, null);

    }
}
