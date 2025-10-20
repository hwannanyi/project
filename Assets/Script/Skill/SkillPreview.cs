using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

public class SkillPreview : MonoBehaviour
{

    public static event Action<SFDType, float> OnSFDStart; // SFD 시작 이벤트
    public static event Action OnSFDEnd; // SFD 시작 이벤트

    public StoryManager storyManager;
    public bool isSFD = false;


    ////////////////////////////


    public GameObject PreviewTimer; // 스킬 프리뷰 타이머 오브젝트



    public GameObject skillPrefab;
    public GameObject skillPrefab2;

    public SkillData skill;
    public Vector3 targetPosition;
    public GameObject casterObj;
    public Stats caster;

    private GameObject targetObj; // 유도 타겟
    public float speed = 5f;


    public UnityEngine.Transform rotatingVisual;

    public Vector3 direction;
    public Vector3 aoeCenter; // AOE 중심 위치

    public GameObject trackingObject; //tracking이 참이라면 생성해 경로를 남김

    public bool CastLock = false;

    public int turns = 0;

    public AICastSkill aICastSkill;

    public Coroutine skillCoroutine = null;

    void OnEnable()
    {
        turns = 0;
        EventManager.Instance.isMove += Count;
        SkillManager.SkillCast += Count;
    }

    public void Initialize(
        SkillData skillData,
        Vector3 targetPos,
        GameObject casterObject,
        Stats character,
        Vector3 aoeCenterPosition,
        GameObject target = null
        )
    {
        skill = skillData;
        casterObj = casterObject;
        caster = character;
        aoeCenter = aoeCenterPosition;

        aICastSkill = casterObject.GetComponent<AICastSkill>();
        CastLock = aICastSkill.skillCastLock;


        if (skill.targeting && target != null)
        {
            targetObj = target;
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

/*        if (skill.skillPreviewStop && skill.SFDtype != SFDType.none)
        {
            /////////////////////////////
            storyManager = StoryManager.instance;
            TurnManager turnManager = TurnManager.Instance;
            StageDataManager stageDataManager = StageDataManager.Instance;
            isSFD = false;

            if (stageDataManager.CurrentStage.ID == "1" && storyManager.PopUptalkRead.Any(t => t.id == "2") && !isSFD)
            {
                Debug.Log("SFD 시작SFD 시작SFD 시작SFD 시작SFD 시작SFD 시작SFD 시작SFD 시작");    
                isSFD = true;
                OnSFDStart?.Invoke(skill.SFDtype, skill.SFDtime); // 이벤트 발생
                //StartCoroutine(skill.SFD(skill.SFDtype, skill.SFDtime));
            }
        }*/

        skillCoroutine = !skill.projectile ? StartCoroutine(StretchObjectToTarget(skill, targetPosition))
            :
            StartCoroutine(StretchObjectToTargetSlide(skill, targetPosition));
    }
    public IEnumerator StretchObjectToTargetSlide(SkillData skill, Vector3 targetPosition)
    {
        PreviewTimer.transform.localScale = new(1, 0, 0); // 초기 스케일 설정
        PreviewTimer.transform.position = new(0, -0.5f, 0); // 초기 스케일 설정
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
            skill.Xaoe : newScale.x + Mathf.FloorToInt(skill.Xaoe * 0.5f) * 2;

        newScale.y = skill.startSkillPosition == StartSkillPosition.mouse ?
            skill.Yaoe : distance + Mathf.FloorToInt(skill.Yaoe * 0.5f) * 2 + 1;
        transform.localScale = newScale;

        // 4. y축 회전각 계산 (수평 방향만)
        Vector3 dir = end - start;
        float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // 5. x축 90도 고정, y축만 회전
        transform.rotation = Quaternion.Euler(90f, yAngle, 0f);

        float elapsed = 0f;
        Vector3 startScale = PreviewTimer.transform.localScale;
        float targetY = 1f; // 목표 y 스케일


        while (TurnManager.Instance.isTurn_cooperation && turns < skill.skillPreviewCount)
        {
            yield return null; // 다음 프레임까지 대기
        }

        while (elapsed < skill.skillPreview)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / skill.skillPreview);
            float newY = Mathf.Lerp(startScale.y, targetY, t);
            float newPosY = Mathf.Lerp(-0.5f, 0, t);
            PreviewTimer.transform.localScale = new Vector3(startScale.x, newY, startScale.z);
            PreviewTimer.transform.localPosition = new Vector3(0, newPosY, 0);

            yield return null; // 다음 프레임까지 대기
        }

        caster.SetAnimation(skill.animationName);
        GameObject skillObject = Instantiate(skill.SkillEffectPrefab, aoeCenter, Quaternion.identity);

        if (skill.projectile)
        {
            skillObject.GetComponent<SkillEffectProjectile>().enabled = true;
            skillObject.GetComponent<SkillEffectHitscan>().enabled = false;
            if (skillObject.TryGetComponent<SkillEffectProjectile>(out var effect))
                effect.Initialize(skill, targetPosition, casterObj, caster, targetObj);
        }
        else
        {
            skillObject.GetComponent<SkillEffectProjectile>().enabled = false;
            skillObject.GetComponent<SkillEffectHitscan>().enabled = true;
            if (skillObject.TryGetComponent<SkillEffectHitscan>(out var effect))
                effect.Initialize(skill, targetPosition, casterObj, caster, targetObj);
        }

        Destroy(gameObject); // 스킬 프리뷰 오브젝트 제거
    }
    public IEnumerator StretchObjectToTarget(SkillData skill,Vector3 targetPosition)
    {
        PreviewTimer.transform.localScale = new(0.1f, 0.1f, 0); // 초기 스케일 설정
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
            skill.Xaoe : newScale.x + Mathf.FloorToInt(skill.Xaoe * 0.5f) * 2;

        newScale.y = skill.startSkillPosition == StartSkillPosition.mouse ?
            skill.Yaoe : distance + Mathf.FloorToInt(skill.Yaoe * 0.5f) * 2 + 1;
        transform.localScale = newScale;

        // 4. y축 회전각 계산 (수평 방향만)
        Vector3 dir = end - start;
        float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // 5. x축 90도 고정, y축만 회전
        transform.rotation = Quaternion.Euler(90f, yAngle, 0f);
        
        float elapsed = 0f;
        Vector3 startScale = PreviewTimer.transform.localScale;
        Vector3 targetScale = transform.localScale;

        while (TurnManager.Instance.isTurn_cooperation && turns < skill.skillPreviewCount)
        {
            yield return null; // 다음 프레임까지 대기
        }

        while (elapsed < skill.skillPreview)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / skill.skillPreview);
            PreviewTimer.transform.localScale = Vector3.Lerp(startScale, new Vector3(1,1,0), t);
            yield return null; // 다음 프레임까지 대기
        }
        caster.SetAnimation(skill.animationName);
        GameObject skillObject = Instantiate(skill.SkillEffectPrefab, aoeCenter, Quaternion.identity);

        if (skill.projectile)
        {
            if (skillObject.TryGetComponent<SkillEffectProjectile>(out var effect))
                effect.Initialize(skill, targetPosition, casterObj, caster, targetObj);
        }
        else
        {
            if (skillObject.TryGetComponent<SkillEffectHitscan>(out var effect))
                effect.Initialize(skill, targetPosition, casterObj, caster, targetObj);
        }

        Destroy(gameObject); // 스킬 프리뷰 오브젝트 제거
    }
    public void OnDestroy()
    {

        EventManager.Instance.isMove -= Count;
        SkillManager.SkillCast -= Count;
        if (skill.skillPreviewStop)
        {
            if (skill.SFDtype == SFDType.none)
                return;
            OnSFDEnd?.Invoke(); // 이벤트 발생
            isSFD = false;
        }
    }

    public void Count() { turns++; }
}
