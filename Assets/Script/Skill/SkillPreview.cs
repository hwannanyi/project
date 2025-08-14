using System.Collections.Generic;
using UnityEngine;

public class SkillPreview : MonoBehaviour
{
    public GameObject skillPrefab;
    public GameObject skillPrefab2;

    public SkillData skill;
    public Vector3 targetPosition;
    public GameObject caster;
    public Stats casterStats;

    private GameObject targetUnit; // 유도 타겟
    public float speed = 5f;


    public UnityEngine.Transform rotatingVisual;

    public Vector3 direction;

    public GameObject trackingObject; //tracking이 참이라면 생성해 경로를 남김

    public bool CastLock = false;

    public AICastSkill aICastSkill;


    public void Initialize(
        SkillData skillData,
        Vector3 targetPos,
        GameObject casterObject,
        Stats character,
        GameObject target = null)
    {
        skill = skillData;
        caster = casterObject;
        casterStats = character;


        aICastSkill = casterObject.GetComponent<AICastSkill>();
        CastLock = aICastSkill.skillCastLock;


        if (skill.targeting && target != null)
        {
            targetUnit = target;
            targetPosition = target.transform.position;
        }
        else
        {
            targetPosition = targetPos;
        }

        // 오브젝트 자체를 x축 90도 회전
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        rotatingVisual.rotation = Quaternion.Euler(90f, 0, 0f);

        // 초기 방향 계산
        direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero && rotatingVisual != null)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(90f, angle, 0f);
        }

        StretchObjectToTarget(skill, targetPosition);
    }

    public void StretchObjectToTarget(SkillData skill,Vector3 targetPosition)
    {

        Vector3 start = transform.position;
        Vector3 end = targetPosition;
        
        // 1. 두 점의 중간 위치로 이동
        Vector3 center = (start + end) * 0.5f;
        transform.position = skill.startSkillPosition == StartSkillPosition.mouse ? targetPosition : center;

        // 2. 거리 계산 (y축을 따라 늘릴 길이)
        float distance = Vector3.Distance(start, end);

        // 3. 오브젝트의 크기(Scale) 조정 (y축을 길이로)
        Vector3 newScale = transform.localScale;
        newScale.x = skill.startSkillPosition == StartSkillPosition.mouse ?
            skill.Xaoe : newScale.x + Mathf.FloorToInt(skill.Xaoe * 0.5f)*2;

        newScale.y = skill.startSkillPosition == StartSkillPosition.mouse ?
            skill.Yaoe :distance + Mathf.FloorToInt(skill.Yaoe * 0.5f)*2 + 1;
        transform.localScale = newScale;

        // 4. y축 회전각 계산 (수평 방향만)
        Vector3 dir = end - start;
        float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // 5. x축 90도 고정, y축만 회전
        transform.rotation = Quaternion.Euler(90f, yAngle, 0f);

/*        // 6. 바라보는 방향으로 -0.5만큼 이동
        float rad = yAngle * Mathf.Deg2Rad;
        Vector3 moveDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        transform.position -= moveDir * 0.5f;*/
    }
}
