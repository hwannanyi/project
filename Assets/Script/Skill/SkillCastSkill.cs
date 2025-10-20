using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TextCore.Text;

public class SkillCastSkill : MonoBehaviour
{
    public CastingSkillData CastSkillData; // 스킬 데이터 참조 필드 추가

    public SkillData skillData; // SkillData 참조 필드 추가
    public Vector3 targetPos;
    public GameObject caster; // 캐릭터 유닛 참조 필드 추가
    public Stats casterStats; // 캐릭터 스탯 참조 필드 추가
    public GameObject targetUnit; // 타겟 유닛 참조 필드 추가

    void Awake()
    {
        CastSkillData = GetComponent<CastingSkillData>();
    }

    void OnDestroy()
    {
        StartSkill(false, CastSkillData.characterStats, CastSkillData.skillData);
    }

    // 유닛 충돌 이벤트에서 호출
    public void StartSkill(bool StartTiming, Stats self, SkillData skillData)
    {
        var AddSkills = StartTiming ? skillData.StartAddSkills : skillData.EndAddSkills;

        if (AddSkills == null)
        {
            SkillManager.Instance.isCastingSkill = StartTiming && SkillManager.Instance.isCastingSkill; // 스킬 캐스트 스킬 초기화
            return; }
        

        SkillData Skill = AddSkills.skill != null ? AddSkills.skill : null;
        
        Condition condition = AddSkills.condition != null ? AddSkills.condition : null;
        ConditionHit conditionHit = AddSkills.conditionHit != null ? AddSkills.conditionHit : null;
        SkillAutoCast targetrule = AddSkills.targetrule != null ? AddSkills.targetrule : null;


        try
        {
            PassiveSkillCast.Instance.AutoCast(targetrule, self, Skill, gameObject);
        }
        catch
        {
            Debug.Log("연계스킬이 없음");
            SkillManager.Instance.isCastingSkill = StartTiming && SkillManager.Instance.isCastingSkill;
        }
        
            //}
        //}
    }

    public bool HitCondition(Stats self, Stats target2, SkillData skillData,
    Target target, TargetUnit targetUnit, Condition_Hit condition_Hit, string name)
    {
        // 조건
        var cond = ConditionBuilder.HitCondition(
            target,
            targetUnit,
            condition_Hit,
        name
        )
        .Build();

        return cond(self, target2, skillData, Team.team);
    }

    public bool IsEffectCondition(Stats targetStats, Stats casterStats, SkillData skillData,
    Target target, AttributeType type, Condition_statement comparison, float value)
    {
        var cond = ConditionBuilder
            .Attribute(target, type, comparison, value)
            .Build();

        return cond(casterStats, targetStats, skillData, Team.team);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
