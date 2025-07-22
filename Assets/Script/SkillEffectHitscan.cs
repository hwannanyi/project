using System;
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
    private Vector3 targetPosition;
    public GameObject targetUnit;
    public GameObject charcterUnit;

    public ColliderMerger colliderMerger; // 콜라이더 병합 컴포넌트

    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;

    private bool hasExitedPhase = false;

    public bool isInitialized = false;

    public SpriteRenderer spriteRenderer; // Inspector에서 할당하거나 GetComponent로 가져오기

    public SkillTiming skillTiming;
    private void Awake()
    {

    }

    public void Initialize(SkillData skill, Vector3 targetPos, GameObject charcter,
        Stats character, GameObject target = null)
    {
        skillData = skill;
        range = skill.range;
        rotatingVisual.rotation = Quaternion.Euler(90f, 0, 0f);
        hasExitedPhase = false;
        charcterUnit = charcter;
        colliderMerger = GetComponent<ColliderMerger>();

        // 외형 변경
        if (spriteRenderer != null && skill.SkillEffectIllustration != null)
        {
            spriteRenderer.sprite = skill.SkillEffectIllustration;
        }

        if (skill.targeting && target != null)
        {
            targetUnit = target;
            transform.position = target.transform.position;
            startPosition = target.transform.position;
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
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(90f, angle, 0f);
        }


/*        // 병합 실행
        ColliderMerger merger = GetComponent<ColliderMerger>();
        if (merger != null)
        {
            merger.MergeChildBoxColliders();
        }*/

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
            hit.Initialize(skill, charcter, character); // 또는 실제 캐릭터 GameObject
        }

        // 스킬 데이터 전송
        var castingSkillData = GetComponent<CastingSkillData>();
        if (castingSkillData != null)
        {
            castingSkillData.SetSkillData(
                skill,
                targetPos,
                charcter,
                character,
                target);
        }


        isInitialized = true;

        //colliderMerger.MergeChildBoxColliders();
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

        // 이동기이라면 시전자를 스킬 위치로 이동시킴
        if (skillData.skillTypes.Contains(skillType.movement))
        {
            var manager = CharacterStats.Instance;

            var casterStats = manager.GetStats(charcterUnit);
            // charcterUnit의 위치를 스킬 위치로 이동한뒤 위치 갱신
            if (charcterUnit != null)
            {
                charcterUnit.transform.position = transform.position;
                casterStats.charPosition = transform.position;
            }
        }

        Destroy(gameObject, 3); // 아주 짧게 남기기
    }

    void OnDestroy()
    {
        
        //임시코드 캐릭터 생성
        if (skillData.summonCharacter != null)
        {
            Stats summonCharacter = skillData.summonCharacter;
            var CharacterManager = CharacterStats.Instance;

            CharacterManager.EnemieCharacters.Add(summonCharacter.name);
            CharacterManager.characterList.Add(summonCharacter);

            GameObject SumCharacterObject = Instantiate(summonCharacter.characterPrefab);
            SumCharacterObject.transform.position = transform.position;
            CharacterManager.characters.Add(SumCharacterObject);
            summonCharacter.characterPrefab = SumCharacterObject;
            
            SumCharacterObject.name = summonCharacter.name;
        }

        if (!hasExitedPhase && !skillData.isreactSkill)
        {
            //TurnManager.Instance.ExitReactPhase(); //미사용, 스킬종료로 인해 대응단계가 종료되지 않도록 변경
            hasExitedPhase = true;
        }
    }

    /*    private Vector3 GetDirection()
        {
            Vector3 currentTarget = (skillData.targeting && targetUnit != null)
                ? targetUnit.transform.position
                : targetPosition;

            currentTarget.y = startPosition.y;
            return (currentTarget - startPosition).normalized;
        }

        private void RotateVisual(Vector3 dir)
        {
            if (rotatingVisual != null && dir.magnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
                rotatingVisual.rotation = Quaternion.Euler(90f, angle, 0f); // 보정 필요 시 -90f
            }
        }*/


}
